using Application.Core;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Persistence;

namespace Application.Plans.Commands.CreatePlan;

public class Handler : IRequestHandler<Command, Result<Unit>>
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;

    public Handler(DataContext dataContext, IMapper mapper)
    {
        _dataContext = dataContext;
        _mapper = mapper;
    }
    
    public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
    {
        var newPlan = _mapper.Map<Plan>(request);
        _dataContext.Plans.Add(newPlan);
        var success = await _dataContext.SaveChangesAsync(cancellationToken) > 0;
        return !success ? Result<Unit>.Failure("Something went wrong") : Result<Unit>.Success(Unit.Value);
    }
}