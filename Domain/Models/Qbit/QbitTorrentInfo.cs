namespace Domain.Models.Qbit
{
    public class QbitTorrentInfo
    {
        public string Hash { get; set; }
        public string Name { get; set; }
        public string MagnetUri { get; set; }
        public long Size { get; set; }
        public double Progress { get; set; }
        public int DownloadSpeed { get; set; }
        public int UploadSpeed { get; set; }
        public int Priority { get; set; }
        public int ConnectedSeeds { get; set; }
        public int TotalSeeds { get; set; }
        public int ConnectedLeechers { get; set; }
        public int TotalLeechers { get; set; }
        public double Ratio { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
        public QbitTorrentState State { get; set; }
        public bool SequentialDownload { get; set; }
        public bool FirstLastPiecePrioritized { get; set; }
        public string Category { get; set; }
        public IReadOnlyCollection<string> Tags { get; set; }
        public bool SuperSeeding { get; set; }
        public bool ForceStart { get; set; }
        public string SavePath { get; set; }
        public DateTime? AddedOn { get; set; }
        public DateTime? CompletionOn { get; set; }
        public string CurrentTracker { get; set; }
        public int? DownloadLimit { get; set; }
        public int? UploadLimit { get; set; }
        public long? Downloaded { get; set; }
        public long? Uploaded { get; set; }
        public long? DownloadedInSession { get; set; }
        public long? UploadedInSession { get; set; }
        public long? IncompletedSize { get; set; }
        public long? CompletedSize { get; set; }
        public double RatioLimit { get; set; }
        public DateTime? LastSeenComplete { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public TimeSpan? ActiveTime { get; set; }
        public bool AutomaticTorrentManagement { get; set; }
        public long? TotalSize { get; set; }
        public TimeSpan? SeedingTime { get; set; }
        public string ContentPath { get; set; }
    }
}
