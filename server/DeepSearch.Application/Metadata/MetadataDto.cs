namespace DeepSearch.Application.Metadata;

/// <summary>A selectable metric exposed to the UI.</summary>
public sealed record MetricInfo(string Code, string Type, string? Field, string Label);

/// <summary>A selectable dimension (filter/breakdown) exposed to the UI.</summary>
public sealed record DimensionInfo(string Code, string Label, IReadOnlyList<string> Values);

/// <summary>Everything the UI needs to render the query builder dynamically.</summary>
public sealed record MetadataDto(
    IReadOnlyList<MetricInfo> Metrics,
    IReadOnlyList<DimensionInfo> Dimensions);
