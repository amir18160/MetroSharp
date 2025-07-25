using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Users.Commands.RequestResetPassword
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly IEmailService _emailService;

        public Handler(DataContext context, IEmailService emailService)
        {
            _emailService = emailService;
            _context = context;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
            .Include(u => u.EmailVerifications)
            .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

            if (user == null)
            {
                return Result<Unit>.Failure("User not found.");
            }

            var alreadyInProcess = user.EmailVerifications.Any(x =>
                x.Purpose == Domain.Enums.EmailPurpose.PasswordReset &&
                x.Expiration > DateTime.UtcNow &&
                !x.Used);

            if (alreadyInProcess)
            {
                return Result<Unit>.Failure("A reset password email is already in progress. Please check your inbox or Spam folder.");
            }

            var generatedCode = Guid.NewGuid().ToString();

            _context.EmailVerifications.Add(new EmailVerification
            {
                Expiration = DateTime.UtcNow.AddMinutes(20),
                User = user,
                Code = generatedCode,
                Purpose = Domain.Enums.EmailPurpose.PasswordReset,
            });

            var result = await _context.SaveChangesAsync(cancellationToken);
            if (result == 0)
            {
                return Result<Unit>.Failure("Failed to save reset password email record.");
            }

            var emailSenderResult = await _emailService.SendPasswordResetEmail(user.Email, user.Name, generatedCode);
            if (!emailSenderResult)
            {
                return Result<Unit>.Failure("Failed to send new reset password email.");
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
