using System.Security.Permissions;
using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Users.Commands.ConfirmEmail
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor;
            _context = context;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var emailVerificationsWithUser = await _context.EmailVerifications
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Code == request.Code.ToString() && x.Expiration > DateTime.UtcNow && !x.Used, cancellationToken);

            if (emailVerificationsWithUser == null)
            {
                return Result<Unit>.Failure("code has expired or already used.");
            }

            emailVerificationsWithUser.Used = true;
            emailVerificationsWithUser.User.IsConfirmed = true;

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;
            return result
                ? Result<Unit>.Success(Unit.Value)
                : Result<Unit>.Failure("failed to confirm confirmation code, try again later.");
        }

    }
}