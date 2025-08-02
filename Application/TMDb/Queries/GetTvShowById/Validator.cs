using FluentValidation;

namespace Application.TMDb.Queries.GetTvShowById
{
    public class Validator: AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}