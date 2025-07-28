using Domain.Enums;

namespace Domain.Entities
{
    public class TorrentTask
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string TorrentTitle { get; set; }
        public TorrentTaskState State { get; set; }
        public string ErrorMessage { get; set; }
        public string TorrentHash { get; set; }
        public string Magnet { get; set; }
        public string FileSavingPath { get; set; }
        public TorrentTaskType TaskType { get; set; }
        public TorrentTaskPriority Priority { get; set; }
        public string SubtitleUrl { get; set; }
        public string SubtitleSavingPath { get; set; }
        public string ImdbId { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public string Regex { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? DownloadStartTime { get; set; }
        public DateTime? DownloadEndTime { get; set; }
        public string UserId { get; set; }
        public int? FfmpegPID { get; set; }

        public TaskDownloadProgress TaskDownloadProgress { get; set; }
        public ICollection<TaskUploadProgress> TaskUploadProgress { get; set; } = new List<TaskUploadProgress>();
        public ICollection<SubtitleVideoPair> SubtitleVideoPairs { get; set; } = new List<SubtitleVideoPair>();

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}