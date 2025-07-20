using Domain.Models.Scrapers.ScreenRent;
using Infrastructure.Scrapers.Core;
using System.Text.RegularExpressions;

namespace Infrastructure.Scrapers.Sites
{
    public class ScreenRantScraper : ScraperBase
    {
        private static readonly string _baseUrl = "https://www.screenrant.com";
        private static readonly string _newsUrl = $"{_baseUrl}/archive/";

        public static async Task<List<ScreenRantArticle>> GetArticleListAsync(int page = 1)
        {
            var document = await AngleSharpGetDocumentAsync($"{_newsUrl}{page}/");

            var articles = new List<ScreenRantArticle>();

            var cards = document.QuerySelectorAll(".sentinel-listing-page-list .display-card");

            foreach (var card in cards)
            {
                var titleEl = card.QuerySelector(".display-card-title a");
                var title = titleEl?.TextContent.Trim();
                var link = ResolveUrl(titleEl?.GetAttribute("href"));

                var image = card.QuerySelector("picture source")?.GetAttribute("srcset");

                var category = card.QuerySelector(".w-display-card-category span")?.TextContent.Trim();
                var excerpt = card.QuerySelector(".display-card-excerpt")?.TextContent.Trim();
                var author = card.QuerySelector(".w-author-name a")?.TextContent.Trim();
                var date = card.QuerySelector("time.display-card-date")?.TextContent.Trim();

                if (!string.IsNullOrWhiteSpace(title))
                {
                    articles.Add(new ScreenRantArticle
                    {
                        Title = title,
                        Link = link,
                        Img = image,
                        Category = category,
                        Excerpt = excerpt,
                        Author = author,
                        Date = date
                    });
                }
            }

            return articles;
        }

        public static async Task<ScreenRantArticleDetail> GetArticleDetailsAsync(string url)
        {
            try
            {
                var document = await AngleSharpGetDocumentAsync(url);

                var title = document.QuerySelector(".article-header .article-header-title")?.TextContent.Trim();
                var image = document.QuerySelector(".heading_image figure picture img")?.GetAttribute("src");

                var paragraphs = document.QuerySelectorAll("#article-body p")
                                         .Select(p => p.TextContent.Trim())
                                         .Where(text => !string.IsNullOrWhiteSpace(text))
                                         .ToList();

                var rawImages = document.QuerySelectorAll("#article-body img")
                                        .Select(img => img.GetAttribute("src"))
                                        .Where(src => !string.IsNullOrWhiteSpace(src))
                                        .ToList();

                var filteredImages = rawImages
                    .Where(img => Regex.IsMatch(img, @"\.(jpg|jpeg|png|webp)$", RegexOptions.IgnoreCase))
                    .ToList();

                return new ScreenRantArticleDetail
                {
                    Title = title,
                    Image = image,
                    Paragraphs = paragraphs,
                    Images = filteredImages
                };
            }
            catch
            {
                return default;
            }
        }

        private static string ResolveUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "";
            return path.StartsWith("http") ? path : $"{_baseUrl}{path}";
        }
    }
}
