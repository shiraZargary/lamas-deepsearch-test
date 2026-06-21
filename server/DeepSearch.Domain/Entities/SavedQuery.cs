using DeepSearch.Domain.Queries;

namespace DeepSearch.Domain.Entities;

/// <summary>A persisted, named query definition the user can re-run later.</summary>
public sealed class SavedQuery
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public QueryDefinition Definition { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
