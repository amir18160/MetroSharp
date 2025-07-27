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

        public async Task ProcessTorrentTaskAsync(Guid torrentTaskId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var conditionChecker = scope.ServiceProvider.GetRequiredService<TorrentTaskStartConditionChecker>();
                var checkAndGenerateOMDbData = scope.ServiceProvider.GetRequiredService<CheckAndGenerateOMDbData>();
                var startTorrentTaskDownload = scope.ServiceProvider.GetRequiredService<StartTorrentTaskDownload>();
                var downloadSubtitleForTorrent = scope.ServiceProvider.GetRequiredService<DownloadSubtitleForTorrent>();
                var pairSubtitleWithVideos = scope.ServiceProvider.GetRequiredService<PairSubtitleWithVideos>();
                var ffmpegTaskProcessor = scope.ServiceProvider.GetRequiredService<FFmpegTaskProcessor>();
                var subtitleEditor = scope.ServiceProvider.GetRequiredService<SubtitleEditor>();
                var fileUploadProcessor = scope.ServiceProvider.GetRequiredService<FileUploadProcessor>();
                var taskCleaner = scope.ServiceProvider.GetRequiredService<TaskCleaner>();

                try
                {
                    /***************************************/
                    /**** Get Task And Check Conditions ****/
                    /***************************************/
                    if (!await conditionChecker.CheckAsync(torrentTaskId)) return;

                    /***************************************/
                    /********** Get Omdb Data  *************/
                    /***************************************/
                    if (!await checkAndGenerateOMDbData.Checker(torrentTaskId)) return;

                    /***************************************/
                    /********** Start Download *************/
                    /***************************************/
                    if (!await startTorrentTaskDownload.ExecuteAsync(torrentTaskId)) return;

                    /***************************************/
                    /********** Download Subtitle **********/
                    /***************************************/
                    await downloadSubtitleForTorrent.GetSubtitleForTorrent(torrentTaskId);

                    /***************************************/
                    /******* pair torrent with videos ******/
                    /***************************************/
                    if (!await pairSubtitleWithVideos.Pair(torrentTaskId)) return;

                    /***************************************/
                    /********* Edit Subtitles Process ******/
                    /***************************************/
                    await subtitleEditor.EditSubtitle(torrentTaskId);

                    /***************************************/
                    /********** Start FFmpeg Process *******/
                    /***************************************/
                    if (!await ffmpegTaskProcessor.ProcessPairs(torrentTaskId)) return;

                    /***************************************/
                    /********** Upload To Telegram   *******/
                    /***************************************/
                    if (!await fileUploadProcessor.StartUploadProcess(torrentTaskId)) return;
                }
                finally
                {
                    /***************************************/
                    /************** Cleanup ****************/
                    /***************************************/
                    await taskCleaner.CleanUpAsync(torrentTaskId);
                }
            }
        }
    }
}
