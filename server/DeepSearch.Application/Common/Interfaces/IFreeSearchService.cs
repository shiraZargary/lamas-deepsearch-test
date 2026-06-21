using DeepSearch.Domain.Queries;

namespace DeepSearch.Application.Common.Interfaces;

/// <summary>
/// Result of interpreting a free-text question: the structured definition plus
/// human-readable notes explaining how the text was understood (shown to the user
/// for confirmation before running). <paramref name="Recognized"/> is false when
/// no meaningful terms were detected and the definition is only a blind default.
/// </summary>
public sealed record FreeSearchResult(
    QueryDefinition Definition,
    IReadOnlyList<string> Notes,
    bool Recognized = true);

/// <summary>
/// "Free search" feature: interprets a free-text (Hebrew) question into a
/// <see cref="QueryDefinition"/>. Implementations are interchangeable
/// (rule-based, Gemini, …) and selected via configuration; consumers never depend
/// on a concrete implementation.
/// </summary>
public interface IFreeSearchService
{
    Task<FreeSearchResult> InterpretAsync(string question, CancellationToken cancellationToken);
}
