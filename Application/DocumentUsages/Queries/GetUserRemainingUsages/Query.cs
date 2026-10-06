using Application.Core;
using Application.DocumentUsages.DTOs;
using MediatR;

namespace Application.DocumentUsages.Queries.GetUserRemainingUsages;

public class Query : IRequest<Result<UserRemainingUsage>>
{
    public string UserId { get; set; }
}