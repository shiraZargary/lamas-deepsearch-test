namespace DeepSearch.Domain.Queries;

/// <summary>
/// Tabular result of executing a <see cref="QueryDefinition"/>: an ordered list of
/// column names and the corresponding rows (each row keyed by column name).
/// Suitable for rendering directly as a table and a chart on the client.
/// </summary>
public sealed record QueryResult
{
    public IReadOnlyList<string> Columns { get; init; } = [];
    public IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows { get; init; } = [];
}
