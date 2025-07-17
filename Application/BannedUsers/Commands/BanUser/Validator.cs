using FluentValidation;

namespace Application.BannedUsers.Commands.BanUser
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("There must be a reason for banning a user.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("There must be a UserId to ban a user.");

            RuleFor(x => x.FreeAt)
                .Must(date => date == null || date > DateTime.UtcNow)
                .WithMessage("FreeAt must be null or a future date.");
        }
    }
}