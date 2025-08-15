using System.Net;
using System.Text.Json;
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
                Headless = false,
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

        private static readonly string[] UserAgents = new[]
    {
        // common desktop user agents
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36",
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36"
    };

        public static async Task<JsonDocument> BrowserFetchJsonAsync(
            string url,
            string method = "GET",
            object body = null,
            Dictionary<string, string> headers = null,
            bool headless = false,
            string proxy = null)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new ArgumentException("Provided URL is invalid.", nameof(url));

            var rnd = new Random();
            var userAgent = UserAgents[rnd.Next(UserAgents.Length)];
            var width = rnd.Next(1200, 1920);
            var height = rnd.Next(700, 1080);
            var timezone = "Europe/Berlin"; // or randomize from list
            var locale = "en-US";

            // launch browser
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless,
                Args = new[]
                {
                "--disable-web-security",
                "--disable-features=IsolateOrigins,site-per-process",
                "--no-sandbox",
                "--disable-blink-features=AutomationControlled"
            },
                Proxy = proxy != null ? new Proxy { Server = proxy } : null
            });

            // create context with stealth settings
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = userAgent,
                ViewportSize = new ViewportSize { Width = width, Height = height },
                Locale = locale,
                TimezoneId = timezone,
                AcceptDownloads = false,
                BypassCSP = true,
                DeviceScaleFactor = rnd.Next(1, 3),
                // default viewport: null for full screen
                Permissions = new[] { "geolocation" },
                // Geolocation = new Geolocation { Latitude = 52.5200f, Longitude = 13.4050f },
                RecordVideoDir = null
            });

            // inject stealth scripts before any page load
            await context.AddInitScriptAsync(@"() => {
                Object.defineProperty(navigator, 'webdriver', { get: () => false });
                window.navigator.chrome = { runtime: {} };
                Object.defineProperty(navigator, 'languages', { get: () => ['en-US', 'en'] });
                Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3, 4, 5] });
            }");

            var page = await context.NewPageAsync();

            // simulate human-like movement
            for (int i = 0; i < 3; i++)
            {
                await page.Mouse.MoveAsync(rnd.Next(0, width), rnd.Next(0, height));
                await page.WaitForTimeoutAsync(rnd.Next(100, 500));
            }

            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // set extra headers (including common ones)
            var defaultHeaders = new Dictionary<string, string>
            {
                ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8",
                ["Accept-Language"] = "en-US,en;q=0.9",
                ["Sec-Fetch-Site"] = "none",
                ["Sec-Fetch-Mode"] = "navigate",
                ["Sec-Fetch-User"] = "?1",
                ["Sec-Fetch-Dest"] = "document"
            };
            if (headers != null)
            {
                foreach (var kv in headers)
                    defaultHeaders[kv.Key] = kv.Value;
            }
            await context.SetExtraHTTPHeadersAsync(defaultHeaders);

            // prepare fetch options
            var fetchOpts = new Dictionary<string, object> { ["method"] = method.ToUpperInvariant() };
            if (!defaultHeaders.ContainsKey("Content-Type") && body != null)
                defaultHeaders["Content-Type"] = "application/json";
            if (body != null && (method == "POST" || method == "PUT" || method == "PATCH"))
            {
                fetchOpts["body"] = body is string s ? s : JsonSerializer.Serialize(body);
            }
            fetchOpts["headers"] = defaultHeaders;

            // random short delay before fetch
            await page.WaitForTimeoutAsync(rnd.Next(200, 1000));

            var jsonText = await page.EvaluateAsync<string>(@"async (url, options) => {
            const resp = await fetch(url, options);
            if (!resp.ok) throw `Fetch failed: ${resp.status}`;
            return await resp.text();
        }", new object[] { url, fetchOpts });

            await browser.CloseAsync();
            return JsonDocument.Parse(jsonText);
        }

        public static async Task<JsonDocument> BrowserNavigateJsonAsync(
            string url,
            bool headless = false,
            Dictionary<string, string> headers = null,
            string proxy = null)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new ArgumentException("Provided URL is invalid.", nameof(url));

            var rnd = new Random();
            var userAgent = UserAgents[rnd.Next(UserAgents.Length)];
            var width = rnd.Next(1200, 1920);
            var height = rnd.Next(700, 1080);
            var timezone = "Europe/Berlin";
            var locale = "en-US";

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless,
                Args = new[]
                {
                "--no-sandbox",
                "--disable-blink-features=AutomationControlled",
                "--disable-features=IsolateOrigins,site-per-process"
            },
                Proxy = proxy != null ? new Proxy { Server = proxy } : null
            });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = userAgent,
                ViewportSize = new ViewportSize { Width = width, Height = height },
                Locale = locale,
                TimezoneId = timezone,
                BypassCSP = true
            });

            // stealth script
            await context.AddInitScriptAsync(@"() => {
            Object.defineProperty(navigator, 'webdriver', { get: () => false });
            window.navigator.chrome = { runtime: {} };
            Object.defineProperty(navigator, 'languages', { get: () => ['en-US', 'en'] });
            Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3, 4, 5] });
        }");

            var page = await context.NewPageAsync();

            // set extra headers
            var defaultHeaders = new Dictionary<string, string>
            {
                ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                ["Accept-Language"] = "en-US,en;q=0.9",
                ["Sec-Fetch-Site"] = "none",
                ["Sec-Fetch-Mode"] = "navigate",
                ["Sec-Fetch-Dest"] = "document"
            };
            if (headers != null)
            {
                foreach (var kv in headers)
                    defaultHeaders[kv.Key] = kv.Value;
            }
            await context.SetExtraHTTPHeadersAsync(defaultHeaders);

            // random delay
            await page.WaitForTimeoutAsync(rnd.Next(200, 1000));

            // navigate and wait
            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // extract inner text of <pre>
            var rawJson = await page.InnerTextAsync("pre");

            await browser.CloseAsync();

            return JsonDocument.Parse(rawJson);
        }

        public static async Task<string> BrowserDownloadFileAsync(
            string url,
            string downloadSelector = null,
            string downloadDir = null,
            bool headless = false,
            Dictionary<string, string> headers = null,
            string proxy = null
        )
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new ArgumentException("Provided URL is invalid.", nameof(url));

            var rnd = new Random();
            var userAgent = UserAgents[rnd.Next(UserAgents.Length)];
            var width = rnd.Next(1200, 1920);
            var height = rnd.Next(700, 1080);

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless,
                Args = new[] { "--no-sandbox", "--disable-blink-features=AutomationControlled" },
                Proxy = proxy != null ? new Proxy { Server = proxy } : null
            });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = userAgent,
                ViewportSize = new ViewportSize { Width = width, Height = height },
                AcceptDownloads = true,
            });

            if (headers != null)
                await context.SetExtraHTTPHeadersAsync(headers);

            var page = await context.NewPageAsync();

            IDownload download;
            if (string.IsNullOrEmpty(downloadSelector))
            {
                download = await page.RunAndWaitForDownloadAsync(() =>
                    page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle }));
            }
            else
            {
                await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                download = await page.RunAndWaitForDownloadAsync(() =>
                    page.ClickAsync(downloadSelector));
            }

            // save to specified directory or temp
            var path = await download.PathAsync();
            await browser.CloseAsync();

            return path;
        }

    }
}