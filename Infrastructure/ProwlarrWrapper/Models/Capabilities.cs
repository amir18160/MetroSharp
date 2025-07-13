namespace Infrastructure.ProwlarrWrapper.Models
{
    public class Capabilities
    {
        public int Id { get; set; }
        public int LimitsMax { get; set; }
        public int LimitsDefault { get; set; }
        public List<CapabilityCategory> Categories { get; set; }
        public bool SupportsRawSearch { get; set; }
        public List<string> SearchParams { get; set; }
        public List<string> TvSearchParams { get; set; }
        public List<string> MovieSearchParams { get; set; }
        public List<string> MusicSearchParams { get; set; }
        public List<string> BookSearchParams { get; set; }
    }

}