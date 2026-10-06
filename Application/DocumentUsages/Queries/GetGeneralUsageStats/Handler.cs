using Application.Core;
using Application.DocumentUsages.DTOs;
using Application.Interfaces;
using MediatR;

namespace Application.DocumentUsages.Queries.GetGeneralUsageStats;

public class Handler : IRequestHandler<Query, Result<DocumentUsageStats>>
{
    private readonly IDocumentUsageService _documentUsageService;

    public Handler(IDocumentUsageService documentUsageService)
    {
        _documentUsageService = documentUsageService;
    }

    public async Task<Result<DocumentUsageStats>> Handle(Query request, CancellationToken cancellationToken)
    {
        try
        {
            var generalStats = await _documentUsageService.GetGeneralStats();
            return Result<DocumentUsageStats>.Success(generalStats);
        }
        catch (Exception ex)
        {
            return Result<DocumentUsageStats>.Failure("Failed to get document stats");
        }
    }
}