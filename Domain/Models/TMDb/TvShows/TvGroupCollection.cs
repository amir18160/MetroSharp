

namespace Domain.Models.TMDb.TvShows
{
    public class TvGroupCollection
    {
        
        public string Id { get; set; }

        
        public string Name { get; set; }

        
        public TvGroupType Type { get; set; }

        
        public string Description { get; set; }

        
        public NetworkWithLogo Network { get; set; }

        
        public int EpisodeCount { get; set; }

        
        public int GroupCount { get; set; }

        
        public List<TvGroup> Groups { get; set; }
    }
}