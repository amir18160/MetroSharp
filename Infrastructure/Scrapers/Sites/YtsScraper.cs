using AngleSharp.Dom;
using Domain.Models.Scrapers.Yts;
using Infrastructure.Scrapers.Core;

namespace Infrastructure.Scrapers.Sites
{

    public class YtsScraper : ScraperBase
    {
        private static readonly string _baseUrl = "https://yts.mx";
        private enum QueryType
        {
            Search,
            All
        }

        /**********************
            private methods
        **********************/

        private static YtsPreview ParseMovieWrap(IElement wrap)
        {
            var movie = new YtsPreview();

            var titleElement = wrap.QuerySelector(".browse-movie-title");
            movie.Title = titleElement?.TextContent?.Trim();
            movie.DetailUrl = titleElement?.GetAttribute("href");

            movie.Year = wrap.QuerySelector(".browse-movie-year")?.TextContent?.Trim();

            var img = wrap.QuerySelector("img");
            var src = img?.GetAttribute("src");
            movie.ImageUrl = string.IsNullOrWhiteSpace(src) ? null : $"{_baseUrl}{src}";
            var rating = wrap.QuerySelector("figcaption .rating")?.TextContent?.Trim();
            movie.Rating = rating;

            var genreHeaders = wrap.QuerySelectorAll("figcaption h4")
                .Where(h => !h.ClassList.Contains("rating"))
                .Select(h => h.TextContent.Trim())
                .ToList();

            movie.Genres = genreHeaders;

            return movie;
        }

        private static List<YtsTorrent> ExtractTorrents(IDocument document)
        {
            var torrents = new List<YtsTorrent>();

            var torrentBlocks = document.QuerySelectorAll(".modal-torrent");

            foreach (var block in torrentBlocks)
            {
                try
                {
                    var quality = block.QuerySelector(".modal-quality span")?.TextContent?.Trim();

                    var sizeText = block.QuerySelectorAll(".quality-size")
                                        .LastOrDefault()?.TextContent?.Trim();

                    var size = ParseSizeInGb(sizeText);

                    var torrentLink = block.QuerySelector("a.download-torrent[href^='https://yts.mx/torrent/download/']")
                                           ?.GetAttribute("href");

                    var magnetLink = block.QuerySelector("a[href^='magnet:?']")
                                           ?.GetAttribute("href");

                    torrents.Add(new YtsTorrent
                    {
                        Quality = quality,
                        Size = size,
                        TorrentFileLink = torrentLink,
                        MagnetLink = magnetLink
                    });
                }
                catch
                {
                    continue;
                }
            }

            return torrents;
        }

        private static double ParseSizeInGb(string sizeText)
        {
            if (string.IsNullOrWhiteSpace(sizeText)) return 0;

            sizeText = sizeText.ToUpper().Replace(" ", "");

            if (sizeText.EndsWith("GB") && double.TryParse(sizeText.Replace("GB", ""), out var gb))
                return gb;

            if (sizeText.EndsWith("MB") && double.TryParse(sizeText.Replace("MB", ""), out var mb))
                return Math.Round(mb / 1024.0, 2);

            return 0;
        }


        private static async Task<List<YtsPreview>> SearchAllMoviesAsync(int page = 1, QueryType type = QueryType.All, string searchFor = "")
        {
            string url;
            string searchQuery;

            if (type == QueryType.All)
            {
                searchQuery = "0";
            }
            else
            {
                searchQuery = searchFor;
            }

            if (page != 1)
            {
                url = $"{_baseUrl}/browse-movies/{searchQuery}/all/all/0/featured/0/all?page={page}";
            }
            else
            {
                url = $"{_baseUrl}/browse-movies/{searchQuery}/all/all/0/featured/0/all";
            }

            var document = await AngleSharpGetDocumentAsync(url);
            var movieWraps = document.QuerySelectorAll(".browse-movie-wrap");

            var movies = new List<YtsPreview>();
            foreach (var wrap in movieWraps)
            {
                movies.Add(ParseMovieWrap(wrap));
            }

            return movies;
        }

        /**********************
            public methods
        **********************/

        public static async Task<List<YtsPreview>> GetAllMoviesAsync(int page = 1)
        {
            return await SearchAllMoviesAsync(page, QueryType.All);
        }

        public static async Task<List<YtsPreview>> SearchMovieAsync(string query)
        {
            return await SearchAllMoviesAsync(type: QueryType.Search, searchFor: query);
        }

        public static async Task<List<YtsPreview>> GetPopularMoviesAsync()
        {
            var document = await AngleSharpGetDocumentAsync(_baseUrl);

            var popularSection = document.GetElementById("popular-downloads")
                ?? throw new Exception("Popular section does not exist");

            var rows = popularSection.QuerySelectorAll(".row");
            if (rows.Length <= 1)
                throw new Exception("No second <row> element in popular section");

            var row = rows[1];

            var movieWraps = row.QuerySelectorAll(".browse-movie-wrap");
            if (movieWraps.Length == 0)
                throw new Exception("No browse-movie-wrap elements found in the row");

            var movies = new List<YtsPreview>();

            foreach (var wrap in movieWraps)
            {
                movies.Add(ParseMovieWrap(wrap));
            }

            return movies;
        }


        public static async Task<YtsDetails> GetMovieDetailsAsync(string url)
        {
            var document = await AngleSharpGetDocumentAsync(url);

            var moviePoster = document.GetElementById("movie-poster") ?? throw new Exception("No movie poster found");
            var image = moviePoster.QuerySelector(".img-responsive") ?? throw new Exception("No image found");
            var imageUrl = image.GetAttribute("src");

            var movieInfo = document.GetElementById("movie-info") ?? throw new Exception("No movie info found");
            var title = movieInfo.QuerySelector(".hidden-xs h1").Text() ?? throw new Exception("No Title Found");
            var yearContainer = movieInfo.QuerySelectorAll(".hidden-xs h2");

            if (yearContainer.Length < 1)
            {
                throw new Exception("No year found");
            }
            var year = yearContainer[0].Text();
            var genres = yearContainer[1]?.Text();

            var movieDescriptions = document.QuerySelector("#movie-sub-info #synopsis p")?.Text();

            var imdbLinkElement = document.QuerySelector("a[href*='imdb.com/title/']");
            if (imdbLinkElement == null)
                return null;

            var imdbUrl = imdbLinkElement.GetAttribute("href");

            var imdbId = imdbUrl?.Split('/').FirstOrDefault(s => s.StartsWith("tt"));

            var ratingContainer = imdbLinkElement.ParentElement;
            var ratingElement = ratingContainer?.QuerySelector("[itemprop='ratingValue']");
            var rating = ratingElement?.TextContent?.Trim();
            var torrents = ExtractTorrents(document);

            return new YtsDetails
            {
                Title = title,
                Year = year,
                Genres = genres.Split("/")
                    .Select(s => s.Trim())
                    .ToList(),
                ImageUrl = imageUrl,
                Description = movieDescriptions,
                ImdbId = imdbId,
                ImdbUrl = imdbUrl,
                ImdbRating = rating,
                DetailUrl = url,
                AvailableTorrents = torrents
            };
        }
    }
}
