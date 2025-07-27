using Domain.Enums;

namespace Application.Tasks.Queries.GetTasks
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string TorrentTitle { get; set; }
        public string State { get; set; }
        public string ErrorMessage { get; set; }
        public string TorrentHash { get; set; }
        public string ImdbId { get; set; }
        public string TaskType { get; set; }
        public string Priority { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        
        public string UserId { get; set; }
        public string UserName { get; set; }

       
        public decimal DownloadProgress { get; set; }
        public long DownloadSize { get; set; }
        public int DownloadSpeed { get; set; }
        public List<UploadProgressDto> UploadProgress { get; set; } = new List<UploadProgressDto>();
    }

    public class UploadProgressDto
    {
        public string FileName { get; set; }
        public double Progress { get; set; }
        public long Total { get; set; }
    }
}
