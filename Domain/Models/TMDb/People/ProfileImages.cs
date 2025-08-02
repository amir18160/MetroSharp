
using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.People
{
    public class ProfileImages
    {
        
        public int Id { get; set; }

        
        public List<ImageData> Profiles { get; set; }
    }
}