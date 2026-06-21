using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Application.Services;
using DeepSearch.Domain.Queries;
using MediatR;

namespace DeepSearch.Application.Features.Queries;

/// <summary>Executes a structured query definition and returns the phrasing + results.</summary>
public sealed record ExecuteQueryQuery(QueryDefinition Definition)
    : IRequest<ExecuteQueryResponse>;

/// <summary>The readable question plus the tabular result (table + chart source).</summary>
public sealed record ExecuteQueryResponse(string Question, QueryResult Result);

public sealed class ExecuteQueryQueryHandler(
    IQueryExecutor executor,
    QuestionPhrasingService phrasing)
    : IRequestHandler<ExecuteQueryQuery, ExecuteQueryResponse>
{
    private readonly IQueryExecutor _executor = executor;
    private readonly QuestionPhrasingService _phrasing = phrasing;

    public async Task<ExecuteQueryResponse> Handle(
        ExecuteQueryQuery request, CancellationToken cancellationToken)
    {
        var question = _phrasing.Phrase(request.Definition);
        var result = await _executor.ExecuteAsync(request.Definition, cancellationToken);
        return new ExecuteQueryResponse(question, result);
    }
}
