using DeepSearch.Application.Common.Interfaces;

namespace DeepSearch.Infrastructure.FreeSearch;

/// <summary>
/// Pure selection logic mapping a configured provider name to a free-search service.
/// Kept separate from DI so it is trivially unit-testable.
/// </summary>
public static class FreeSearchServiceSelector
{
    public const string GeminiProvider = "Gemini";

    public static IFreeSearchService Select(string? provider, IFreeSearchService ruleBased, IFreeSearchService gemini)
        => string.Equals(provider, GeminiProvider, StringComparison.OrdinalIgnoreCase)
            ? gemini
            : ruleBased;
}
