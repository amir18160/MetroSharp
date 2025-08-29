using Application.Core;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.OMDb.Commands.Update
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var omdbItem = await _context.OmdbItems.FindAsync(Guid.Parse(request.Id));

            if (omdbItem == null)
            {
                return null;
            }

            _mapper.Map(request.OmdbItem, omdbItem);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return Result<Unit>.Failure("Failed to update OMDB item");
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}