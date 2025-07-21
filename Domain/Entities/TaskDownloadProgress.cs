namespace Domain.Entities
{
    public class TaskDownloadProgress
    {
        public Guid Id { get; set; }
        public decimal Progress { get; set; }
        public int Size { get; set; }
        public int Speed { get; set; }

        public Guid TorrentTaskId { get; set; }
        public TorrentTask TorrentTask { get; set; }
    }
}