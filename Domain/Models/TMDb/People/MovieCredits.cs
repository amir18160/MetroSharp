using System.Collections.Generic;


namespace Domain.Models.TMDb.People
{
    public class MovieCredits
    {
        
        public List<MovieRole> Cast { get; set; }

        
        public List<MovieJob> Crew { get; set; }

        
        public int Id { get; set; }
    }
}