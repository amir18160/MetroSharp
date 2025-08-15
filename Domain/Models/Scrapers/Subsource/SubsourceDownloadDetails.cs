using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Models.Scrapers.Subsource
{
    public class SubsourceDownloadDetails
    {
        public SubsourceSubtitleDetails Subtitle { get; set; }
    }

    public class SubsourceSubtitleDetails
    {
        [JsonPropertyName("download_token")]
        public string DownloadToken { get; set; }
    }
}