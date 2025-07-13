using FluentValidation;

namespace Application.Users.Update
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Bio).MaximumLength(500);
            RuleFor(x => x.Image).MaximumLength(250);
            RuleFor(x => x.TelegramProfileImage).MaximumLength(250);
        }
    }
}
