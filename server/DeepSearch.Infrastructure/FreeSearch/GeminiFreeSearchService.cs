using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Domain.Queries;
using Microsoft.Extensions.Options;

namespace DeepSearch.Infrastructure.FreeSearch;

/// <summary>
/// Real free-search service using the Gemini API (free tier). Sends the question
/// with a strict instruction to return JSON matching <see cref="QueryDefinition"/>,
/// then validated by the same pipeline as the structured builder. Swapped in by
/// setting <c>FreeSearch:Provider = "Gemini"</c> — no other code changes.
/// </summary>
public sealed class GeminiFreeSearchService(HttpClient httpClient, IOptions<FreeSearchSettings> options)
    : IFreeSearchService
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly FreeSearchSettings _settings = options.Value;

    public async Task<FreeSearchResult> InterpretAsync(string question, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.Gemini.ApiKey))
        {
            throw new InvalidOperationException(
                "מפתח Gemini אינו מוגדר. הגדר FreeSearch:Gemini:ApiKey כדי להשתמש בספק זה.");
        }

        var prompt = BuildPrompt(question);
        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { responseMimeType = "application/json", temperature = 0.0 }
        };

        var url = $"{BaseUrl}/{_settings.Gemini.Model}:generateContent?key={_settings.Gemini.ApiKey}";
        using var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken);
        var json = payload?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
                   ?? throw new InvalidOperationException("תגובת Gemini ריקה");

        var definition = JsonSerializer.Deserialize<QueryDefinition>(json, JsonOptions)
                         ?? throw new InvalidOperationException("לא ניתן לפענח את תגובת Gemini");

        return new FreeSearchResult(definition, ["פורש באמצעות Gemini"]);
    }

    private static string BuildPrompt(string question) =>
        $$"""
        You convert a Hebrew statistics question into a strict JSON object.
        Output ONLY JSON, no markdown, matching exactly this schema:
        {
          "population": { "gender": string|null, "ageGroup": string|null, "city": string|null, "sector": string|null },
          "metric": { "type": "Average"|"Count"|"Sum", "field": "income"|null },
          "period": { "kind": "SingleYear"|"Range", "fromYear": number, "toYear": number },
          "breakdowns": ["Year"|"Gender"|"City"|"AgeGroup"|"Sector"]
        }
        Rules: gender ∈ {"נשים","גברים"}; city is a Hebrew city name; for Average/Sum set "field":"income";
        for Count set "field":null; if a single year, kind="SingleYear" and fromYear=toYear.
        Question: "{{question}}"
        """;

    private sealed record GeminiResponse([property: JsonPropertyName("candidates")] List<Candidate>? Candidates);
    private sealed record Candidate([property: JsonPropertyName("content")] Content? Content);
    private sealed record Content([property: JsonPropertyName("parts")] List<Part>? Parts);
    private sealed record Part([property: JsonPropertyName("text")] string? Text);
}
