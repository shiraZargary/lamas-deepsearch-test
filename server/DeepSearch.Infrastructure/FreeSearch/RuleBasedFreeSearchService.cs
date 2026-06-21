using System.Text.RegularExpressions;
using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Application.Metadata;
using DeepSearch.Domain.Enums;
using DeepSearch.Domain.Queries;

namespace DeepSearch.Infrastructure.FreeSearch;

/// <summary>
/// Offline, deterministic Hebrew free-search service that maps a free-text question
/// to a <see cref="QueryDefinition"/> using keyword/value matching driven by the
/// database metadata. Default implementation — requires no API key.
/// </summary>
public sealed partial class RuleBasedFreeSearchService(IMetadataRepository metadata) : IFreeSearchService
{
    private readonly IMetadataRepository _metadata = metadata;

    public async Task<FreeSearchResult> InterpretAsync(string question, CancellationToken cancellationToken)
    {
        var normalized = Normalize(question);
        var notes = new List<string>();

        var metrics = await _metadata.GetMetricsAsync(cancellationToken);
        var dimensions = await _metadata.GetDimensionsAsync(cancellationToken);

        var metric = DetectMetric(normalized, metrics, notes, out var metricExplicit);
        var population = DetectPopulation(normalized, dimensions, notes);
        var period = DetectPeriod(normalized, dimensions, notes, out var periodExplicit);
        var breakdowns = DetectBreakdowns(normalized, notes);

        var definition = new QueryDefinition
        {
            Population = population,
            Metric = metric,
            Period = period,
            Breakdowns = breakdowns
        };

        // "Recognized" means we found at least one meaningful signal in the text.
        // A blind default (Count over the whole default year range, no filters) is
        // NOT a real interpretation, so we flag it for the UI to block execution.
        var populationRecognized =
            population.Gender is not null || population.City is not null ||
            population.Sector is not null || population.AgeGroup is not null;

        var recognized = metricExplicit || populationRecognized || periodExplicit || breakdowns.Count > 0;
        if (!recognized)
        {
            notes.Insert(0, "לא זוהו מונחים מוכרים בשאלה — נסו לנסח מחדש (לדוגמה: \"שכר ממוצע של נשים בתל אביב לפי שנה\")");
        }

        return new FreeSearchResult(definition, notes, recognized);
    }

    private static string Normalize(string text) =>
        WhitespaceRegex().Replace(text.Replace('–', '-').Replace('—', '-'), " ").Trim();

    private static Metric DetectMetric(string text, IReadOnlyList<MetricInfo> metrics, List<string> notes, out bool explicitMetric)
    {
        MetricType type;
        explicitMetric = true;
        if (text.Contains("ממוצע")) type = MetricType.Average;
        else if (text.Contains("סכום") || text.Contains("סך")) type = MetricType.Sum;
        else if (text.Contains("כמות") || text.Contains("מספר")) type = MetricType.Count;
        else if (text.Contains("שכר") || text.Contains("הכנסה")) type = MetricType.Average;
        else { type = MetricType.Count; explicitMetric = false; }

        var field = metrics.FirstOrDefault(m =>
            string.Equals(m.Type, type.ToString(), StringComparison.OrdinalIgnoreCase))?.Field;

        var label = type switch
        {
            MetricType.Average => "שכר ממוצע",
            MetricType.Sum => "סך השכר",
            _ => "כמות"
        };
        notes.Add($"מדד: {label}");
        return new Metric { Type = type, Field = field };
    }

    private static Population DetectPopulation(
        string text, IReadOnlyList<DimensionInfo> dimensions, List<string> notes)
    {
        string? Match(string code) => dimensions
            .FirstOrDefault(d => d.Code == code)?.Values
            .FirstOrDefault(v => text.Contains(v));

        var gender = Match("gender");
        var city = Match("city");
        var sector = Match("sector");

        // Prefer an explicitly listed age-group value; otherwise accept a free
        // range like "25-35" found in the text.
        var ageGroup = Match("ageGroup");
        if (ageGroup is null && AgeRangeRegex().Match(text) is { Success: true } range)
        {
            ageGroup = range.Value;
        }

        var parts = new List<string>();
        if (gender is not null) parts.Add(gender);
        if (sector is not null) parts.Add(sector);
        if (ageGroup is not null) parts.Add($"גילאי {ageGroup}");
        if (city is not null) parts.Add(city);
        if (parts.Count > 0) notes.Add("אוכלוסייה: " + string.Join(", ", parts));

        return new Population { Gender = gender, City = city, Sector = sector, AgeGroup = ageGroup };
    }

    private static Period DetectPeriod(
        string text, IReadOnlyList<DimensionInfo> dimensions, List<string> notes, out bool explicitPeriod)
    {
        var years = YearRegex().Matches(text)
            .Select(m => int.Parse(m.Value))
            .Distinct()
            .OrderBy(y => y)
            .ToList();

        if (years.Count >= 2)
        {
            explicitPeriod = true;
            notes.Add($"תקופה: {years.First()}–{years.Last()}");
            return new Period { Kind = PeriodKind.Range, FromYear = years.First(), ToYear = years.Last() };
        }
        if (years.Count == 1)
        {
            explicitPeriod = true;
            notes.Add($"תקופה: {years[0]}");
            return new Period { Kind = PeriodKind.SingleYear, FromYear = years[0] };
        }

        explicitPeriod = false;
        var yearValues = dimensions.FirstOrDefault(d => d.Code == "year")?.Values
            .Select(int.Parse).OrderBy(y => y).ToList() ?? [DateTime.UtcNow.Year];
        notes.Add($"לא צוינה שנה — נבחר טווח ברירת מחדל {yearValues.First()}–{yearValues.Last()}");
        return new Period { Kind = PeriodKind.Range, FromYear = yearValues.First(), ToYear = yearValues.Last() };
    }

    private static List<BreakdownDimension> DetectBreakdowns(string text, List<string> notes)
    {
        var map = new (string Label, BreakdownDimension Dim)[]
        {
            ("שנה", BreakdownDimension.Year),
            ("מגדר", BreakdownDimension.Gender),
            ("עיר", BreakdownDimension.City),
            ("קבוצת גיל", BreakdownDimension.AgeGroup),
            ("מגזר", BreakdownDimension.Sector)
        };

        var result = new List<BreakdownDimension>();
        if (text.Contains("לפי"))
        {
            var afterLefi = text[(text.IndexOf("לפי", StringComparison.Ordinal) + 3)..];
            foreach (var (label, dim) in map)
            {
                if (afterLefi.Contains(label))
                {
                    result.Add(dim);
                }
            }
        }

        if (result.Count > 0)
        {
            notes.Add("פילוח: " + string.Join(", ", result));
        }
        return result;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"\b(19|20)\d{2}\b")]
    private static partial Regex YearRegex();

    [GeneratedRegex(@"(?<!\d)\d{1,2}-\d{1,2}(?!\d)")]
    private static partial Regex AgeRangeRegex();
}
