using System.Collections.Generic;

using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.TvShows
{
    public class Credits
    {
        
        public List<Cast> Cast { get; set; }

        
        public List<Crew> Crew { get; set; }

        
        public int Id { get; set; }
    }
}