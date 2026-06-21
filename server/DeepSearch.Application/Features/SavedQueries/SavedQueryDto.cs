using DeepSearch.Domain.Queries;

namespace DeepSearch.Application.Features.SavedQueries;

/// <summary>Client-facing representation of a persisted query.</summary>
public sealed record SavedQueryDto(
    string Id,
    string Name,
    QueryDefinition Definition,
    DateTime CreatedAt);
