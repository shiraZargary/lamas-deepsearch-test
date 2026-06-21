using DeepSearch.Domain.Enums;

namespace DeepSearch.Domain.Queries;

/// <summary>
/// The single, technology-agnostic contract that describes a query.
/// Produced by BOTH the structured query builder and the natural-language parser,
/// and consumed by the execution layer — guaranteeing one execution path with
/// two entry points.
/// </summary>
public sealed record QueryDefinition
{
    public Population Population { get; init; } = new();
    public Metric Metric { get; init; } = new();
    public Period Period { get; init; } = new();
    public IReadOnlyList<BreakdownDimension> Breakdowns { get; init; } = [];
}
