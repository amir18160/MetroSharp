using Domain.Enums;
using Hangfire;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class TaskPollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<TaskPollingService> _logger;
        private readonly TorrentTaskSettings _settings;

        public TaskPollingService(
            IServiceScopeFactory scopeFactory,
            IBackgroundJobClient backgroundJobClient,
            ILogger<TaskPollingService> logger,
            IOptions<TorrentTaskSettings> settings)
        {
            _settings = settings.Value;
            _scopeFactory = scopeFactory;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.IsPollerEnabled)
            {
                return;
            }

            _logger.LogInformation("Purge Hangfire database");
            PurgeAllJobs();
            _logger.LogInformation("Hangfire Database was purged successfully");

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
                                task.State = TorrentTaskState.JobQueue;
                                var result = await downloadContext.SaveChangesAsync(stoppingToken) > 0;
                                if (!result)
                                {
                                    _logger.LogError("Torrent task with id {TaskId} failed to poll.", task.Id);
                                    continue;
                                }

                                _logger.LogInformation($"Adding task with {task.Id} to poller.");
                                _backgroundJobClient.Enqueue<TorrentTaskProcessor>(p =>
                                    p.ProcessTorrentTaskAsync(task.Id, stoppingToken)
                                );
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to enqueue task {taskId}", task.Id);
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
                }
            }
        }


        public static void PurgeAllJobs()
        {
            using var connection = JobStorage.Current.GetConnection();
            foreach (var recurringJob in connection.GetRecurringJobs())
            {
                RecurringJob.RemoveIfExists(recurringJob.Id);
            }

            var monitoring = JobStorage.Current.GetMonitoringApi();
            foreach (var queue in monitoring.Queues())
            {
                var toDelete = monitoring.EnqueuedJobs(queue.Name, 0, (int)queue.Length);
                foreach (var job in toDelete)
                {
                    BackgroundJob.Delete(job.Key);
                }
            }

            foreach (var job in monitoring.ScheduledJobs(0, int.MaxValue))
                BackgroundJob.Delete(job.Key);

            foreach (var job in monitoring.ProcessingJobs(0, int.MaxValue))
                BackgroundJob.Delete(job.Key);

            foreach (var job in monitoring.SucceededJobs(0, int.MaxValue))
                BackgroundJob.Delete(job.Key);

            foreach (var job in monitoring.FailedJobs(0, int.MaxValue))
                BackgroundJob.Delete(job.Key);
        }
    }
}