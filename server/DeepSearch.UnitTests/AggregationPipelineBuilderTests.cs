using DeepSearch.Domain.Enums;
using DeepSearch.Domain.Queries;
using DeepSearch.Infrastructure.Querying;
using MongoDB.Bson;

namespace DeepSearch.UnitTests;

public class AggregationPipelineBuilderTests
{
    private readonly AggregationPipelineBuilder _builder = new();

    private static QueryDefinition SampleDefinition() => new()
    {
        Population = new Population { Gender = "נשים", City = "ירושלים" },
        Metric = new Metric { Type = MetricType.Average, Field = "income" },
        Period = new Period { Kind = PeriodKind.Range, FromYear = 2021, ToYear = 2024 },
        Breakdowns = [BreakdownDimension.Year]
    };

    [Fact]
    public void Build_Match_AppliesPopulationFiltersAndYearRange()
    {
        var plan = _builder.Build(SampleDefinition());

        var match = plan.Stages[0]["$match"].AsBsonDocument;
        Assert.Equal("נשים", match["gender"].AsString);
        Assert.Equal("ירושלים", match["city"].AsString);
        Assert.Equal(2021, match["year"]["$gte"].AsInt32);
        Assert.Equal(2024, match["year"]["$lte"].AsInt32);
    }

    [Fact]
    public void Build_Group_UsesAvgAccumulatorAndBreakdownKey()
    {
        var plan = _builder.Build(SampleDefinition());

        var group = plan.Stages[1]["$group"].AsBsonDocument;
        Assert.Equal("$year", group["_id"]["year"].AsString);
        Assert.Equal("$income", group["value"]["$avg"].AsString);
        Assert.Contains("year", plan.GroupKeys);
    }

    [Fact]
    public void Build_Count_DoesNotRequireField()
    {
        var def = SampleDefinition() with { Metric = new Metric { Type = MetricType.Count } };

        var plan = _builder.Build(def);

        var group = plan.Stages[1]["$group"].AsBsonDocument;
        Assert.Equal(1, group["value"]["$sum"].AsInt32);
    }

    [Fact]
    public void Build_NoBreakdowns_GroupsByNullAndOmitsSort()
    {
        var def = SampleDefinition() with { Breakdowns = [] };

        var plan = _builder.Build(def);

        var group = plan.Stages[1]["$group"].AsBsonDocument;
        Assert.Equal(BsonNull.Value, group["_id"]);
        Assert.Empty(plan.GroupKeys);
        Assert.DoesNotContain(plan.Stages, s => s.Contains("$sort"));
    }

    [Theory]
    [InlineData("secret")]
    [InlineData("income; drop")]
    [InlineData("$where")]
    [InlineData("")]
    public void Build_RejectsNonWhitelistedMetricField(string field)
    {
        var def = SampleDefinition() with
        {
            Metric = new Metric { Type = MetricType.Sum, Field = field }
        };

        Assert.Throws<ArgumentException>(() => _builder.Build(def));
    }

    [Fact]
    public void Build_PassesUserValuesAsBsonValues_NotInterpolatedIntoFieldNames()
    {
        // A malicious "value" must land as a plain match value, never as an operator/field.
        var def = SampleDefinition() with
        {
            Population = new Population { City = "{$ne:null}" }
        };

        var plan = _builder.Build(def);
        var match = plan.Stages[0]["$match"].AsBsonDocument;

        Assert.True(match["city"].IsString);
        Assert.Equal("{$ne:null}", match["city"].AsString);
    }
}
