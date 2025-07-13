namespace Infrastructure.ProwlarrWrapper.Models
{
    public class SearchOptions
    {
        public string Query { get; set; }
        public string Type { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<int> IndexerIds { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
    }
}