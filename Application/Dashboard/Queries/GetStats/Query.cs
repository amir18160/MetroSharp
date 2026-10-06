using Application.Core;
using Application.Dashboard.DTOs;
using MediatR;

namespace Application.Dashboard.Queries.GetStats;

public class Query : IRequest<Result<StatsDto>>
{
}