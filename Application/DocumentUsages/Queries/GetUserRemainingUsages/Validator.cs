using FluentValidation;

namespace Application.DocumentUsages.Queries.GetUserRemainingUsages;

public class Validator: AbstractValidator<Query>
{
    public Validator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}