namespace Domain.Entities
{
    public class SubtitleVideoPair
    {
        public Guid Id { get; set; }
        public string VideoPath { get; set; }
        public string SubtitlePath { get; set; }
        public string FinalPath { get; set; }
        public bool Ignore { get; set; }
        public Guid TorrentTaskId { get; set; }
        public TorrentTask TorrentTask { get; set; }
        public bool IsMovie { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
    }
}
