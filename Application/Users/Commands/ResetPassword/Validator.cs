using FluentValidation;

namespace Application.Users.Commands.ResetPassword
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Code).NotEmpty();
            RuleFor(x => x.NewPassword)
                  .MinimumLength(6).WithMessage("New password must be at least 6 characters.")
                  .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
                  .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter.")
                  .Matches("[0-9]").WithMessage("New password must contain at least one number.")
                  .Matches("[^a-zA-Z0-9]").WithMessage("New password must contain at least one non-alphanumeric character.");
        }
    }
}