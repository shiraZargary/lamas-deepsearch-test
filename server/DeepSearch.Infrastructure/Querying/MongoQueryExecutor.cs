using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Domain.Queries;
using DeepSearch.Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DeepSearch.Infrastructure.Querying;

/// <summary>
/// Executes a <see cref="QueryDefinition"/> by running the built aggregation
/// pipeline against the <c>statistics_fact</c> collection and mapping the
/// BSON output into a technology-agnostic <see cref="QueryResult"/>.
/// </summary>
public sealed class MongoQueryExecutor(MongoContext context, AggregationPipelineBuilder builder)
    : IQueryExecutor
{
    private readonly MongoContext _context = context;
    private readonly AggregationPipelineBuilder _builder = builder;

    public async Task<QueryResult> ExecuteAsync(QueryDefinition definition, CancellationToken cancellationToken)
    {
        var plan = _builder.Build(definition);

        var cursor = await _context.StatisticsFact.AggregateAsync<BsonDocument>(
            plan.Stages, cancellationToken: cancellationToken);
        var documents = await cursor.ToListAsync(cancellationToken);

        var columns = new List<string>(plan.GroupKeys) { plan.ValueColumn };

        var rows = documents
            .Select(doc => MapRow(doc, columns))
            .ToList();

        return new QueryResult { Columns = columns, Rows = rows };
    }

    private static IReadOnlyDictionary<string, object?> MapRow(BsonDocument doc, IEnumerable<string> columns)
    {
        var row = new Dictionary<string, object?>();
        foreach (var column in columns)
        {
            row[column] = doc.TryGetValue(column, out var value) ? ToClr(value) : null;
        }
        return row;
    }

    private static object? ToClr(BsonValue value) => value.BsonType switch
    {
        BsonType.Null => null,
        BsonType.Int32 => value.AsInt32,
        BsonType.Int64 => value.AsInt64,
        BsonType.Double => Math.Round(value.AsDouble, 2),
        BsonType.Decimal128 => (decimal)value.AsDecimal128,
        BsonType.Boolean => value.AsBoolean,
        _ => value.AsString
    };
}
