using DeepSearch.Domain.Enums;

namespace DeepSearch.Infrastructure.Querying;

/// <summary>
/// Maps domain concepts to the whitelisted set of MongoDB field names.
/// Field names NEVER come from raw user input — only from these fixed maps —
/// which (together with passing values as BsonValues) keeps the dynamic
/// aggregation pipeline safe from NoSQL injection.
/// </summary>
internal static class QueryFieldMap
{
    public static readonly IReadOnlyDictionary<BreakdownDimension, string> BreakdownToField =
        new Dictionary<BreakdownDimension, string>
        {
            [BreakdownDimension.Year] = "year",
            [BreakdownDimension.Gender] = "gender",
            [BreakdownDimension.City] = "city",
            [BreakdownDimension.AgeGroup] = "ageGroup",
            [BreakdownDimension.Sector] = "sector"
        };

    /// <summary>Numeric fields permitted as the target of Average/Sum.</summary>
    public static readonly IReadOnlySet<string> AllowedMetricFields =
        new HashSet<string>(StringComparer.Ordinal) { "income" };

    public static string ResolveBreakdownField(BreakdownDimension dimension) =>
        BreakdownToField.TryGetValue(dimension, out var field)
            ? field
            : throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Unknown breakdown dimension");

    public static string ResolveMetricField(string? field)
    {
        if (string.IsNullOrWhiteSpace(field) || !AllowedMetricFields.Contains(field))
        {
            throw new ArgumentException($"Metric field '{field}' is not allowed.", nameof(field));
        }

        return field;
    }
}
