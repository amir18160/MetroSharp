using FluentValidation;

namespace Application.Plans.Commands.UpdatePlan;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        // RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().When(x => x.Title != null);
        RuleFor(x => x.Length).NotEmpty().When(x => x.Length != null).InclusiveBetween(1, 365);
        RuleFor(x => x.Price).NotEmpty().When(x => x.Price != null).InclusiveBetween(1, 1000);
        RuleFor(x => x.DiscountPercent).NotEmpty().When(x => x.DiscountPercent != null).InclusiveBetween(0, 100);
        RuleFor(x => x.NumberOfDownloads).NotEmpty().When(x => x.NumberOfDownloads != null).InclusiveBetween(0, 1000);
        RuleFor(x => x.NumberOfRequest).NotEmpty().When(x => x.NumberOfRequest != null).InclusiveBetween(0, 1000);
    }
}