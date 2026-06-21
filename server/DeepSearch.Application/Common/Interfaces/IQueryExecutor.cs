using DeepSearch.Domain.Queries;

namespace DeepSearch.Application.Common.Interfaces;

/// <summary>
/// Executes a <see cref="QueryDefinition"/> against the data store and returns
/// tabular results. The single execution path used by the structured builder,
/// saved queries and the natural-language flow.
/// </summary>
public interface IQueryExecutor
{
    Task<QueryResult> ExecuteAsync(QueryDefinition definition, CancellationToken cancellationToken);
}
