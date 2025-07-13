using System.Net;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Playwright;

namespace Infrastructure.Scrapers.Core
{
    public class ScraperBase
    {

        private readonly static IConfiguration config = Configuration.Default.WithDefaultLoader();
        private readonly static IBrowsingContext context = BrowsingContext.New(config);
        private static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static async Task<IDocument> AngleSharpLoadDocumentFromString(string renderedHtml)
        {
            var document = await context.OpenAsync(request => request.Content(renderedHtml));
            return document;
        }

        public static async Task<IDocument> AngleSharpGetDocumentAsync(string address)
        {
            if (!IsValidUrl(address))
            {
                throw new Exception("provided url is invalid.");
            }

            var document = await context.OpenAsync(address);

            var response = document?.Context?.Active;
            if ((response?.StatusCode != HttpStatusCode.OK) || response == null)
            {
                throw new Exception($"Downloaded document status code was not 200|OK. status code is {response?.StatusCode}");
            }

            return document;
        }

        protected static async Task<IDocument> GetDocumentFromPlaywrightAsync(string url, Func<IPage, Task> waitForPage = null)
        {
            if (!IsValidUrl(url))
            {
                throw new Exception("provided url is invalid.");
            }

            using var playwright = await Playwright.CreateAsync();
            var browserOptions = new BrowserTypeLaunchOptions
            {
                Headless = true,
            };
            await using var browser = await playwright.Webkit.LaunchAsync(browserOptions);

            try
            {
                var page = await browser.NewPageAsync();
                await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

                if (waitForPage != null)
                {
                    await waitForPage(page);
                }

                var renderedHtml = await page.ContentAsync();
                await browser.CloseAsync();

                return await AngleSharpLoadDocumentFromString(renderedHtml);
            }
            catch (Exception Ex)
            {
                await browser.CloseAsync();
                throw new Exception($"Failed To Get Data From Headless Browser, {Ex.Message}");
            }
        }
    }
}