using Hangfire;
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
        
        
        [AutomaticRetry(Attempts = 0)]
        public async Task ProcessTorrentTaskAsync(Guid torrentTaskId, CancellationToken cancellationToken)
        {
            Console.WriteLine($"running task with id {torrentTaskId}" );
            using var scope = _serviceProvider.CreateScope();
            var conditionChecker = scope.ServiceProvider.GetRequiredService<TorrentTaskStartConditionChecker>();
            var checkAndGenerateOMDbData = scope.ServiceProvider.GetRequiredService<CheckAndGenerateOMDbData>();
            var startTorrentTaskDownload = scope.ServiceProvider.GetRequiredService<StartTorrentTaskDownload>();
            var extractSubtitleForTorrent = scope.ServiceProvider.GetRequiredService<ExtractSubtitleForTorrent>();
            var pairSubtitleWithVideos = scope.ServiceProvider.GetRequiredService<PairSubtitleWithVideos>();
            var ffmpegTaskProcessor = scope.ServiceProvider.GetRequiredService<FFmpegTaskProcessor>();
            var subtitleEditor = scope.ServiceProvider.GetRequiredService<SubtitleEditor>();
            var fileUploadProcessor = scope.ServiceProvider.GetRequiredService<FileUploadProcessor>();
            var taskCleaner = scope.ServiceProvider.GetRequiredService<TaskCleaner>();
            
            // Register a cancellation callback that enqueues the TaskCleaner into the dedicated "cleaners" queue.
            // This is a best-effort backup if the job is cancelled or the process crashes.
            
            try
            {
                /***************************************/
                /**** Get Task And Check Conditions ****/
                /***************************************/
                if (!await conditionChecker.CheckAsync(torrentTaskId, cancellationToken)) return;

                /***************************************/
                /********** Get OMDb Data  *************/
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
                Console.WriteLine("cancellation was requested");
                throw;
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
            }
        }
    }
}
