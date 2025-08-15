using FluentValidation;

namespace Application.Scrapers.Queries.SearchSubtitle
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.SearchQuery)
                .NotEmpty();
        }
    }
}