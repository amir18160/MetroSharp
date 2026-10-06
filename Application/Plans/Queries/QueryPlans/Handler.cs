using Application.Core;
using Application.Plans.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Plans.Queries.QueryPlans;

public class Handler : IRequestHandler<Query, Result<List<PlanDto>>>
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;

    public Handler(DataContext dataContext, IMapper mapper)
    {
        _dataContext = dataContext;
        _mapper = mapper;
    }

    public async Task<Result<List<PlanDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var allPlans = await _dataContext.Plans.ProjectTo<PlanDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return Result<List<PlanDto>>.Success(allPlans);
    }
}