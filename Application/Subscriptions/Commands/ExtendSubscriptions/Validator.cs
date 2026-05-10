using FluentValidation;

namespace Application.Subscriptions.Commands.ExtendSubscriptions;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.DaysToAdd).NotEmpty().InclusiveBetween(0, 365);
        RuleFor(x => x.ExpirationMargin).NotEmpty().InclusiveBetween(0, 365);
    }
}