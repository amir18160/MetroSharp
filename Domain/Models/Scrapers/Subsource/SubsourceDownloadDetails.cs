using System.Text.Json.Serialization;

namespace Domain.Models.Scrapers.Subsource;

public class SubsourceDownloadDetails
{
    public SubsourceSubtitleDetails Subtitle { get; set; }
}

public class SubsourceSubtitleDetails
{
    [JsonPropertyName("download_token")] public string DownloadToken { get; set; }
}