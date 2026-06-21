using DeepSearch.Domain.Queries;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeepSearch.Infrastructure.Persistence.Documents;

/// <summary>Persistence model for the <c>saved_queries</c> collection.</summary>
public sealed class SavedQueryDocument
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>The query definition stored as a native sub-document.</summary>
    [BsonElement("definition")]
    public QueryDefinition Definition { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
}
