using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.GeminiWrapper.Prompts;
using Infrastructure.OmdbWrapper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence;

namespace Infrastructure.OmdbWrapper
{
    public class OMDbService: IOmdbService 
    {
        private readonly List<string> _apiKeys;
        private readonly ILogger<OMDbService> _logger;
        private readonly DownloadContext _downloadContext;
        private readonly string _apiUrl;

        private readonly HttpClient _httpClient;
        private readonly IGeminiService _geminiService;
        private const int MaxRequestsPerDay = 1000;


        public OMDbService(IOptions<OMDbSettings> settings, ILogger<OMDbService> logger, DownloadContext downloadContext, HttpClient httpClient, IGeminiService geminiService)
        {
            _apiKeys = settings.Value.ApiKeys;
            _apiUrl = settings.Value.ApiUrl;
            _logger = logger;
            _downloadContext = downloadContext;
            _httpClient = httpClient;
            _geminiService = geminiService;
        }

        public async Task<OmdbItem> GetTitleByImdbIdAsync(string imdbId)
        {
            var today = DateTime.UtcNow.Date;

            foreach (var key in _apiKeys)
            {
                if (await CanUseKeyAsync(key, today))
                {
                    var url = $"{_apiUrl}/?apikey={key}&i={imdbId}&plot=full";
                    var response = await _httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var contentStream = await response.Content.ReadAsStreamAsync();
                        await UpdateUsageAsync(key, today);
                        var result = await JsonDocument.ParseAsync(contentStream);
                        var omdbItem = CreateOmdbItem(result);
                        try
                        {
                            var plotFa = await _geminiService.GenerateContentAsync(AiPrompts.TranslateRawText(omdbItem.Plot));
                            omdbItem.PlotFa = plotFa;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to translate using gemini service");
                        }
                    }
                }
            }

            _logger.LogWarning("All OMDb API keys exhausted.");
            throw new InvalidOperationException("All OMDb API keys have reached the daily limit.");
        }


        private static OmdbItem CreateOmdbItem(JsonDocument apiDocument)
        {
            string Sanitize(string value) =>
                string.IsNullOrWhiteSpace(value) || value == "N/A" ? null : value;

            List<string> ParseList(string value) =>
                Sanitize(value)?.Split(',').Select(s => s.Trim()).ToList() ?? new List<string>();

            var root = apiDocument.RootElement;

            var type = Sanitize(root.GetProperty("Type").GetString());
            var itemType = type == "series" ? OmdbItemType.Series : OmdbItemType.Movie;

            int rottenTomatoesScore = 0;
            if (root.TryGetProperty("Ratings", out var ratingsProp) && ratingsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var ratingElem in ratingsProp.EnumerateArray())
                {
                    if (ratingElem.TryGetProperty("Source", out var sourceProp) &&
                        ratingElem.TryGetProperty("Value", out var valueProp) &&
                        sourceProp.GetString() == "Rotten Tomatoes")
                    {
                        var value = valueProp.GetString(); // e.g. "95%"
                        if (!string.IsNullOrWhiteSpace(value) && value.EndsWith("%") &&
                            int.TryParse(value.TrimEnd('%'), out var parsedScore))
                        {
                            rottenTomatoesScore = parsedScore;
                            break;
                        }
                    }
                }
            }

            return new OmdbItem
            {
                ImdbId = Sanitize(root.GetProperty("imdbID").GetString()) ?? string.Empty,
                Title = Sanitize(root.GetProperty("Title").GetString()) ?? string.Empty,
                Rated = Sanitize(root.GetProperty("Rated").GetString()) ?? string.Empty,
                Released = Sanitize(root.GetProperty("Released").GetString()) ?? string.Empty,
                Runtime = Sanitize(root.GetProperty("Runtime").GetString()) ?? string.Empty,
                Genres = ParseList(root.GetProperty("Genre").GetString()),
                Directors = ParseList(root.GetProperty("Director").GetString()),
                Writers = ParseList(root.GetProperty("Writer").GetString()),
                Actors = ParseList(root.GetProperty("Actors").GetString()),
                Plot = Sanitize(root.GetProperty("Plot").GetString()) ?? string.Empty,
                PlotFa = string.Empty,
                Languages = ParseList(root.GetProperty("Language").GetString()),
                Countries = ParseList(root.GetProperty("Country").GetString()),
                Awards = Sanitize(root.GetProperty("Awards").GetString()) ?? string.Empty,
                Poster = Sanitize(root.GetProperty("Poster").GetString()) ?? string.Empty,
                Metascore = int.TryParse(Sanitize(root.GetProperty("Metascore").GetString()), out var meta) ? meta : 0,
                ImdbRating = double.TryParse(Sanitize(root.GetProperty("imdbRating").GetString()), out var rating) ? rating : 0,
                ImdbVotes = double.TryParse(Sanitize(root.GetProperty("imdbVotes").GetString())?.Replace(",", ""), out var votes) ? votes : 0,
                BoxOffice = Sanitize(root.TryGetProperty("BoxOffice", out var boxProp) ? boxProp.GetString() : null) ?? string.Empty,
                Year = int.TryParse(Sanitize(root.GetProperty("Year").GetString()), out var year) ? year : 0,
                Type = itemType,
                TotalSeasons = int.TryParse(Sanitize(root.TryGetProperty("totalSeasons", out var seasonProp) ? seasonProp.GetString() : null), out var totalSeasons)
                    ? totalSeasons
                    : 0,
                RottenTomatoesScore = rottenTomatoesScore
            };
        }



        private async Task<bool> CanUseKeyAsync(string key, DateTime date)
        {
            var usage = await _downloadContext.ApiUsages
                .FirstOrDefaultAsync(x => x.ApiKey == key && x.Date == date && x.ApiType == ApiServiceType.OMDb);

            return usage == null || usage.Count < MaxRequestsPerDay;
        }

        private async Task UpdateUsageAsync(string key, DateTime date)
        {
            var usage = await _downloadContext.ApiUsages
                .FirstOrDefaultAsync(x => x.ApiKey == key && x.Date == date && x.ApiType == ApiServiceType.OMDb);

            if (usage == null)
            {
                usage = new ApiUsage
                {
                    ApiKey = key,
                    Date = date,
                    Count = 1,
                    ApiType = ApiServiceType.OMDb
                };
                _downloadContext.ApiUsages.Add(usage);
            }
            else
            {
                usage.Count++;
            }

            await _downloadContext.SaveChangesAsync();
        }
    }
}