using AngleSharp.Dom;
using Domain.Models.Scrapers.Subsource;
using Infrastructure.Scrapers.Core;
using Microsoft.Playwright;

namespace Infrastructure.Scrapers.Sites
{
    public class SubsourceScraper : ScraperBase
    {
        private static readonly string _baseUrl = "https://subsource.net";
        private static string GenerateSearchUrl(string query)
        {
            return $"{_baseUrl}/search/${query}";
        }

        private static List<SubsourceTableEntity> ParseSubtitleTable(IDocument document)
        {
            var results = new List<SubsourceTableEntity>();

            var anchors = document.QuerySelectorAll("a");

            if (anchors.Length == 0)
                throw new Exception("No subtitle links found");

            foreach (var anchor in anchors)
            {
                var href = anchor.GetAttribute("href");
                if (string.IsNullOrWhiteSpace(href))
                    continue;

                var tds = anchor.QuerySelectorAll("td");
                if (tds.Length < 8) continue;

                string language = tds[1]?.TextContent?.Trim();
                string title = anchor.QuerySelector("p[aria-label]")?.GetAttribute("aria-label")?.Trim();
                string source = tds[4]?.TextContent?.Trim();
                string uploader = tds[5]?.TextContent?.Trim();
                string team = tds[7]?.TextContent?.Trim();

                results.Add(new SubsourceTableEntity
                {
                    Href = href,
                    Language = language,
                    Title = title,
                    Source = source,
                    Uploader = uploader,
                    Team = team
                });
            }

            return results;
        }


        public static async Task<ICollection<SubsourceSearchResult>> SearchSubtitle(string query)
        {
            var document = await GetDocumentFromPlaywrightAsync(GenerateSearchUrl(query), async (page) =>
                {
                    await page.Locator("p", new PageLocatorOptions { HasText = "Search Results" })
                            .WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
                });

            var posterImages = document.QuerySelectorAll("img[alt='poster']");
            var results = new List<SubsourceSearchResult>();

            foreach (var img in posterImages)
            {
                var imageUrl = img.GetAttribute("src");
                var titleElement = img.ParentElement
                    ?.QuerySelector("p.MuiTypography-root");

                var title = titleElement?.TextContent?.Trim();

                var anchor = img.Closest("a");
                var href = anchor?.GetAttribute("href");

                if (!string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(href))
                {
                    results.Add(new SubsourceSearchResult
                    {
                        Title = title,
                        ImageUrl = imageUrl,
                        Href = $"{_baseUrl}{href}"
                    });
                }
            }

            Console.WriteLine(results.Count);

            if (results.Count == 0)
            {
                throw new Exception("no result");
            }
            return results;
        }

        public static async Task<ICollection<SubsourceTableEntity>> GetSubtitlesForUrl(string url)
        {
            var results = new Dictionary<string, SubsourceTableEntity>();

            await GetDocumentFromPlaywrightAsync(url, async (page) =>
            {
                await page.Locator("table").WaitForAsync(new LocatorWaitForOptions { Timeout = 20000 });

                var scrollSelector = "[data-viewport]";
                var scrollTarget = await page.QuerySelectorAsync(scrollSelector);
                var useDocumentScroll = scrollTarget == null;

                for (int i = 0; i < 200; i++)
                {
                    var anchors = await page.QuerySelectorAllAsync("a[href^='/subtitle/']");

                    foreach (var anchor in anchors)
                    {
                        var href = await anchor.GetAttributeAsync("href");
                        if (string.IsNullOrWhiteSpace(href) || results.ContainsKey(href))
                            continue;

                        var tr = await anchor.EvaluateHandleAsync("el => el.closest('tr')");
                        var tds = await tr.EvaluateAsync<string[]>("row => Array.from(row.querySelectorAll('td')).map(td => td.textContent.trim())");

                        string language = tds.Length > 1 ? tds[1] : null;
                        string source = tds.Length > 4 ? tds[4] : null;
                        string uploader = tds.Length > 5 ? tds[5] : null;
                        string team = tds.Length > 7 ? tds[7] : null;


                        string title = await anchor.EvaluateAsync<string>("el => el.querySelector('p[aria-label]')?.getAttribute('aria-label')");
                        string caption = await anchor.EvaluateAsync<string>(
                            "el => el.closest('tr')?.querySelector('td:last-child p')?.textContent?.trim()"
                        );

                        results[href] = new SubsourceTableEntity
                        {
                            Href = $"{_baseUrl}{href}",
                            Language = language,
                            Title = title,
                            Source = source,
                            Uploader = uploader,
                            Team = team,
                            Caption = caption
                        };
                    }

                    // Scroll to load more
                    if (useDocumentScroll)
                        await page.EvaluateAsync("window.scrollBy(0, 500)");
                    else
                        await scrollTarget.EvaluateAsync("el => el.scrollBy(0, 500)");

                    await Task.Delay(50);
                }
            });

            return results.Values.ToList();
        }

        public static async Task<string> GetDownloadLinkFromUrl(string url)
        {
            string downloadHref = null!;

            await GetDocumentFromPlaywrightAsync(url, async (page) =>
            {
                var anchor = await page.WaitForSelectorAsync("a[href^='https://api.subsource.net/api/downloadSub/']", new PageWaitForSelectorOptions
                {
                    Timeout = 20000,
                    State = WaitForSelectorState.Visible
                }) ?? throw new Exception("Download link not found on the page.");
                downloadHref = await anchor.GetAttributeAsync("href");
            });

            return downloadHref;
        }

    }
}