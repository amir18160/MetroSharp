namespace Application.Core
{
    public class DateFilter
    {
        public DateTime? GreaterThan { get; set; }
        public DateTime? GreaterThanOrEqual { get; set; }
        public DateTime? LessThan { get; set; }
        public DateTime? LessThanOrEqual { get; set; }
        public DateTime? Equal { get; set; }
    }
}