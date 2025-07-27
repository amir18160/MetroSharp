namespace Domain.Enums
{
    public enum TorrentTaskState
    {
        Pending,
        JobQueue,
        JobStarted,
        InGettingOmdbDetailsProcess,
        InQbitButDownloadNotStarted,
        TorrentTimedOut,
        InQbitAndDownloadStarted,
        TorrentWasDownloaded,
        InDownloadingSubtitle,
        InParingSubtitlesWithVideo,
        InFfmpegButProcessNotStarted,
        InFfmpegAndProcessStarted,
        InUploaderButUploadingNotStarted,
        InUploaderAndUploadingStarted,
        Completed,
        Error,
    }
}