namespace Infrastructure.QbitTorrentClient.Models
{
    public class QbitTorrentSettings
    {
        public Uri ConnectionString { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DownloadFolder { get; set; }
    }
}