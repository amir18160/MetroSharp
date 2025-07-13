using System.Collections.ObjectModel;

namespace Domain.Entities
{
    public class Episode
    {
        public Guid Id { get; set; }
        public int EpisodeNumber { get; set; }

        /**********************/
        /***** Relations ******/
        /**********************/

        public Guid SeasonId { get; set; }
        public Season Season { get; set; }
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}