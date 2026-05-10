using FluentValidation;

namespace Application.Users.Commands.ActiveDeactive;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Activate)
            .NotNull().WithMessage("Activation state is required.");
    }
}