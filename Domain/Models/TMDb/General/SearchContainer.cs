
namespace Domain.Models.TMDb.General
{
    public class SearchContainer<T>
    {

        public int Page { get; set; }


        public List<T> Results { get; set; }

        public int TotalPages { get; set; }

        public int TotalResults { get; set; }
    }
}