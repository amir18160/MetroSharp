using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Domain.Models.Qbit
{

    public enum QbitTorrentState
    {
    [EnumMember(Value = "unknown")]
    Unknown,
    //
    // Summary:
    //     Some error occurred, applies to paused torrents
    [EnumMember(Value = "error")]
    Error,
    //
    // Summary:
    //     Torrent is paused and has finished downloading
    [EnumMember(Value = "pausedUP")]
    PausedUpload,
    //
    // Summary:
    //     Torrent is paused and has NOT finished downloading
    [EnumMember(Value = "pausedDL")]
    PausedDownload,
    //
    // Summary:
    //     Queuing is enabled and torrent is queued for upload
    [EnumMember(Value = "queuedUP")]
    QueuedUpload,
    //
    // Summary:
    //     Queuing is enabled and torrent is queued for download
    [EnumMember(Value = "queuedDL")]
    QueuedDownload,
    //
    // Summary:
    //     Torrent is being seeded and data is being transferred
    [EnumMember(Value = "uploading")]
    Uploading,
    //
    // Summary:
    //     Torrent is being seeded, but no connection were made
    [EnumMember(Value = "stalledUP")]
    StalledUpload,
    //
    // Summary:
    //     Torrent has finished downloading and is being checked; this status also applies
    //     to preallocation (if enabled) and checking resume data on qBt startup
    [EnumMember(Value = "checkingUP")]
    CheckingUpload,
    //
    // Summary:
    //     Torrent is being checked
    [EnumMember(Value = "checkingDL")]
    CheckingDownload,
    //
    // Summary:
    //     Torrent is being downloaded and data is being transferred
    [EnumMember(Value = "downloading")]
    Downloading,
    //
    // Summary:
    //     Torrent is being downloaded, but no connection were made
    [EnumMember(Value = "stalledDL")]
    StalledDownload,
    //
    // Summary:
    //     Torrent has just started downloading and is fetching metadata
    [EnumMember(Value = "metaDL")]
    FetchingMetadata,
    //
    // Summary:
    //     Torrent has just started downloading and is fetching metadata
    [EnumMember(Value = "forcedMetaDL")]
    ForcedFetchingMetadata,
    [EnumMember(Value = "forcedUP")]
    ForcedUpload,
    [EnumMember(Value = "forcedDL")]
    ForcedDownload,
    //
    // Summary:
    //     The files are missing
    [EnumMember(Value = "missingFiles")]
    MissingFiles,
    //
    // Summary:
    //     Allocating space on disk
    [EnumMember(Value = "allocating")]
    Allocating,
    //
    // Summary:
    //     Queued for checking
    [EnumMember(Value = "queuedForChecking")]
    QueuedForChecking,
    //
    // Summary:
    //     Resume data is being checked
    [EnumMember(Value = "checkingResumeData")]
    CheckingResumeData,
    //
    // Summary:
    //     Data is being moved from the temporary folder
    [EnumMember(Value = "moving")]
    Moving
    }
}
