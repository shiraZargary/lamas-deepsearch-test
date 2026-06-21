using DeepSearch.Application.Services;
using DeepSearch.Domain.Enums;
using DeepSearch.Domain.Queries;

namespace DeepSearch.UnitTests;

public class QuestionPhrasingServiceTests
{
    private readonly QuestionPhrasingService _service = new();

    [Fact]
    public void Phrase_AverageWithPopulationRangeAndBreakdown()
    {
        var def = new QueryDefinition
        {
            Population = new Population { Gender = "נשים", AgeGroup = "25-35", City = "ירושלים" },
            Metric = new Metric { Type = MetricType.Average, Field = "income" },
            Period = new Period { Kind = PeriodKind.Range, FromYear = 2021, ToYear = 2024 },
            Breakdowns = [BreakdownDimension.Year]
        };

        var sentence = _service.Phrase(def);

        Assert.Contains("השכר הממוצע", sentence);
        Assert.Contains("נשים", sentence);
        Assert.Contains("בגילאי 25-35", sentence);
        Assert.Contains("בירושלים", sentence);
        Assert.Contains("בין השנים 2021–2024", sentence);
        Assert.Contains("בחלוקה לפי שנה", sentence);
    }

    [Fact]
    public void Phrase_SingleYearUsesSingularForm()
    {
        var def = new QueryDefinition
        {
            Metric = new Metric { Type = MetricType.Count },
            Period = new Period { Kind = PeriodKind.SingleYear, FromYear = 2023 }
        };

        var sentence = _service.Phrase(def);

        Assert.Contains("כמות הרשומות", sentence);
        Assert.Contains("בשנת 2023", sentence);
    }
}
