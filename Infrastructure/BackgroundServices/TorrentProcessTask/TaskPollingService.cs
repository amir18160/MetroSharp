using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence;
using Quartz;
using Quartz.Impl.Matchers;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class TaskPollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<TaskPollingService> _logger;
        private readonly TorrentTaskSettings _settings;

        public TaskPollingService(
            IServiceScopeFactory scopeFactory,
            ISchedulerFactory schedulerFactory,
            ILogger<TaskPollingService> logger,
            IOptions<TorrentTaskSettings> settings)
        {
            _scopeFactory = scopeFactory;
            _schedulerFactory = schedulerFactory;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.IsPollerEnabled)
            {
                _logger.LogInformation("Torrent poller disabled by settings.");
                return;
            }

            // Obtain scheduler instance lazily (avoid depending on IScheduler in ctor)
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            _logger.LogInformation("Clearing Quartz scheduler jobs at startup (best-effort).");
            try
            {
                await PurgeAllJobsAsync(scheduler, stoppingToken);
                _logger.LogInformation("Quartz scheduler cleared.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to purge Quartz scheduler at startup.");
            }

            _logger.LogInformation("Torrent Task Polling Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    try
                    {
                        var downloadContext = scope.ServiceProvider.GetRequiredService<DownloadContext>();

                        var pendingTasks = await downloadContext.TorrentTasks
                            .Where(t => t.State == TorrentTaskState.Pending)
                            .OrderByDescending(t => t.Priority)
                            .Take(10)
                            .ToListAsync(stoppingToken);

                        foreach (var task in pendingTasks)
                        {
                            try
                            {
                                // Respect cancellation
                                stoppingToken.ThrowIfCancellationRequested();

                                task.State = TorrentTaskState.JobQueue;
                                var result = await downloadContext.SaveChangesAsync(stoppingToken) > 0;
                                if (!result)
                                {
                                    _logger.LogError("Torrent task with id {TaskId} failed to poll.", task.Id);
                                    continue;
                                }

                                _logger.LogInformation("Scheduling task with id {TaskId} on Quartz.", task.Id);

                                var jobKey = new JobKey($"torrent-{task.Id}", "torrent-jobs");

                                var jobDetail = JobBuilder.Create<TorrentTaskJob>()
                                    .WithIdentity(jobKey)
                                    .UsingJobData("torrentTaskId", task.Id.ToString())
                                    .Build();

                                var trigger = TriggerBuilder.Create()
                                    .WithIdentity($"trigger-torrent-{task.Id}", "torrent-triggers")
                                    .StartNow()
                                    .Build();

                                await scheduler.ScheduleJob(jobDetail, trigger, stoppingToken);
                            }
                            catch (OperationCanceledException)
                            {
                                _logger.LogInformation("Task polling loop cancelled while scheduling tasks.");
                                break;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to schedule task {taskId}", task.Id);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Torrent Task Polling Service is stopping...");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while polling tasks");
                    }
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // graceful shutdown
                }
            }

            _logger.LogInformation("Torrent Task Polling Service stopped.");
        }

        private static async Task PurgeAllJobsAsync(IScheduler scheduler, CancellationToken cancellationToken)
        {
            if (scheduler == null) return;

            var jobGroups = await scheduler.GetJobGroupNames(cancellationToken);
            foreach (var group in jobGroups)
            {
                var keys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group), cancellationToken);
                if (keys != null && keys.Count > 0)
                {
                    await scheduler.DeleteJobs(keys.ToList(), cancellationToken);
                }
            }

            var triggerGroups = await scheduler.GetTriggerGroupNames(cancellationToken);
            foreach (var tg in triggerGroups)
            {
                var triggers = await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(tg), cancellationToken);
                foreach (var triggerKey in triggers)
                {
                    try
                    {
                        await scheduler.UnscheduleJob(triggerKey, cancellationToken);
                    }
                    catch
                    {
                        // best-effort
                    }
                }
            }
        }
    }
}
