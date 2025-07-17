using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.GeminiWrapper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;
using Persistence;

namespace Infrastructure.GeminiWrapper;

public class GeminiService :IGeminiService
{
    private readonly List<string> _apiKeys;
    private readonly ILogger<GeminiService> _logger;
    private readonly DownloadContext _context;


    public GeminiService(IOptions<GeminiSettings> options, ILogger<GeminiService> logger, DownloadContext context)
    {
        _logger = logger;
        _apiKeys = options.Value.ApiKeys;
        _context = context;
    }

    public async Task<string> GenerateContentAsync(string prompt)
    {
        var apiKey = await GetAvailableKeyAsync() ?? throw new InvalidOperationException("No available Gemini API key.");
        var ai = new GoogleAI(apiKey);
        var model = ai.GenerativeModel(Model.Gemini20FlashLite);

        try
        {
            var result = await model.GenerateContent(prompt);
            var text = result.Text;
            await UpdateUsageAsync(apiKey);
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini with key ending in {Suffix}", apiKey[^5..]);
            throw;
        }
    }

    private async Task<string> GetAvailableKeyAsync()
    {
        var today = DateTime.UtcNow.Date;

        foreach (var key in _apiKeys)
        {
            var usage = await _context.ApiUsages
                .Where(x => x.ApiKey == key && x.Date == today)
                .FirstOrDefaultAsync();

            if (usage == null || usage.Count < 1200)
                return key;
        }

        _logger.LogWarning("All Gemini API keys exceeded daily request limit.");
        return null;
    }

    private async Task UpdateUsageAsync(string apiKey)
    {
        var today = DateTime.UtcNow.Date;

        var usage = await _context.ApiUsages
            .FirstOrDefaultAsync(x => x.ApiKey == apiKey && x.Date == today);

        if (usage == null)
        {
            usage = new ApiUsage
            {
                ApiKey = apiKey,
                Date = today,
                Count = 1,
                ApiType = ApiServiceType.Gemini
            };
            _context.ApiUsages.Add(usage);
        }
        else
        {
            usage.Count++;
        }

        await _context.SaveChangesAsync();
    }
}
