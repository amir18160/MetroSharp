namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class TorrentTaskSettings
    {
        public bool IsEnabled { get; set; }
        public bool IsPollerEnabled { get; set; }
        public long MinimumRequiredSpace { get; set; }
        public int MaxAllowedDownloadTimeInMinutes { get; set; }
        public int MaxAllowedTimeToGetMetaData { get; set; }
        public string SubtitleDownloadPath { get; set; }
        public string SubtitleEditPath { get; set; }
        public string FFmpegPath { get; set; }

    }
}