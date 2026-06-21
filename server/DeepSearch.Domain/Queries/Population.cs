namespace DeepSearch.Domain.Queries;

/// <summary>
/// The population (filter) the query applies to. All properties are optional;
/// a null value means "do not filter on this attribute".
/// </summary>
public sealed record Population
{
    public string? Gender { get; init; }
    public string? AgeGroup { get; init; }
    public string? City { get; init; }
    public string? Sector { get; init; }
}
