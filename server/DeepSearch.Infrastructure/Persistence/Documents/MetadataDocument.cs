using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeepSearch.Infrastructure.Persistence.Documents;

/// <summary>Persistence model for the <c>metadata</c> collection.</summary>
public sealed class MetadataDocument
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId Id { get; set; }

    [BsonElement("kind")]
    public string Kind { get; set; } = string.Empty;

    [BsonElement("code")]
    public string Code { get; set; } = string.Empty;

    [BsonElement("label")]
    public string Label { get; set; } = string.Empty;

    [BsonElement("type")]
    [BsonIgnoreIfNull]
    public string? Type { get; set; }

    [BsonElement("field")]
    [BsonIgnoreIfNull]
    public string? Field { get; set; }

    [BsonElement("values")]
    [BsonIgnoreIfNull]
    public List<string>? Values { get; set; }
}
