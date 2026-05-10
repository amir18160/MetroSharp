/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

export enum TorrentTaskState {
    Pending = 0,
    JobQueue = 1,
    JobStarted = 2,
    InGettingOmdbDetailsProcess = 3,
    InQbitButDownloadNotStarted = 4,
    TorrentTimedOut = 5,
    InQbitAndDownloadStarted = 6,
    TorrentWasDownloaded = 7,
    InExtractingSubtitle = 8,
    InParingSubtitlesWithVideo = 9,
    InFfmpegButProcessNotStarted = 10,
    InFfmpegAndProcessStarted = 11,
    InUploaderButUploadingNotStarted = 12,
    InUploaderAndUploadingStarted = 13,
    Completed = 14,
    Error = 15,
    Cancelling = 16,
    Cancelled = 17,
}
