using DeepSearch.Application.Common.Exceptions;
using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Application.Features.Queries;
using MediatR;

namespace DeepSearch.Application.Features.SavedQueries;

/// <summary>Loads a saved query by id and re-runs it via the standard execute path.</summary>
public sealed record RunSavedQueryQuery(string Id) : IRequest<ExecuteQueryResponse>;

public sealed class RunSavedQueryQueryHandler(ISavedQueryRepository repository, ISender sender)
    : IRequestHandler<RunSavedQueryQuery, ExecuteQueryResponse>
{
    private readonly ISavedQueryRepository _repository = repository;
    private readonly ISender _sender = sender;

    public async Task<ExecuteQueryResponse> Handle(
        RunSavedQueryQuery request, CancellationToken cancellationToken)
    {
        var saved = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"שאילתה שמורה עם המזהה '{request.Id}' לא נמצאה");

        // Reuse the single execution path (validation + phrasing + execution).
        return await _sender.Send(new ExecuteQueryQuery(saved.Definition), cancellationToken);
    }
}
