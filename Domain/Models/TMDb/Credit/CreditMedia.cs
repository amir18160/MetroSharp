
namespace Domain.Models.TMDb.Credit
{
    public class CreditMedia
    {
        public string Character { get; set; }

        public List<CreditEpisode> Episodes { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string OriginalName { get; set; }

        public List<CreditSeason> Seasons { get; set; }
    }
}