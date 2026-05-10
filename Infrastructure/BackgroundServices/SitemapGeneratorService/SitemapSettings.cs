namespace Infrastructure.BackgroundServices.SitemapGeneratorService;

public class SitemapSettings
{
    public bool IsEnabled { get; set; }
    public string OutputSubfolder { get; set; }
    public string BaseUrl { get; set; }
    public int PageSize { get; set; }
    public int RunIntervalDays { get; set; }
    public int MaxUrlsPerSitemap { get; set; }
    public bool DeleteStaleFiles { get; set; }
}