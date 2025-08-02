
using Domain.Models.TMDb.Search;
using Domain.Models.TMDb.People;

namespace Domain.Models.TMDb.Find
{
    public class FindPerson : SearchPerson
    {
      
        public PersonGender Gender { get; set; }

      
        public string KnownForDepartment { get; set; }
    }
}