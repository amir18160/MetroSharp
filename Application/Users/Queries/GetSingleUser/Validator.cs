using FluentValidation;

namespace Application.Users.Queries.GetSingleUser
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x)
                .Must(HaveExactlyOneProperty)
                .WithMessage("Exactly one of Id, Phone, or TelegramId must be provided.");
        }

        private bool HaveExactlyOneProperty(Query query)
        {   
            int count = 0;
            if (!string.IsNullOrWhiteSpace(query.Id)) count++;
            if (!string.IsNullOrWhiteSpace(query.Phone)) count++;
            if (!string.IsNullOrWhiteSpace(query.TelegramId)) count++;

            return count == 1;
        }
    }
}


