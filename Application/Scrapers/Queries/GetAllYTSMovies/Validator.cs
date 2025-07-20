using FluentValidation;

namespace Application.Scrapers.Queries.GetAllYTSMovies
{
    public class Validator: AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page number must be greater than 0.");
        }
    }
}