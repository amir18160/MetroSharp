
using Domain.Models.TMDb.People;

namespace Domain.Models.TMDb.General
{
    public class CrewBase
    {

        public string Department { get; set; }


        public int Id { get; set; }


        public string Name { get; set; }

     
        public string ProfilePath { get; set; }

        public PersonGender Gender { get; set; }


        public bool Adult { get; set; }

        
        public string KnownForDepartment { get; set; }

        public string OriginalName { get; set; }

 
        public float Popularity { get; set; }
    }
}