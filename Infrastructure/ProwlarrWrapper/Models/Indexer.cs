namespace Infrastructure.ProwlarrWrapper.Models
{
    public class Indexer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Field> Fields { get; set; }
        public string ImplementationName { get; set; }
        public string Implementation { get; set; }
        public string ConfigContract { get; set; }
        public string InfoLink { get; set; }
        public IndexerMessage Message { get; set; }
        public List<int> Tags { get; set; }
        public List<string> Presets { get; set; }
        public List<string> IndexerUrls { get; set; }
        public List<string> LegacyUrls { get; set; }
        public string DefinitionName { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string Encoding { get; set; }
        public bool Enable { get; set; }
        public bool Redirect { get; set; }
        public bool SupportsRss { get; set; }
        public bool SupportsSearch { get; set; }
        public bool SupportsRedirect { get; set; }
        public bool SupportsPagination { get; set; }
        public int AppProfileId { get; set; }
        public string Protocol { get; set; }
        public string Privacy { get; set; }
        public Capabilities Capabilities { get; set; }
        public int Priority { get; set; }
        public int DownloadClientId { get; set; }
        public DateTime Added { get; set; }
        public IndexerStatus Status { get; set; }
        public string SortName { get; set; }
    }

}