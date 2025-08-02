using System.Collections.Generic;

using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.TvShows
{
    public class NetworkLogos
    {
        
        public int Id { get; set; }

        
        public List<ImageData> Logos { get; set; }
    }
}