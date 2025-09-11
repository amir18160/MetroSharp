using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.BackgroundServices.TorrentProcessTask;

// Use IJob and the JobExecutionContext.CancellationToken (Quartz 3.x pattern)
public class TorrentTaskJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TorrentTaskJob> _logger;

    public TorrentTaskJob(IServiceProvider serviceProvider, ILogger<TorrentTaskJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var idString = context.MergedJobDataMap.GetString("torrentTaskId") ??
                       context.JobDetail.JobDataMap.GetString("torrentTaskId");

        if (!Guid.TryParse(idString, out var torrentTaskId))
        {
            _logger.LogError("TorrentTaskJob invoked without valid torrentTaskId.");
            return;
        }

        // Quartz will cancel this token when an interruption is requested (Scheduler.Interrupt)
        var ct = context.CancellationToken;

        // Register a cancellation callback to run TaskCleaner as a best-effort backup.
        // Use Task.Run inside Register to avoid deadlocking the calling thread if CleanUpAsync is async.
        ct.Register(() =>
        {
            _logger.LogInformation(
                "Cancellation requested for torrent task {TaskId} — launching best-effort cleanup callback.",
                torrentTaskId);

            Task.Run(async () =>
            {
                try
                {
                    using var cleanupScope = _serviceProvider.CreateScope();
                    var cleaner = cleanupScope.ServiceProvider.GetRequiredService<TaskCleaner>();
                    // Do not pass the job token here — use CancellationToken.None for reliable cleanup persistence
                    await cleaner.CleanUpAsync(torrentTaskId, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cleanup callback failed for task {TaskId}", torrentTaskId);
                }
            });
        });

        try
        {
            _logger.LogInformation("Running torrent task job {TaskId}", torrentTaskId);

            using var scope = _serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<TorrentTaskProcessor>();

            // Pass the context token into your processor. Your existing code will observe cancellationToken.ThrowIfCancellationRequested()
            await processor.ProcessTorrentTaskAsync(torrentTaskId, ct);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Torrent task {TaskId} was cancelled via scheduler interruption.", torrentTaskId);
            // rethrow so Quartz can record the job as cancelled/failed if you want it to.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception running torrent task {TaskId}", torrentTaskId);
            throw;
        }
    }
}