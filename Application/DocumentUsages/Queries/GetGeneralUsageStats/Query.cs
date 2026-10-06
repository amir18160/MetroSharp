using Application.Core;
using Application.DocumentUsages.DTOs;
using MediatR;

namespace Application.DocumentUsages.Queries.GetGeneralUsageStats;

public class Query : IRequest<Result<DocumentUsageStats>>
{
}