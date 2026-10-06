using FluentValidation;

namespace Application.Plans.Commands.CreatePlan;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Length).NotEmpty().InclusiveBetween(1, 365);
        RuleFor(x => x.Price).NotEmpty().InclusiveBetween(1, 1000);
        RuleFor(x => x.DiscountPercent).NotEmpty().When(x => x.DiscountPercent != null).InclusiveBetween(0, 100);
        RuleFor(x => x.DiscountReason).MaximumLength(255);
        RuleFor(x => x.NumberOfDirectLinks).NotEmpty().InclusiveBetween(0, 1000);
        RuleFor(x => x.NumberOfFileDownloads).NotEmpty().InclusiveBetween(0, 1000);
        RuleFor(x => x.NumberOfRequest).NotEmpty().InclusiveBetween(0, 1000);
    }
}