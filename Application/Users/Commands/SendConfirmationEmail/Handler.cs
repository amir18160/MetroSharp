using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Users.Commands.SendConfirmationEmail
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly IUserAccessor _userAccessor;
        private readonly DataContext _context;
        private readonly IEmailService _emailService;
        public Handler(IUserAccessor userAccessor, DataContext context, IEmailService emailService)
        {
            _emailService = emailService;
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.EmailVerifications)
                .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUserName(), cancellationToken);

            if (user == null)
            {
                return Result<Unit>.Failure("User not found.");
            }

            if (user.IsConfirmed)
            {
                return Result<Unit>.Failure("User is already confirmed.");
            }

            var alreadyInProcess = user.EmailVerifications.Any(x =>
                x.Purpose == Domain.Enums.EmailPurpose.Confirmation &&
                x.Expiration > DateTime.Now &&
                !x.Used);

            if (alreadyInProcess)
            {
                return Result<Unit>.Failure("A confirmation email is already in progress. Please check your inbox or Spam folder.");
            }

            var generatedCode = Guid.NewGuid().ToString();

            _context.EmailVerifications.Add(new EmailVerification
            {
                Expiration = DateTime.Now.AddMinutes(20),
                User = user,
                Code = generatedCode,
                Purpose = Domain.Enums.EmailPurpose.Confirmation,
            });

            var result = await _context.SaveChangesAsync(cancellationToken);
            if (result == 0)
            {
                return Result<Unit>.Failure("Failed to save email verification record.");
            }
            
            var emailSenderResult = await _emailService.SendConfirmationEmail(user.Email, user.Name, generatedCode);
            if (!emailSenderResult)
            {
                return Result<Unit>.Failure("Failed to send new confirmation email.");
            }

            return Result<Unit>.Success(Unit.Value);
        }

    }
}