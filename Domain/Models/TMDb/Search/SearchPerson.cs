using Domain.Models.TMDb.General;
namespace Domain.Models.TMDb.Search
{
    public class SearchPerson : SearchBase
    {
        public SearchPerson()
        {
            MediaType = MediaType.Person;
        }


        public bool Adult { get; set; }


        public List<KnownForBase> KnownFor { get; set; }


        public string Name { get; set; }


        public string ProfilePath { get; set; }
    }
}