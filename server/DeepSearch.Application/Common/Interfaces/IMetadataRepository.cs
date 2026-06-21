using DeepSearch.Application.Metadata;

namespace DeepSearch.Application.Common.Interfaces;

/// <summary>Reads the metadata that drives the query builder UI.</summary>
public interface IMetadataRepository
{
    Task<IReadOnlyList<MetricInfo>> GetMetricsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<DimensionInfo>> GetDimensionsAsync(CancellationToken cancellationToken);
}
