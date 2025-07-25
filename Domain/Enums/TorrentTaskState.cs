using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        Error,
    }
}