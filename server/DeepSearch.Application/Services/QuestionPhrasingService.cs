using System.Text;
using DeepSearch.Domain.Enums;
using DeepSearch.Domain.Queries;

namespace DeepSearch.Application.Services;

/// <summary>
/// Builds a human-readable Hebrew sentence describing a <see cref="QueryDefinition"/>.
/// Pure, dependency-free logic so it is trivially unit-testable (Req #2).
/// </summary>
public sealed class QuestionPhrasingService
{
    private static readonly Dictionary<BreakdownDimension, string> BreakdownLabels = new()
    {
        [BreakdownDimension.Year] = "שנה",
        [BreakdownDimension.Gender] = "מגדר",
        [BreakdownDimension.City] = "עיר",
        [BreakdownDimension.AgeGroup] = "קבוצת גיל",
        [BreakdownDimension.Sector] = "מגזר"
    };

    public string Phrase(QueryDefinition def)
    {
        var sb = new StringBuilder();

        sb.Append(MetricPhrase(def.Metric));

        var population = PopulationPhrase(def.Population);
        if (population.Length > 0)
        {
            sb.Append(" של ").Append(population);
        }

        sb.Append(' ').Append(PeriodPhrase(def.Period));

        if (def.Breakdowns.Count > 0)
        {
            var dims = def.Breakdowns.Select(b => BreakdownLabels.GetValueOrDefault(b, b.ToString()));
            sb.Append(", בחלוקה לפי ").Append(string.Join(", ", dims));
        }

        return sb.ToString();
    }

    private static string MetricPhrase(Metric metric) => metric.Type switch
    {
        MetricType.Average => "השכר הממוצע",
        MetricType.Sum => "סך השכר",
        MetricType.Count => "כמות הרשומות",
        _ => "המדד"
    };

    private static string PopulationPhrase(Population p)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(p.Gender)) parts.Add(p.Gender!);
        if (!string.IsNullOrWhiteSpace(p.Sector)) parts.Add($"מהמגזר ה{p.Sector}");
        if (!string.IsNullOrWhiteSpace(p.AgeGroup)) parts.Add($"בגילאי {p.AgeGroup}");
        if (!string.IsNullOrWhiteSpace(p.City)) parts.Add($"ב{p.City}");
        return string.Join(" ", parts);
    }

    private static string PeriodPhrase(Period period) => period.Kind == PeriodKind.SingleYear
        ? $"בשנת {period.FromYear}"
        : $"בין השנים {period.FromYear}–{period.ToYear}";
}
