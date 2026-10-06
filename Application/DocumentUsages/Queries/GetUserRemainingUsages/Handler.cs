using Application.Core;
using Application.DocumentUsages.DTOs;
using Application.Interfaces;
using MediatR;

namespace Application.DocumentUsages.Queries.GetUserRemainingUsages;

public class Handler: IRequestHandler<Query, Result<UserRemainingUsage>>
{
    private readonly IDocumentUsageService _documentUsageService;

    public Handler(IDocumentUsageService documentUsageService)
    {
        _documentUsageService = documentUsageService;
    }


    public async Task<Result<UserRemainingUsage>> Handle(Query request, CancellationToken cancellationToken)
    {
        try
        {
            var remaining = await _documentUsageService.GetUserRemainingUsage(request.UserId);
            return Result<UserRemainingUsage>.Success(remaining);
        }
        catch (Exception ex)
        {
            return Result<UserRemainingUsage>.Failure("Failed to get user usage stats");
        }
    }
}