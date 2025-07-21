using Domain.Enums;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class TaskPollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<TaskPollingService> _logger;

        public TaskPollingService(IServiceScopeFactory scopeFactory, IBackgroundJobClient backgroundJobClient, ILogger<TaskPollingService> logger)
        {
            _scopeFactory = scopeFactory;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Torrent Task Polling Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var downloadContext = scope.ServiceProvider.GetRequiredService<DownloadContext>();

                    var pendingTasks = downloadContext.TorrentTasks
                        .Where(t => t.State == TorrentTaskState.Pending)
                        .OrderByDescending(t=> t.Priority)
                        .ToList();

                    foreach (var task in pendingTasks)
                    {
                        // Enqueue background job
                        _backgroundJobClient.Enqueue<TorrentProcessTask>(p => p.ProcessTorrentTaskAsync(task.Id));
                        // Optionally update status to avoid reprocessing
                        
                    }

                    await downloadContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while polling tasks");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Adjustable polling interval
            }
        }
    }

}