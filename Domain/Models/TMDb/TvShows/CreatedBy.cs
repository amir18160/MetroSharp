
using Domain.Models.TMDb.People;

namespace Domain.Models.TMDb.TvShows
{
    public class CreatedBy
    {
        
        public int Id { get; set; }

        
        public string CreditId { get; set; }

        
        public string Name { get; set; }

        
        public PersonGender Gender { get; set; }

        
        public string ProfilePath { get; set; }
    }
}