namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class TorrentTaskProcessor
    {
        private readonly TorrentTaskStartConditionChecker _conditionChecker;
        private readonly CheckAndGenerateOMDbData _checkAndGenerateOMDbData;
        private readonly StartTorrentTaskDownload _startTorrentTaskDownload;
        private readonly DownloadSubtitleForTorrent _downloadSubtitleForTorrent;
        private readonly PairSubtitleWithVideos _pairSubtitleWithVideos;
        private readonly FFmpegTaskProcessor _ffmpegTaskProcessor;
        private readonly SubtitleEditor _subtitleEditor;


        public TorrentTaskProcessor
        (
            SubtitleEditor subtitleEditor,
            FFmpegTaskProcessor ffmpegTaskProcessor,
            PairSubtitleWithVideos pairSubtitleWithVideos,
            DownloadSubtitleForTorrent downloadSubtitleForTorrent,
            StartTorrentTaskDownload startTorrentTaskDownload,
            CheckAndGenerateOMDbData checkAndGenerateOMDbData,
            TorrentTaskStartConditionChecker conditionChecker
        )
        {
            _subtitleEditor = subtitleEditor;
            _ffmpegTaskProcessor = ffmpegTaskProcessor;
            _pairSubtitleWithVideos = pairSubtitleWithVideos;
            _downloadSubtitleForTorrent = downloadSubtitleForTorrent;
            _startTorrentTaskDownload = startTorrentTaskDownload;
            _checkAndGenerateOMDbData = checkAndGenerateOMDbData;
            _conditionChecker = conditionChecker;
        }
        public async Task ProcessTorrentTaskAsync(Guid torrentTaskId)
        {
            /***************************************/
            /**** Get Task And Check Conditions ****/
            /***************************************/

            if (!await _conditionChecker.CheckAsync(torrentTaskId))
            {
                return;
            }

            /***************************************/
            /********** Get Omdb Data  *************/
            /***************************************/

            if (!await _checkAndGenerateOMDbData.Checker(torrentTaskId))
            {
                return;
            }

            /***************************************/
            /********** Start Download *************/
            /***************************************/

            if (!await _startTorrentTaskDownload.ExecuteAsync(torrentTaskId))
            {
                return;
            }

            /***************************************/
            /********** Download Subtitle **********/
            /***************************************/

            await _downloadSubtitleForTorrent.GetSubtitleForTorrent(torrentTaskId);

            /***************************************/
            /******* pair torrent with videos ******/
            /***************************************/

            if (!await _pairSubtitleWithVideos.Pair(torrentTaskId))
            {
                return;
            }

            /***************************************/
            /********* Edit Subtitles Process ******/
            /***************************************/

            await _subtitleEditor.EditSubtitle(torrentTaskId);

            /***************************************/
            /********** Start FFmpeg Process *******/
            /***************************************/

            if (!await _ffmpegTaskProcessor.ProcessPairs(torrentTaskId))
            {
                return;
            }

            /***************************************/
            /********** Upload To Telegram   *******/
            /***************************************/


            /***************************************/
            /******* Add Document to Database ******/
            /***************************************/

        }
    }
}
