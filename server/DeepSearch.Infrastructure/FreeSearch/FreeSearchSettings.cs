namespace DeepSearch.Infrastructure.FreeSearch;

/// <summary>Configuration for the free-search feature (bound from "FreeSearch").</summary>
public sealed class FreeSearchSettings
{
    public const string SectionName = "FreeSearch";

    /// <summary>"RuleBased" (default, offline) or "Gemini".</summary>
    public string Provider { get; set; } = "RuleBased";

    public GeminiSettings Gemini { get; set; } = new();

    public sealed class GeminiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-1.5-flash";
    }
}
