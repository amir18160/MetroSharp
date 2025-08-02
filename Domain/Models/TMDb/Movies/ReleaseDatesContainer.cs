using System.Collections.Generic;


namespace Domain.Models.TMDb.Movies
{
    public class ReleaseDatesContainer
    {

        
        public string Iso_3166_1 { get; set; }

        
        public List<ReleaseDateItem> ReleaseDates { get; set; }
    }
}