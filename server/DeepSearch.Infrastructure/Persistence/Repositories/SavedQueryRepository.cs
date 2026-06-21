using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Domain.Entities;
using DeepSearch.Infrastructure.Persistence.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DeepSearch.Infrastructure.Persistence.Repositories;

/// <summary>Stores saved queries in the <c>saved_queries</c> collection.</summary>
public sealed class SavedQueryRepository(MongoContext context) : ISavedQueryRepository
{
    private readonly MongoContext _context = context;

    public async Task<SavedQuery> AddAsync(SavedQuery query, CancellationToken cancellationToken)
    {
        var doc = new SavedQueryDocument
        {
            Name = query.Name,
            Definition = query.Definition,
            CreatedAt = query.CreatedAt
        };

        await _context.SavedQueries.InsertOneAsync(doc, cancellationToken: cancellationToken);
        query.Id = doc.Id.ToString();
        return query;
    }

    public async Task<IReadOnlyList<SavedQuery>> GetAllAsync(CancellationToken cancellationToken)
    {
        var docs = await _context.SavedQueries
            .Find(FilterDefinition<SavedQueryDocument>.Empty)
            .SortByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(ToEntity).ToList();
    }

    public async Task<SavedQuery?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        var doc = await _context.SavedQueries
            .Find(d => d.Id == objectId)
            .FirstOrDefaultAsync(cancellationToken);

        return doc is null ? null : ToEntity(doc);
    }

    private static SavedQuery ToEntity(SavedQueryDocument doc) => new()
    {
        Id = doc.Id.ToString(),
        Name = doc.Name,
        Definition = doc.Definition,
        CreatedAt = doc.CreatedAt
    };
}
