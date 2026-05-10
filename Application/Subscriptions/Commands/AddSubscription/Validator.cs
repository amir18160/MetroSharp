using FluentValidation;

namespace Application.Subscriptions.Commands.AddSubscription;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x=> x.UserId).NotEmpty();
        RuleFor(x=> x.PlanId).NotEmpty();
    }
}