using Application.Core;
using MediatR;
using Persistence;

namespace Application.Tags.Commands.Delete
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tag = await _context.Tags.FindAsync(request.Id);

            if (tag == null)
            {
                return null;
            }

            _context.Remove(tag);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return Result<Unit>.Failure("Failed to delete tag");
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}