using System.Net;
using AngleSharp;
using AngleSharp.Dom;

namespace Infrastructure.Scrapers.Core;

public class ScraperBase
{
    private static readonly IConfiguration Config = Configuration.Default.WithDefaultLoader();
    private static readonly IBrowsingContext Context = BrowsingContext.New(Config);

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    public static async Task<IDocument> AngleSharpLoadDocumentFromString(string renderedHtml)
    {
        var document = await Context.OpenAsync(request => request.Content(renderedHtml));
        return document;
    }

    public static async Task<IDocument> AngleSharpGetDocumentAsync(string address)
    {
        if (!IsValidUrl(address)) throw new Exception("provided url is invalid.");

        var document = await Context.OpenAsync(address);

        var response = document?.Context?.Active;
        if (response?.StatusCode != HttpStatusCode.OK || response == null)
            throw new Exception(
                $"Downloaded document status code was not 200|OK. status code is {response?.StatusCode}");

        return document;
    }


    private static readonly string[] UserAgents = new[]
    {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36",
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36"
    };
}