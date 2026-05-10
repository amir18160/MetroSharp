using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence; // your DataContext namespace
using Slugify;

namespace Infrastructure.BackgroundServices.SitemapGeneratorService
{

    public class SitemapGeneratorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<SitemapGeneratorService> _logger;
        private readonly SitemapSettings _options;

        public SitemapGeneratorService(
            IServiceProvider serviceProvider,
            IWebHostEnvironment env,
            ILogger<SitemapGeneratorService> logger,
            IOptions<SitemapSettings> options)
        {
            _serviceProvider = serviceProvider;
            _env = env;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.IsEnabled)
            {
                _logger.LogInformation("SitemapGeneratorService is disabled.");
                return;
            }

            var period = TimeSpan.FromDays(Math.Max(1, _options.RunIntervalDays));

            // initial run immediately
            try
            {
                _logger.LogInformation("SitemapGeneratorService starting initial run.");
                await GenerateSitemapsAsync(stoppingToken);
                _logger.LogInformation("SitemapGeneratorService initial run complete.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Initial sitemap generation failed.");
            }

            using var timer = new PeriodicTimer(period);
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    try
                    {
                        _logger.LogInformation("SitemapGeneratorService scheduled run starting.");
                        await GenerateSitemapsAsync(stoppingToken);
                        _logger.LogInformation("SitemapGeneratorService scheduled run complete.");
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("SitemapGeneratorService canceled.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Sitemap generation failed during scheduled run.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // shutting down
            }
            finally
            {
                _logger.LogInformation("SitemapGeneratorService stopping.");
            }
        }

        private async Task GenerateSitemapsAsync(CancellationToken cancellationToken)
        {
            // Ensure output folder is under wwwroot so it is publicly served
            var outputFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, _options.OutputSubfolder);
            Directory.CreateDirectory(outputFolder);

            var parts = new List<string>();
            var page = 0;
            var pageSize = Math.Clamp(_options.PageSize, 100, _options.MaxUrlsPerSitemap);

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataContext>(); // <-- replace with your actual DbContext

            var slugHelper = new SlugHelper();
            var defaultDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // We'll add a set of static pages (home, genres, about-us) to the very first part
            var staticPaths = new[]
            {
                "/", "/genres", "/about-us"
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                var skip = page * pageSize;

                // Pull only simple scalar fields from DB to avoid translator issues.
                // Replace property names to match your OmdbItems schema
                var rawItems = await db.OmdbItems
                    .AsNoTracking()
                    .OrderByDescending(e => e.Id)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(e => new
                    {
                        e.Title,
                        Year = (int?)e.Year, // if Year is nullable
                        e.ImdbId,
                        e.CreatedAt
                    })
                    .ToListAsync(cancellationToken);

                if (rawItems.Count == 0) break;

                // Build stable in-memory DTOs with correct lastmod and loc
                var items = rawItems.Select(e =>
                {
                    var last = e.CreatedAt.Year != 1 ?  e.CreatedAt : defaultDate;
                    // ensure UTC
                    if (last.Kind == DateTimeKind.Unspecified) last = DateTime.SpecifyKind(last, DateTimeKind.Utc);
                    last = last.ToUniversalTime();

                    var yearPart = e.Year.HasValue ? $" {e.Year.Value}" : string.Empty;
                    var slugSource = $"{e.Title}-{yearPart}";
                    var slug = slugHelper.GenerateSlug(slugSource);

                    var loc = BuildLoc(_options.BaseUrl, "/movies", slug, e.ImdbId);

                    return new
                    {
                        Loc = loc,
                        LastMod = last
                    };
                }).ToList();

                page++;

                // create filenames (plain xml)
                var partFileName = $"sitemap-part-{page}.xml";
                var partTempName = Path.Combine(outputFolder, partFileName + ".tmp");
                var partFinalPath = Path.Combine(outputFolder, partFileName);

                // Write XML (plain, no gzip) and include namespace attributes and elements you requested.
                var xmlSettings = new XmlWriterSettings
                {
                    Async = true,
                    Encoding = Encoding.UTF8,
                    Indent = false
                };

                await using (var fs = new FileStream(partTempName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                await using (var xw = XmlWriter.Create(fs, xmlSettings))
                {
                    await xw.WriteStartDocumentAsync();

                    // <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:news="..." ...>
                    xw.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                    await xw.WriteAttributeStringAsync("xmlns", "news", null, "http://www.google.com/schemas/sitemap-news/0.9");
                    await xw.WriteAttributeStringAsync("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");
                    await xw.WriteAttributeStringAsync("xmlns", "mobile", null, "http://www.google.com/schemas/sitemap-mobile/1.0");
                    await xw.WriteAttributeStringAsync("xmlns", "image", null, "http://www.google.com/schemas/sitemap-image/1.1");
                    await xw.WriteAttributeStringAsync("xmlns", "video", null, "http://www.google.com/schemas/sitemap-video/1.1");

                    // If this is the first part, include static pages first
                    if (page == 1 && staticPaths?.Length > 0)
                    {
                        foreach (var path in staticPaths)
                        {
                            await WriteUrlAsync(xw, $"{_options.BaseUrl}{path}", DateTime.UtcNow, "daily", "0.7");
                        }
                    }

                    // write dynamic items
                    foreach (var it in items)
                    {
                        await WriteUrlAsync(xw, it.Loc, it.LastMod, "daily", "0.7");
                    }

                    await xw.WriteEndElementAsync(); // urlset
                    await xw.WriteEndDocumentAsync();
                    await xw.FlushAsync();
                }

                // Atomic replace
                if (File.Exists(partFinalPath)) File.Delete(partFinalPath);
                File.Move(partTempName, partFinalPath);

                _logger.LogInformation("Wrote sitemap part: {file}", partFinalPath);

                // record public URL (used by sitemap-index)
                var publicUrl = $"{_options.BaseUrl}/{_options.OutputSubfolder}/{partFileName}";
                parts.Add(publicUrl);

                if (items.Count < pageSize) break; // last page
            }

            // write sitemap-index.xml
            var indexTempPath = Path.Combine(outputFolder, "sitemap-index.xml.tmp");
            var indexFinalPath = Path.Combine(outputFolder, "sitemap-index.xml");

            var idxSettings = new XmlWriterSettings
            {
                Async = true,
                Encoding = Encoding.UTF8,
                Indent = true
            };

            await using (var fs = new FileStream(indexTempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            await using (var xw = XmlWriter.Create(fs, idxSettings))
            {
                await xw.WriteStartDocumentAsync();
                xw.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");

                foreach (var p in parts)
                {
                    xw.WriteStartElement("sitemap");
                    xw.WriteElementString("loc", p);
                    // optional lastmod for each sitemap part - set to now UTC
                    xw.WriteElementString("lastmod", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                    await xw.WriteEndElementAsync(); // sitemap
                }

                await xw.WriteEndElementAsync(); // sitemapindex
                await xw.WriteEndDocumentAsync();
                await xw.FlushAsync();
            }

            if (File.Exists(indexFinalPath)) File.Delete(indexFinalPath);
            File.Move(indexTempPath, indexFinalPath);

            _logger.LogInformation("Wrote sitemap index: {file}", indexFinalPath);

            // optional cleanup of stale parts
            if (_options.DeleteStaleFiles)
            {
                var existing = Directory.EnumerateFiles(outputFolder, "sitemap-part-*.xml")
                    .Select(Path.GetFullPath)
                    .ToList();

                var keep = new HashSet<string>(parts.Select(p =>
                {
                    var localPath = new Uri(p).LocalPath; // e.g. /sitemaps/sitemap-part-1.xml
                    return Path.GetFullPath(Path.Combine(outputFolder, Path.GetFileName(localPath)));
                }));

                foreach (var f in existing.Where(f => !keep.Contains(f)))
                {
                    try
                    {
                        File.Delete(f);
                        _logger.LogInformation("Deleted stale sitemap part: {f}", f);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete stale sitemap file {f}", f);
                    }
                }
            }
        }

        private static async Task WriteUrlAsync(XmlWriter xw, string loc, DateTime lastmodUtc, string changefreq, string priority)
        {
            xw.WriteStartElement("url");
            xw.WriteElementString("loc", loc);

            // format lastmod with milliseconds and Z
            xw.WriteElementString("lastmod", lastmodUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

            // optional fields for SEO
            xw.WriteElementString("changefreq", changefreq);
            xw.WriteElementString("priority", priority);

            await xw.WriteEndElementAsync(); // url
        }

        private static string BuildLoc(string baseUrl, string routePrefix, string slug, string imdbId)
        {
            // ensure baseUrl has no trailing slash
            baseUrl = baseUrl?.TrimEnd('/') ?? string.Empty;
            routePrefix = routePrefix?.TrimEnd('/') ?? string.Empty;
            // encode query param - you can URL-encode imdbId if needed
            var imdbQuery = string.IsNullOrWhiteSpace(imdbId) ? "" : $"?imdbId={Uri.EscapeDataString(imdbId)}";
            return $"{baseUrl}{routePrefix}/{slug}{imdbQuery}";
        }
    }
}
