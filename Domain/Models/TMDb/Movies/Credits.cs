using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.Movies
{
    public class Credits
    {
        
        public List<Cast> Cast { get; set; }

      
        public List<Crew> Crew { get; set; }

      
        public int Id { get; set; }
    }
}