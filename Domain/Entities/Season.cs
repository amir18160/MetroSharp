namespace Domain.Entities
{
    public class Season
    {
        public Guid Id { get; set; }
        public int SeasonNumber { get; set; }

        /**********************/
        /***** Relations ******/
        /**********************/
        public Guid OmdbItemId { get; set; }
        public OmdbItem OmdbItem { get; set; }
        
        public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
    }
}