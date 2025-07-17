using Application.Core;
using FluentValidation;

namespace Application.Users.Queries.GetUsers
{
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Role)
                .Must(role => role == "User" || role == "Admin" || role == "Owner")
                .WithMessage("Role must be either User, Admin, or Owner.")
                .When(x => !string.IsNullOrWhiteSpace(x.Role));

            RuleFor(x => x.CreatedAt)
                .SetValidator(new DateFilterValidator())
                .When(x => x.CreatedAt != null);

            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
