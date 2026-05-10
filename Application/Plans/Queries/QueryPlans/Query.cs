using Application.Core;
using Domain.Entities;
using MediatR;

namespace Application.Plans.Queries.QueryPlans;

public class Query : IRequest<Result<List<PlanDto>>>
{
}