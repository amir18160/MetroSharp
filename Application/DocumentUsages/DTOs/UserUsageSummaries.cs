namespace Application.DocumentUsages.DTOs;

public class UserUsageSummaries
{
    public UsageSummary TodayUsage { get; set; }
    public UsageSummary Last7Days { get; set; }
    public UsageSummary Last30Days { get; set; }
}