using Application.Core;
using Application.DocumentUsages.DTOs;
using Application.Interfaces;
using MediatR;

namespace Application.DocumentUsages.Queries.GetUserUsageStats;

public class Handler : IRequestHandler<Query, Result<UserUsage>>
{
    private readonly IDocumentUsageService _documentUsageService;

    public Handler(IDocumentUsageService documentUsageService)
    {
        _documentUsageService = documentUsageService;
    }

    public async Task<Result<UserUsage>> Handle(Query request, CancellationToken cancellationToken)
    {
        try
        {
            var userUsage = await _documentUsageService.GetUserUsageStats(request.UserId);
            return Result<UserUsage>.Success(userUsage);
        }
        catch (Exception ex)
        {
            return Result<UserUsage>.Failure("Failed to get user usage stats");
        }
    }
}