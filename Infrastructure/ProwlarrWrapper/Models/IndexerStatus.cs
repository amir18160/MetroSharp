namespace Infrastructure.ProwlarrWrapper.Models
{
    public class IndexerStatus
    {
        public int Id { get; set; }
        public int IndexerId { get; set; }
        public DateTime DisabledTill { get; set; }
        public DateTime MostRecentFailure { get; set; }
        public DateTime InitialFailure { get; set; }
    }

}