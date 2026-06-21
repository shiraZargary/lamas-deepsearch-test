using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Application.Metadata;
using MongoDB.Driver;

namespace DeepSearch.Infrastructure.Persistence.Repositories;

/// <summary>Reads metric/dimension metadata from the <c>metadata</c> collection.</summary>
public sealed class MetadataRepository(MongoContext context) : IMetadataRepository
{
    private readonly MongoContext _context = context;

    public async Task<IReadOnlyList<MetricInfo>> GetMetricsAsync(CancellationToken cancellationToken)
    {
        var docs = await _context.Metadata
            .Find(d => d.Kind == "metric")
            .ToListAsync(cancellationToken);

        return docs
            .Select(d => new MetricInfo(d.Code, d.Type ?? string.Empty, d.Field, d.Label))
            .ToList();
    }

    public async Task<IReadOnlyList<DimensionInfo>> GetDimensionsAsync(CancellationToken cancellationToken)
    {
        var docs = await _context.Metadata
            .Find(d => d.Kind == "dimension")
            .ToListAsync(cancellationToken);

        return docs
            .Select(d => new DimensionInfo(d.Code, d.Label, d.Values ?? []))
            .ToList();
    }
}
