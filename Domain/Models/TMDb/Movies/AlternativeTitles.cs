using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.Movies
{
    public class AlternativeTitles
    {
        public int Id { get; set; }
        public List<AlternativeTitle> Titles { get; set; }
    }
}