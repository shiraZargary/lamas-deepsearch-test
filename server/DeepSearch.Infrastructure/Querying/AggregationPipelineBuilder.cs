using DeepSearch.Domain.Enums;
using DeepSearch.Domain.Queries;
using MongoDB.Bson;

namespace DeepSearch.Infrastructure.Querying;

/// <summary>The built pipeline plus the column metadata needed to read results.</summary>
public sealed record PipelinePlan(
    BsonDocument[] Stages,
    IReadOnlyList<string> GroupKeys,
    string ValueColumn);

/// <summary>
/// Translates a <see cref="QueryDefinition"/> into a MongoDB aggregation pipeline
/// (<c>$match → $group → $project → $sort</c>). All values are passed as
/// <see cref="BsonValue"/> (parameterized); field names come only from the
/// whitelisted <see cref="QueryFieldMap"/> — never from raw strings.
/// </summary>
public sealed class AggregationPipelineBuilder
{
    public const string ValueColumn = "value";

    public PipelinePlan Build(QueryDefinition def)
    {
        var stages = new List<BsonDocument>
        {
            new("$match", BuildMatch(def))
        };

        var groupKeys = def.Breakdowns
            .Select(QueryFieldMap.ResolveBreakdownField)
            .Distinct()
            .ToList();

        stages.Add(new BsonDocument("$group", BuildGroup(def, groupKeys)));
        stages.Add(new BsonDocument("$project", BuildProjection(groupKeys)));

        if (groupKeys.Count > 0)
        {
            var sort = new BsonDocument();
            foreach (var key in groupKeys)
            {
                sort[key] = 1;
            }
            stages.Add(new BsonDocument("$sort", sort));
        }

        return new PipelinePlan([.. stages], groupKeys, ValueColumn);
    }

    private static BsonDocument BuildMatch(QueryDefinition def)
    {
        var match = new BsonDocument();
        var p = def.Population;

        if (!string.IsNullOrWhiteSpace(p.Gender)) match["gender"] = p.Gender;
        if (!string.IsNullOrWhiteSpace(p.AgeGroup)) match["ageGroup"] = p.AgeGroup;
        if (!string.IsNullOrWhiteSpace(p.City)) match["city"] = p.City;
        if (!string.IsNullOrWhiteSpace(p.Sector)) match["sector"] = p.Sector;

        match["year"] = new BsonDocument
        {
            ["$gte"] = def.Period.EffectiveFromYear,
            ["$lte"] = def.Period.EffectiveToYear
        };

        return match;
    }

    private static BsonDocument BuildGroup(QueryDefinition def, IReadOnlyList<string> groupKeys)
    {
        BsonValue id;
        if (groupKeys.Count == 0)
        {
            id = BsonNull.Value;
        }
        else
        {
            var idDoc = new BsonDocument();
            foreach (var key in groupKeys)
            {
                idDoc[key] = $"${key}";
            }
            id = idDoc;
        }

        return new BsonDocument
        {
            ["_id"] = id,
            [ValueColumn] = BuildAccumulator(def.Metric)
        };
    }

    private static BsonDocument BuildAccumulator(Metric metric) => metric.Type switch
    {
        MetricType.Count => new BsonDocument("$sum", 1),
        MetricType.Sum => new BsonDocument("$sum", $"${QueryFieldMap.ResolveMetricField(metric.Field)}"),
        MetricType.Average => new BsonDocument("$avg", $"${QueryFieldMap.ResolveMetricField(metric.Field)}"),
        _ => throw new ArgumentOutOfRangeException(nameof(metric), metric.Type, "Unknown metric type")
    };

    private static BsonDocument BuildProjection(IReadOnlyList<string> groupKeys)
    {
        var project = new BsonDocument { ["_id"] = 0, [ValueColumn] = 1 };
        foreach (var key in groupKeys)
        {
            project[key] = $"$_id.{key}";
        }
        return project;
    }
}
