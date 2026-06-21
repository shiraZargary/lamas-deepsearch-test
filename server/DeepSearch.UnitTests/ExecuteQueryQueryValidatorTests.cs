using DeepSearch.Application.Features.Queries;
using DeepSearch.Domain.Enums;
using DeepSearch.Domain.Queries;

namespace DeepSearch.UnitTests;

public class ExecuteQueryQueryValidatorTests
{
    private readonly ExecuteQueryQueryValidator _validator = new();

    private static ExecuteQueryQuery Query(QueryDefinition def) => new(def);

    [Fact]
    public void Valid_AverageQuery_Passes()
    {
        var def = new QueryDefinition
        {
            Metric = new Metric { Type = MetricType.Average, Field = "income" },
            Period = new Period { Kind = PeriodKind.Range, FromYear = 2020, ToYear = 2024 }
        };

        Assert.True(_validator.Validate(Query(def)).IsValid);
    }

    [Fact]
    public void Average_WithoutField_Fails()
    {
        var def = new QueryDefinition
        {
            Metric = new Metric { Type = MetricType.Average },
            Period = new Period { Kind = PeriodKind.SingleYear, FromYear = 2024 }
        };

        Assert.False(_validator.Validate(Query(def)).IsValid);
    }

    [Fact]
    public void Range_WithToYearBeforeFromYear_Fails()
    {
        var def = new QueryDefinition
        {
            Metric = new Metric { Type = MetricType.Count },
            Period = new Period { Kind = PeriodKind.Range, FromYear = 2024, ToYear = 2020 }
        };

        Assert.False(_validator.Validate(Query(def)).IsValid);
    }

    [Fact]
    public void Count_WithoutField_Passes()
    {
        var def = new QueryDefinition
        {
            Metric = new Metric { Type = MetricType.Count },
            Period = new Period { Kind = PeriodKind.SingleYear, FromYear = 2024 }
        };

        Assert.True(_validator.Validate(Query(def)).IsValid);
    }
}
