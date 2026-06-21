using DeepSearch.Domain.Enums;

namespace DeepSearch.Domain.Queries;

/// <summary>
/// The measure to compute. <see cref="Field"/> is the numeric field aggregated
/// (e.g. "income"); it is ignored for <see cref="MetricType.Count"/>.
/// </summary>
public sealed record Metric
{
    public MetricType Type { get; init; }
    public string? Field { get; init; }
}
