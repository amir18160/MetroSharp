namespace Domain.Entities
{
    public class TaskUploadProgress
    {
        public string FileName { get; set; }
        public double Progress { get; set; }
        public long Read { get; set; }
        public long Total { get; set; }
        public TorrentTask TorrentTask { set; get; }
        public Guid TorrentTaskId { set; get; }
    }
}