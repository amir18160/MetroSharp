using AngleSharp.Dom;
using Infrastructure.Scrapers.Core;
using Infrastructure.Scrapers.Models.X1337;

namespace Infrastructure.Scrapers.Sites
{
    public class X1337Scraper : ScraperBase
    {
        private static readonly string _baseUrl = "https://1337x.to";
        private static readonly List<string> _altBaseUrls = ["https://www.1337xx.to", "https://1337xto.to"];

        private static async Task<string> FetchHtmlAsync(string path)
        {
            foreach (var baseUrl in new[] { _baseUrl }.Concat(_altBaseUrls))
            {
                var fullUrl = path.Replace(_baseUrl, baseUrl);
                try
                {
                    var document = await AngleSharpGetDocumentAsync(fullUrl);
                    return document.DocumentElement.OuterHtml;
                }
                catch { continue; }
            }

            throw new Exception("Failed to fetch HTML. All base URLs failed.");
        }

        private static List<X1337Preview> ParseMovieTable(IDocument document)
        {
            var movies = new List<X1337Preview>();
            var rows = document.QuerySelectorAll("tbody tr");

            foreach (var row in rows)
            {
                try
                {
                    var name = row.QuerySelector(".coll-1")?.TextContent.Trim();
                    var refLink = row.QuerySelector(".coll-1 a:nth-of-type(2)")?.GetAttribute("href");
                    var seeds = row.QuerySelector(".coll-2")?.TextContent.Trim();
                    var leeches = row.QuerySelector(".coll-3")?.TextContent.Trim();

                    var fileSize = row.QuerySelector(".coll-4")?.ChildNodes
                        .FirstOrDefault(n => n.NodeType == NodeType.Text)?.TextContent.Trim();

                    if (name != null && refLink != null)
                    {
                        movies.Add(new X1337Preview
                        {
                            Name = name,
                            RefLink = $"{_baseUrl}{refLink}",
                            Seeds = seeds,
                            Leeches = leeches,
                            FileSize = fileSize
                        });
                    }
                }
                catch { continue; }
            }

            return movies;
        }

        public static async Task<List<X1337Preview>> GetMoviesFromTableAsync(string subUrl)
        {
            var html = await FetchHtmlAsync($"{_baseUrl}{subUrl}");
            var document = await AngleSharpGetDocumentAsync($"{_baseUrl}{subUrl}");
            return ParseMovieTable(document);
        }

        public static Task<List<X1337Preview>> GetTop100MoviesAsync() =>
            GetMoviesFromTableAsync("/top-100-movies");

        public static Task<List<X1337Preview>> GetPopularTodayAsync() =>
            GetMoviesFromTableAsync("/popular-movies");

        public static Task<List<X1337Preview>> GetPopularThisWeekAsync() =>
            GetMoviesFromTableAsync("/popular-movies-week");

        public static Task<List<X1337Preview>> SearchMoviesAsync(string query)
        {
            var formatted = query.Replace(" ", "%20");
            return GetMoviesFromTableAsync($"/category-search/{formatted}/Movies/1/");
        }

        public static Task<List<X1337Preview>> GetAllMoviesAsync(int page = 1) =>
            GetMoviesFromTableAsync($"/cat/Movies/{page}/");

        public static async Task<X1337Details> GetDetailsAsync(string refLink)
        {
            var document = await AngleSharpGetDocumentAsync(refLink);
            var nameSlug = refLink.Split('/').Reverse().Skip(1).FirstOrDefault();
            var name = nameSlug?.Replace("-", ".").Trim();

            var seeds = document.QuerySelector(".seeds")?.TextContent?.Trim();
            var leeches = document.QuerySelector(".leeches")?.TextContent?.Trim();

            var imdb = document.QuerySelector("a[href*='imdb']")?.TextContent?.Trim();
            var imageSrc = document.QuerySelector(".torrent-image img")?.GetAttribute("src");
            var image = string.IsNullOrWhiteSpace(imageSrc) ? null : $"https:{imageSrc}";

            var size = document.QuerySelector("strong:contains('Total size')")?.NextElementSibling?.TextContent?.Trim();

            var magnet = document.QuerySelector("a[href^='magnet:']")?.GetAttribute("href");

            return new X1337Details
            {
                Name = name,
                Seeds = seeds,
                Leeches = leeches,
                Magnet = magnet,
                Image = image,
                ImdbRef = imdb,
                Size = size
            };
        }
    }
}