using Application.Core;
using FluentValidation;

namespace Application.BannedUsers.Queries.GetBannedUsers
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.UserId)
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage("UserId must be a valid GUID.")
                .When(x => !string.IsNullOrWhiteSpace(x.UserId));

            RuleFor(x => x.BannedAt)
                .SetValidator(new DateFilterValidator())
                .When(x => x.BannedAt != null);

            RuleFor(x => x.FreeAt)
                .SetValidator(new DateFilterValidator())
                .When(x => x.FreeAt != null);

            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
