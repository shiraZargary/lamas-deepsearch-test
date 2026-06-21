using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Application.Metadata;
using DeepSearch.Domain.Enums;
using DeepSearch.Infrastructure.FreeSearch;

namespace DeepSearch.UnitTests;

public class RuleBasedFreeSearchServiceTests
{
    private sealed class FakeMetadataRepository : IMetadataRepository
    {
        public Task<IReadOnlyList<MetricInfo>> GetMetricsAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<MetricInfo>>(
            [
                new("avg_income", "Average", "income", "שכר ממוצע"),
                new("count_people", "Count", null, "כמות מועסקים"),
                new("sum_income", "Sum", "income", "סך השכר")
            ]);

        public Task<IReadOnlyList<DimensionInfo>> GetDimensionsAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<DimensionInfo>>(
            [
                new("year", "שנה", ["2020", "2021", "2022", "2023", "2024"]),
                new("gender", "מגדר", ["נשים", "גברים"]),
                new("city", "עיר", ["ירושלים", "תל אביב", "חיפה", "באר שבע"]),
                new("ageGroup", "קבוצת גיל", ["18-24", "25-35", "36-50", "51-67"]),
                new("sector", "מגזר", ["כללי", "חרדי", "ערבי"])
            ]);
    }

    private readonly RuleBasedFreeSearchService _freeSearch = new(new FakeMetadataRepository());

    [Fact]
    public async Task Interprets_AverageSalary_Women_Jerusalem_Range_ByYear()
    {
        var result = await _freeSearch.InterpretAsync(
            "הצג את השכר הממוצע של נשים בירושלים בשנים 2021-2024 לפי שנה", CancellationToken.None);

        var d = result.Definition;
        Assert.Equal(MetricType.Average, d.Metric.Type);
        Assert.Equal("income", d.Metric.Field);
        Assert.Equal("נשים", d.Population.Gender);
        Assert.Equal("ירושלים", d.Population.City);
        Assert.Null(d.Population.AgeGroup); // "2021-2024" must not be misread as an age range
        Assert.Equal(PeriodKind.Range, d.Period.Kind);
        Assert.Equal(2021, d.Period.FromYear);
        Assert.Equal(2024, d.Period.ToYear);
        Assert.Contains(BreakdownDimension.Year, d.Breakdowns);
        Assert.NotEmpty(result.Notes);
    }

    [Fact]
    public async Task Interprets_Count_SingleYear_City()
    {
        var result = await _freeSearch.InterpretAsync("כמות מועסקים בתל אביב בשנת 2023", CancellationToken.None);

        var d = result.Definition;
        Assert.Equal(MetricType.Count, d.Metric.Type);
        Assert.Null(d.Metric.Field);
        Assert.Equal("תל אביב", d.Population.City);
        Assert.Equal(PeriodKind.SingleYear, d.Period.Kind);
        Assert.Equal(2023, d.Period.FromYear);
    }

    [Fact]
    public async Task Interprets_Sector_AgeRange_AndDefaultsPeriodWhenNoYear()
    {
        var result = await _freeSearch.InterpretAsync(
            "שכר ממוצע של נשים חרדיות בגילאי 25-35", CancellationToken.None);

        var d = result.Definition;
        Assert.Equal("חרדי", d.Population.Sector);
        Assert.Equal("25-35", d.Population.AgeGroup);
        Assert.Equal(PeriodKind.Range, d.Period.Kind);
        Assert.Equal(2020, d.Period.FromYear);
        Assert.Equal(2024, d.Period.ToYear);
    }

    [Fact]
    public async Task Recognizes_MeaningfulQuestion()
    {
        var result = await _freeSearch.InterpretAsync(
            "שכר ממוצע של נשים בתל אביב לפי שנה", CancellationToken.None);

        Assert.True(result.Recognized);
    }

    [Theory]
    [InlineData("zdfs")]
    [InlineData("asdf qwer")]
    [InlineData("...")]
    public async Task FlagsGibberish_AsNotRecognized(string gibberish)
    {
        var result = await _freeSearch.InterpretAsync(gibberish, CancellationToken.None);

        Assert.False(result.Recognized);
        Assert.Contains(result.Notes, n => n.Contains("לא זוהו מונחים מוכרים"));
    }
}
