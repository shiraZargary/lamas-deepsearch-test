using DeepSearch.Application.Common.Interfaces;
using MediatR;

namespace DeepSearch.Application.Features.SavedQueries;

/// <summary>Lists all saved queries (most recent first).</summary>
public sealed record GetSavedQueriesQuery : IRequest<IReadOnlyList<SavedQueryDto>>;

public sealed class GetSavedQueriesQueryHandler(ISavedQueryRepository repository)
    : IRequestHandler<GetSavedQueriesQuery, IReadOnlyList<SavedQueryDto>>
{
    private readonly ISavedQueryRepository _repository = repository;

    public async Task<IReadOnlyList<SavedQueryDto>> Handle(
        GetSavedQueriesQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return items
            .Select(q => new SavedQueryDto(q.Id, q.Name, q.Definition, q.CreatedAt))
            .ToList();
    }
}
