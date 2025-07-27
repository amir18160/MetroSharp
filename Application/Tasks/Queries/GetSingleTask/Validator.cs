using FluentValidation;

namespace Application.Tasks.Queries.GetSingleTask
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}