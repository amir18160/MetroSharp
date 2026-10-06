using FluentValidation;

namespace Application.DocumentUsages.Queries.GetUserUsageStats;

public class Validator : AbstractValidator<Query>
{
    public Validator()
    {
        RuleFor(q => q.UserId).NotEmpty();
    }
}