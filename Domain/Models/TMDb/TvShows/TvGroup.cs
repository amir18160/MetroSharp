

namespace Domain.Models.TMDb.TvShows
{
    public class TvGroup
    {
        
        public string Id { get; set; }

        
        public string Name { get; set; }

        
        public int Order { get; set; }

        
        public List<TvGroupEpisode> Episodes { get; set; }

        
        public bool Locked { get; set; }
    }
}