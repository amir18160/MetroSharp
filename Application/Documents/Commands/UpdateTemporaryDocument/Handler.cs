using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Documents.Commands.UpdateTemporaryDocument;

public class Handler : IRequestHandler<Command, Result<Unit>>
{
    private readonly DataContext _context;
    private readonly ILogger<Handler> _logger;
    private readonly IOmdbService _omdbService;

    public Handler(DataContext context, ILogger<Handler> logger, IOmdbService omdbService)
    {
        _context = context;
        _logger = logger;
        _omdbService = omdbService;
    }

    public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .Include(d => d.Episode) // ensure navigation is loaded
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (document == null)
            return Result<Unit>.Failure("Document not found");

        // --- Ensure OmdbItem exists ---
        var omdbItem = await _context.OmdbItems
            .Include(x => x.Seasons)
            .ThenInclude(s => s.Episodes).ThenInclude(episode => episode.Documents)
            .AsSingleQuery()
            .FirstOrDefaultAsync(x => x.ImdbId == request.ImdbId, cancellationToken);

        if (omdbItem == null)
            try
            {
                var newOmdbItem = await _omdbService.GetTitleByImdbIdAsync(request.ImdbId);
                if (newOmdbItem == null)
                    return Result<Unit>.Failure("OMDb item not found");
                _context.OmdbItems.Add(newOmdbItem);
                var created = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!created)
                    return Result<Unit>.Failure("Failed to create OMDb item");

                omdbItem = newOmdbItem;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch OMDb item");
                return Result<Unit>.Failure("Failed to create OMDb item");
            }

        // --- Ensure Season exists ---
        if (request.Type == DocumentType.Episode)
        {
            Season season = null;
            if (request.Season.HasValue)
            {
                season = omdbItem.Seasons
                    .FirstOrDefault(x => x.SeasonNumber == request.Season.Value);

                if (season == null)
                {
                    season = new Season
                    {
                        SeasonNumber = request.Season.Value,
                        OmdbItem = omdbItem
                    };

                    _context.Seasons.Add(season);
                    var created = await _context.SaveChangesAsync(cancellationToken) > 0;
                    if (!created)
                        return Result<Unit>.Failure("Failed to create season");
                }
            }

            // --- Ensure Episode exists ---
            Episode episode = null;
            if (request.Episode.HasValue && season != null)
            {
                episode = season.Episodes
                    .FirstOrDefault(x => x.EpisodeNumber == request.Episode.Value);

                if (episode == null)
                {
                    episode = new Episode
                    {
                        EpisodeNumber = request.Episode.Value,
                        Season = season,
                        Documents = new List<Document>()
                    };

                    _context.Episodes.Add(episode);
                    var created = await _context.SaveChangesAsync(cancellationToken) > 0;
                    if (!created)
                        return Result<Unit>.Failure("Failed to create episode");
                }
            }

            // --- Link Document to Episode ---
            if (episode != null)
            {
                if (episode.Documents == null)
                    episode.Documents = new List<Document>();

                if (!episode.Documents.Contains(document)) episode.Documents.Add(document);

                document.Episode = episode;
            }
        }

        // --- Update Document ---
        document.FileId = document.FileId.Replace("temp_", string.Empty);
        document.UniqueFileId = document.FileId.Replace("temp_", string.Empty);
        document.UpdatedAt = DateTime.UtcNow;
        document.Type = request.Type;

        if (string.IsNullOrWhiteSpace(request.FileName)) document.FileName = request.FileName;

        if (request.IsSubbed.HasValue) document.IsSubbed = request.IsSubbed.Value;

        if (!request.Season.HasValue && !request.Episode.HasValue) omdbItem.Documents.Add(document);


        var success = await _context.SaveChangesAsync(cancellationToken) > 0;
        return success
            ? Result<Unit>.Success(Unit.Value)
            : Result<Unit>.Failure("Failed to update document");
    }
}