namespace Domain.Models.Scrapers.Subsource;

public class SubsourceSearchResultResponse
{
    public bool Success { get; set; }
    public List<SubsourceSearchApiResultItem> Results { get; set; }
}