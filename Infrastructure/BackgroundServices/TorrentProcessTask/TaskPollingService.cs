using Domain.Enums;
using Hangfire;
using Infrastructure.BackgroundServices.Models;
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

        public TaskPollingService(IServiceScopeFactory scopeFactory, IBackgroundJobClient backgroundJobClient, ILogger<TaskPollingService> logger, IOptions<TorrentTaskSettings> settings)
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

            _logger.LogInformation("Torrent Task Polling Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
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
                            _backgroundJobClient.Enqueue<TorrentTaskProcessor>(p => p.ProcessTorrentTaskAsync(task.Id));
                            task.State = TorrentTaskState.JobQueue;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to enqueue task {taskId}", task.Id);
                        }
                    }

                    await downloadContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while polling tasks");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}