using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Application.Metadata;
using MediatR;

namespace DeepSearch.Application.Features.Metadata;

/// <summary>Returns the metrics and dimensions that drive the query builder UI.</summary>
public sealed record GetMetadataQuery : IRequest<MetadataDto>;

public sealed class GetMetadataQueryHandler(IMetadataRepository repository)
    : IRequestHandler<GetMetadataQuery, MetadataDto>
{
    private readonly IMetadataRepository _repository = repository;

    public async Task<MetadataDto> Handle(GetMetadataQuery request, CancellationToken cancellationToken)
    {
        var metrics = await _repository.GetMetricsAsync(cancellationToken);
        var dimensions = await _repository.GetDimensionsAsync(cancellationToken);
        return new MetadataDto(metrics, dimensions);
    }
}
