using DeepSearch.Domain.Enums;
using FluentValidation;

namespace DeepSearch.Application.Features.Queries;

/// <summary>Validates a query definition before it reaches the executor.</summary>
public sealed class ExecuteQueryQueryValidator : AbstractValidator<ExecuteQueryQuery>
{
    private const int MinYear = 1948;
    private const int MaxYear = 2100;

    public ExecuteQueryQueryValidator()
    {
        RuleFor(x => x.Definition.Metric.Type)
            .IsInEnum().WithMessage("סוג המדד אינו תקין");

        // Average/Sum require a numeric field; Count does not.
        When(x => x.Definition.Metric.Type is MetricType.Average or MetricType.Sum, () =>
        {
            RuleFor(x => x.Definition.Metric.Field)
                .NotEmpty().WithMessage("יש לבחור שדה מספרי עבור ממוצע/סכום");
        });

        RuleFor(x => x.Definition.Period.FromYear)
            .InclusiveBetween(MinYear, MaxYear).WithMessage($"שנת התחלה חייבת להיות בין {MinYear} ל-{MaxYear}");

        // For a range, ToYear must be >= FromYear and within bounds.
        When(x => x.Definition.Period.Kind == PeriodKind.Range, () =>
        {
            RuleFor(x => x.Definition.Period.ToYear)
                .InclusiveBetween(MinYear, MaxYear).WithMessage($"שנת סיום חייבת להיות בין {MinYear} ל-{MaxYear}")
                .GreaterThanOrEqualTo(x => x.Definition.Period.FromYear)
                .WithMessage("שנת הסיום חייבת להיות גדולה או שווה לשנת ההתחלה");
        });

        RuleForEach(x => x.Definition.Breakdowns)
            .IsInEnum().WithMessage("פילוח אינו תקין");
    }
}
