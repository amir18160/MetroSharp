using Application.Core;
using FluentValidation;

namespace Application.Subscriptions.Queries.QuerySubscriptions;

public class Validator: AbstractValidator<Query>
{
    public Validator()
    {
        RuleFor(x => x.StartedAt).SetValidator(new DateFilterValidator()).When(x => x.StartedAt != null);
        RuleFor(x => x.EndAt).SetValidator(new DateFilterValidator()).When(x => x.EndAt != null);
    }
}