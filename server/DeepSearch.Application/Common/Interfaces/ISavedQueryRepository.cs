using DeepSearch.Domain.Entities;

namespace DeepSearch.Application.Common.Interfaces;

/// <summary>Persists and retrieves saved query definitions.</summary>
public interface ISavedQueryRepository
{
    Task<SavedQuery> AddAsync(SavedQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<SavedQuery>> GetAllAsync(CancellationToken cancellationToken);
    Task<SavedQuery?> GetByIdAsync(string id, CancellationToken cancellationToken);
}
