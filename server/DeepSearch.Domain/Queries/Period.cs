using DeepSearch.Domain.Enums;

namespace DeepSearch.Domain.Queries;

/// <summary>
/// The time span of the query. For <see cref="PeriodKind.SingleYear"/> only
/// <see cref="FromYear"/> is meaningful; for <see cref="PeriodKind.Range"/>
/// the inclusive range [FromYear, ToYear] applies.
/// </summary>
public sealed record Period
{
    public PeriodKind Kind { get; init; }
    public int FromYear { get; init; }
    public int ToYear { get; init; }

    /// <summary>Inclusive lower bound of the year range, regardless of kind.</summary>
    public int EffectiveFromYear => FromYear;

    /// <summary>Inclusive upper bound; equals FromYear for a single year.</summary>
    public int EffectiveToYear => Kind == PeriodKind.SingleYear ? FromYear : ToYear;
}
