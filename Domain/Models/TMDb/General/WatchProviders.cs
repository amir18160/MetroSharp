

namespace Domain.Models.TMDb.General
{
    public class WatchProviders
    {
        public string Link { get; set; }
        public List<WatchProviderItem> FlatRate { get; set; }
        public List<WatchProviderItem> Rent { get; set; }
        public List<WatchProviderItem> Buy { get; set; }
        public List<WatchProviderItem> Free { get; set; }
        public List<WatchProviderItem> Ads { get; set; }
    }
}
