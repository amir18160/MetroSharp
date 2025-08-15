using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Models.Scrapers.Subsource
{
    public class SubsourceSearchApiResultItem
    {
        public int Id { set; get; }
        public string Title { set; get; }
        public string Type { set; get; }
        public string Link { set; get; }
        public int ReleaseYear { get; set; }
        public string Poster { set; get; }
        public List<Season> Seasons { get; set; }
    }


    public class Season
    {
        [JsonPropertyName("season")]
        public int Number { get; set; }
        public int Id { get; set; }
        public string Link { get; set; }
    }
}