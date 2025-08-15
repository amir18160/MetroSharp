using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class TorrentTaskProcessor
    {
        private readonly IServiceProvider _serviceProvider;

        public TorrentTaskProcessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Let Hangfire inject the CancellationToken (it will when you delete the job or when server cancels)
        [AutomaticRetry(Attempts = 0)]
        public async Task ProcessTorrentTaskAsync(Guid torrentTaskId, CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var conditionChecker = scope.ServiceProvider.GetRequiredService<TorrentTaskStartConditionChecker>();
                var checkAndGenerateOMDbData = scope.ServiceProvider.GetRequiredService<CheckAndGenerateOMDbData>();
                var startTorrentTaskDownload = scope.ServiceProvider.GetRequiredService<StartTorrentTaskDownload>();
                var extractSubtitleForTorrent = scope.ServiceProvider.GetRequiredService<ExtractSubtitleForTorrent>();
                var pairSubtitleWithVideos = scope.ServiceProvider.GetRequiredService<PairSubtitleWithVideos>();
                var ffmpegTaskProcessor = scope.ServiceProvider.GetRequiredService<FFmpegTaskProcessor>();
                var subtitleEditor = scope.ServiceProvider.GetRequiredService<SubtitleEditor>();
                var fileUploadProcessor = scope.ServiceProvider.GetRequiredService<FileUploadProcessor>();
                var taskCleaner = scope.ServiceProvider.GetRequiredService<TaskCleaner>();
                var backgroundJobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

                // Register a cancellation callback that enqueues the TaskCleaner into the dedicated "cleaners" queue.
                // This is a best-effort backup if the job is cancelled or the process crashes.
                CancellationTokenRegistration registration = default;
                try
                {
                    registration = cancellationToken.Register(() =>
                    {
                        try
                        {
                            var job = Job.FromExpression<TaskCleaner>(c => c.CleanUpAsync(torrentTaskId, CancellationToken.None));
                            backgroundJobClient.Create(job, new EnqueuedState("cleaners"));
                        }
                        catch
                        {
                            // swallow — this is only a backup attempt
                        }
                    });

                    try
                    {
                        /***************************************/
                        /**** Get Task And Check Conditions ****/
                        /***************************************/
                        if (!await conditionChecker.CheckAsync(torrentTaskId, cancellationToken)) return;

                        /***************************************/
                        /********** Get Omdb Data  *************/
                        /***************************************/
                        if (!await checkAndGenerateOMDbData.Checker(torrentTaskId, cancellationToken)) return;

                        /***************************************/
                        /********** Start Download *************/
                        /***************************************/
                        if (!await startTorrentTaskDownload.ExecuteAsync(torrentTaskId, cancellationToken)) return;

                        /***************************************/
                        /********** Download Subtitle **********/
                        /***************************************/
                        if (!await extractSubtitleForTorrent.GetSubtitleForTorrent(torrentTaskId, cancellationToken)) return;

                        /***************************************/
                        /******* pair torrent with videos ******/
                        /***************************************/
                        if (!await pairSubtitleWithVideos.Pair(torrentTaskId, cancellationToken)) return;

                        /***************************************/
                        /********* Edit Subtitles Process ******/
                        /***************************************/
                        await subtitleEditor.EditSubtitle(torrentTaskId, cancellationToken);

                        /***************************************/
                        /********** Start FFmpeg Process *******/
                        /***************************************/
                        if (!await ffmpegTaskProcessor.ProcessPairs(torrentTaskId, cancellationToken)) return;

                        /***************************************/
                        /********** Upload To Telegram   *******/
                        /***************************************/
                        if (!await fileUploadProcessor.StartUploadProcess(torrentTaskId, cancellationToken)) return;
                    }
                    catch (OperationCanceledException)
                    {
                        // Let the finally block and the cancellation callback handle cleanup/enqueueing.
                        throw;
                    }
                }
                finally
                {
                    // Ensure cleanup runs. Use CancellationToken.None so cleaner's critical work isn't cancelled by the job token.
                    try
                    {
                        await taskCleaner.CleanUpAsync(torrentTaskId, CancellationToken.None);
                    }
                    catch
                    {
                        // best-effort — don't throw from finally
                    }

                    // Dispose the registration to avoid leaking resources.
                    registration.Dispose();
                }
            }
        }
    }
}
