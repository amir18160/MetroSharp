using System.Text.Json.Serialization;

namespace Domain.Models.Scrapers.Subsource
{
    public class SubsourceSubtitlesList
    {
        [JsonPropertyName("media_type")]
        public string MediaType { get; set; }

        [JsonPropertyName("subtitles")]
        public List<SubtitleResultListItem> Subtitles { get; set; }

    }

    public class SubtitleResultListItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("release_type")]
        public string ReleaseType { get; set; }

        [JsonPropertyName("release_info")]
        public string ReleaseInfo { get; set; }

        [JsonPropertyName("upload_date")]
        public DateTime UploadDate { get; set; }

        [JsonPropertyName("caption")]
        public string Caption { get; set; }

        [JsonPropertyName("rating")]
        public string Rating { get; set; }

        [JsonPropertyName("uploader_displayname")]
        public string UploaderDisplayName { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("production_type")]
        public string ProductionType { get; set; }
    }
}