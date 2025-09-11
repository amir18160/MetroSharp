using FluentValidation;

namespace Application.Core;

public class DateFilterValidator : AbstractValidator<DateFilter>
{
    public DateFilterValidator()
    {
        RuleFor(x => x)
            .Must(HaveOnlyOneEqualityConstraint)
            .WithMessage(
                "Only one of 'Equal' or a range (GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual) can be set.");

        RuleFor(x => x)
            .Must(HaveValidRange)
            .WithMessage(
                "Date range is invalid. Ensure that the greater values are less than or equal to the lesser values.");
    }

    private bool HaveOnlyOneEqualityConstraint(DateFilter filter)
    {
        var hasEqual = filter.Equal.HasValue;
        var hasRange =
            filter.GreaterThan.HasValue ||
            filter.GreaterThanOrEqual.HasValue ||
            filter.LessThan.HasValue ||
            filter.LessThanOrEqual.HasValue;

        return !(hasEqual && hasRange);
    }

    private bool HaveValidRange(DateFilter filter)
    {
        var lowerBound = filter.GreaterThanOrEqual ?? filter.GreaterThan;
        var upperBound = filter.LessThanOrEqual ?? filter.LessThan;

        if (lowerBound.HasValue && upperBound.HasValue) return lowerBound <= upperBound;

        return true;
    }
}