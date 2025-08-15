using FluentValidation;

namespace Application.Scrapers.Queries.GetTorrentDetails
{
    public class Validator: AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Source)
                .NotEmpty();
        }
    }
}
