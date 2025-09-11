using Application.Core;
using MediatR;

namespace Application.Dashboard.Queries.GetStats;

public class Query : IRequest<Result<StatsDto>>
{
}