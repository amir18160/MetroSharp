namespace Application.DocumentUsages.DTOs;

public class DocumentUsageStats
{
    public UsageSummary Last24HoursUsage { get; set; }
    public UsageSummary Last7DaysUsage { get; set; }
    public UsageSummary Last30DaysUsage { get; set; }
    public UsageSummary Last90DaysUsage { get; set; }
    public UsageSummary LastYearUsage { get; set; }
    public UsageSummary TotalUsages { get; set; }
}