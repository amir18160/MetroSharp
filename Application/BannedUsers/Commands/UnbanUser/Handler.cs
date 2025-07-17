using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.BannedUsers.Commands.UnbanUser
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
            var user = await _context.Users
                .Include(x => x.BannedUser)
                .FirstOrDefaultAsync(x => x.UserName == request.UserId, cancellationToken);

            if (user == null)
                return Result<Unit>.Failure("No user found with that ID.");

            if (user.BannedUser == null)
                return Result<Unit>.Failure("User is not currently banned.");

            _context.BannedUsers.Remove(user.BannedUser);

            var success = await _context.SaveChangesAsync(cancellationToken) > 0;

            return success
                ? Result<Unit>.Success(Unit.Value)
                : Result<Unit>.Failure("Failed to unban user.");
        }
    }
}
