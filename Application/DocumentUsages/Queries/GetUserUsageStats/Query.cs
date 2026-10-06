using Application.Core;
using Application.DocumentUsages.DTOs;
using MediatR;

namespace Application.DocumentUsages.Queries.GetUserUsageStats;

public class Query : IRequest<Result<UserUsage>>
{
    public string UserId { get; set; }
}