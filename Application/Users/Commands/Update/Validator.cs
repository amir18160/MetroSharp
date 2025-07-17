using FluentValidation;

namespace Application.Users.Commands.Update
{
    public class Validator : AbstractValidator<Command>
    {

        public Validator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters.");

            When(x => !string.IsNullOrEmpty(x.NewPassword), () =>
            {
                RuleFor(x => x.CurrentPassword)
                    .NotEmpty().WithMessage("Current password is required when changing password.");

                RuleFor(x => x.NewPassword)
                    .MinimumLength(6).WithMessage("New password must be at least 6 characters.")
                    .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter.") 
                    .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter.")
                    .Matches("[0-9]").WithMessage("New password must contain at least one number.") 
                    .Matches("[^a-zA-Z0-9]").WithMessage("New password must contain at least one non-alphanumeric character.");
            });

            When(x => !string.IsNullOrEmpty(x.CurrentPassword), () =>
            {
                RuleFor(x => x.NewPassword)
                    .NotEmpty().WithMessage("New password is required if current password is provided.");
            });
        }
    }
}
