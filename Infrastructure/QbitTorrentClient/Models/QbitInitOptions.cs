namespace Infrastructure.QbitTorrentClient.Models;

public class QbitTorrentSettings
{
    public bool IsEnabled { get; set; }
    public Uri ConnectionString { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string DownloadFolder { get; set; }
}