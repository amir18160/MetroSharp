using Domain.Models.TMDb.People;

namespace Domain.Models.TMDb.Movies
{
    public class Cast
    {
        public int CastId { get; set; }

        public string Character { get; set; }

        public string CreditId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public string ProfilePath { get; set; }

        public PersonGender Gender { get; set; }

        public bool Adult { get; set; }
        public string KnownForDepartment { get; set; }

        public string OriginalName { get; set; }

        public float Popularity { get; set; }
    }
}