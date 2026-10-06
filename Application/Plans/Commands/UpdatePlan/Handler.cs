using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Plans.Commands.UpdatePlan;

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
        var plan = await _dataContext.Plans.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (plan == null) return null;
        _mapper.Map(request, plan);
        var success = await _dataContext.SaveChangesAsync(cancellationToken) > 0;
        return !success ? Result<Unit>.Failure("Something went wrong") : Result<Unit>.Success(Unit.Value);
    }
}