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
        InExtractingSubtitle,
        InParingSubtitlesWithVideo,
        InFfmpegButProcessNotStarted,
        InFfmpegAndProcessStarted,
        InUploaderButUploadingNotStarted,
        InUploaderAndUploadingStarted,
        Completed,
        Error,
        Cancelling,
        Cancelled
    }
}