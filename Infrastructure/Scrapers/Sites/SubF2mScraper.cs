using AngleSharp.Dom;
using Domain.Models.Scrapers.Sub2fm;
using Infrastructure.Scrapers.Core;

namespace Infrastructure.Scrapers.Sites
{
    public class SubF2mScraper : ScraperBase
    {
        private const string _baseUrl = "https://subf2m.co";

        /**********************
            Public Methods
        **********************/

        public static async Task<List<SubF2mSubtitleSearchResult>> SearchSubtitleAsync(string query)
        {
            var url = BuildSearchUrl(query);

            IDocument document;
            try
            {
                document = await AngleSharpGetDocumentAsync(url);
            }
            catch
            {
                return [];
            }

            return ParseSearchResults(document);
        }

        public static async Task<List<SubF2mSubtitleDetail>> GetShowSubtitlesAsync(string url, string language = "", bool slugBased = false)
        {
            var resolvedUrl = BuildSubtitlesUrl(url, language, slugBased);

            IDocument document;
            try
            {
                document = await AngleSharpGetDocumentAsync(resolvedUrl);
            }
            catch
            {
                return new();
            }

            return ParseSubtitleDetails(document);
        }

        public static async Task<SubF2SubtitleDownload> GetDownloadLinkAsync(string url, string language = "", bool slugBased = false, string id = "")
        {
            var resolvedUrl = BuildDownloadUrl(url, language, slugBased, id);

            IDocument document;
            try
            {
                document = await AngleSharpGetDocumentAsync(resolvedUrl);
            }
            catch
            {
                return null;
            }

            return ParseDownloadLink(document);
        }

        /**********************
            Private Helpers
        **********************/

        private static string BuildSearchUrl(string query)
        {
            var formatted = query.Replace(" ", "+");
            return $"{_baseUrl}/subtitles/searchbytitle?query={formatted}";
        }

        private static string BuildSubtitlesUrl(string url, string language, bool slugBased)
        {
            if (slugBased)
                url = $"{_baseUrl}/subtitles/{url}";

            language = language switch
            {
                "fa" or "per" or "" => "farsi_persian",
                "en" => "english",
                _ => language
            };

            return $"{url}/{language}";
        }

        private static string BuildDownloadUrl(string url, string language, bool slugBased, string id)
        {
            if (slugBased && !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(language))
            {
                return $"{_baseUrl}/subtitles/{url}/{language}/{id}";
            }

            return url;
        }

        private static List<SubF2mSubtitleSearchResult> ParseSearchResults(IDocument document)
        {
            var items = document.QuerySelectorAll(".search-result ul li");

            return items
                .Select(item => item.QuerySelector("a"))
                .Where(anchor => anchor != null)
                .Select(anchor => new SubF2mSubtitleSearchResult
                {
                    Title = anchor.TextContent.Trim(),
                    Url = _baseUrl + anchor.GetAttribute("href")
                })
                .ToList();
        }

        private static List<SubF2mSubtitleDetail> ParseSubtitleDetails(IDocument document)
        {
            var container = document.QuerySelector(".sublist.larglist");
            if (container == null) return new();

            var items = container.QuerySelectorAll("li.item");

            return items.Select(item =>
            {
                var downloadAnchor = item.QuerySelector("a.download.icon-download");
                if (downloadAnchor == null) return null;

                var translator = item.QuerySelector(".comment-col a")?.TextContent?.Trim();
                var description = item.QuerySelector(".comment-col p")?.TextContent?.Trim();

                var nameList = item.QuerySelectorAll(".col-info .scrolllist li")
                                   .Select(li => li.TextContent.Trim())
                                   .ToList();

                return new SubF2mSubtitleDetail
                {
                    Url = _baseUrl + downloadAnchor.GetAttribute("href"),
                    Translator = translator,
                    Description = description,
                    Names = nameList
                };
            })
            .Where(detail => detail != null)
            .ToList();
        }

        private static SubF2SubtitleDownload ParseDownloadLink(IDocument document)
        {
            var anchor = document.QuerySelector(".download a#downloadButton");
            if (anchor == null) return null;

            return new SubF2SubtitleDownload
            {
                DownloadUrl = _baseUrl + anchor.GetAttribute("href")
            };
        }
    }
}
