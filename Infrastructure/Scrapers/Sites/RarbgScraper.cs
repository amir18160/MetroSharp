using AngleSharp.Dom;
using Domain.Models.Scrapers.Rarbg;
using Infrastructure.Scrapers.Core;
using System.Text.RegularExpressions;

namespace Infrastructure.Scrapers.Sites
{
    public class RarbgScraper : ScraperBase
    {
        private static readonly string _baseUrl = "https://therarbg.com";

        public static async Task<List<RarbgPreview>> SearchMoviesAsync(string imdbId)
        {
            var url = $"{_baseUrl}/get-posts/keywords:{imdbId}/";
            var document = await AngleSharpGetDocumentAsync(url);
            return ParseMovieTable(document);
        }

        public static async Task<List<RarbgPreview>> GetLatestMoviesAsync(int page = 1)
        {
            var url = page > 1
                ? $"{_baseUrl}/get-posts/category:Movies:time:10D/?page={page}"
                : $"{_baseUrl}/get-posts/category:Movies:time:10D/";

            var document = await AngleSharpGetDocumentAsync(url);
            return ParseMovieTable(document);
        }

        public static async Task<RarbgDetails> GetTitleDetailsByHrefAsync(string href)
        {
            var document = await AngleSharpGetDocumentAsync(href);
            var result = new RarbgDetails();

            var GetText = (string label) =>
                document.QuerySelector($"th:contains(\"{label}\") + td")?.TextContent.Trim();

            var GetHref = (string label) =>
                document.QuerySelector($"th:contains(\"{label}\") + td a[href]")?.GetAttribute("href");

            result.Torrent = GetHref("Torrent:");
            result.MagnetLink = document.QuerySelector("a[href^=\"magnet\"]")?.GetAttribute("href");
            result.Thumbnail = document.QuerySelector("th:contains(\"Thumbnail:\") + td img")?.GetAttribute("src");
            result.Trailer = document.QuerySelector("th:contains(\"Trailer:\") + td iframe")?.GetAttribute("src");
            result.Uploader = GetText("Uploader:");
            result.Downloads = GetText("Downloads:");
            result.Type = GetText("Type:");
            result.Genre = GetText("Genre:")?.Split(',').Select(g => g.Trim()).ToList() ?? new();
            result.InfoHash = GetText("Info Hash:");
            result.Language = GetText("Language:");
            result.Description = GetText("Description:");
            result.Category = document.QuerySelector("th:contains(\"Category:\") + td a")?.TextContent;
            result.Size = GetText("Size:");
            result.Added = GetText("Added:");
            result.MultipleQualityAvailable = !string.IsNullOrWhiteSpace(GetText("Multiple Quality Available:"));

            var peers = GetText("Peers:");
            result.Peers = new RarbgPeerInfo
            {
                Seeders = Regex.Match(peers ?? "", @"Seeders:\s*(\d+)").Groups[1].Value,
                Leechers = Regex.Match(peers ?? "", @"Leechers:\s*(\d+)").Groups[1].Value
            };

            result.Imdb = new RarbgImdbInfo
            {
                Link = GetHref("IMDB"),
                Title = GetText("IMDB Title"),
                Genres = document.QuerySelectorAll("th:contains(\"IMDB Genre\") + td a")
                    .Select(a => a.TextContent.Trim()).ToList(),
                Runtime = GetText("IMDB Runtime"),
                Rating = GetText("IMDB Rating"),
                Director = GetText("Director"),
                Cast = GetText("IMDB cast"),
                Plot = GetText("IMDB plot")
            };
            return result;
        }

        public static async Task<string> GetMostSuitedMagnetAsync(string imdbId)
        {
            var results = await SearchMoviesAsync(imdbId);
            if (results.Count == 0) return null;

            var filtered = results
                .Select(m =>
                {
                    double sizeMB = m.Size.ToLower().Contains("gb")
                        ? double.Parse(Regex.Match(m.Size, @"[\d.]+").Value) * 1024
                        : double.Parse(Regex.Match(m.Size, @"[\d.]+").Value);

                    return (Movie: m, Size: sizeMB);
                })
                .Where(x => x.Size < 2048)
                .OrderBy(x => x.Size)
                .Select(x => x.Movie)
                .ToList();

            return filtered.FirstOrDefault()?.MagnetLink;
        }

        public static  async Task<List<RarbgPreview>> GetTorrentsByImdbIdAsync(string imdbId)
        {
            try
            {
                return await SearchMoviesAsync(imdbId);
            }
            catch
            {
                return null;
            }
        }

        private  static List<RarbgPreview> ParseMovieTable(IDocument document)
        {
            return document.QuerySelectorAll("tbody > tr").Select(row =>
            {
                var titleAnchor = row.QuerySelector(".cellName a");
                var categoryAnchor = row.QuerySelector("td:nth-child(3) a");

                return new RarbgPreview
                {
                    Title = titleAnchor?.TextContent.Trim(),
                    TitleHref = _baseUrl + titleAnchor?.GetAttribute("href"),
                    Category = categoryAnchor?.TextContent.Trim(),
                    CategoryHref = _baseUrl + categoryAnchor?.GetAttribute("href"),
                    Date = row.QuerySelector("td:nth-child(4) div")?.TextContent.Trim(),
                    Size = row.QuerySelector(".sizeCell")?.TextContent.Trim(),
                    Seeders = int.TryParse(row.QuerySelectorAll("td").ElementAtOrDefault(6)?.TextContent.Trim(), out var s) ? s : 0,
                    Leechers = int.TryParse(row.QuerySelectorAll("td").ElementAtOrDefault(7)?.TextContent.Trim(), out var l) ? l : 0
                };
            }).Where(x => !string.IsNullOrEmpty(x.Title)).ToList();
        }
    }
}
