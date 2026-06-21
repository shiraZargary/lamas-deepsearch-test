using DeepSearch.Infrastructure.Persistence.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DeepSearch.Infrastructure.Persistence;

/// <summary>
/// Central access point to the Deep Search MongoDB collections.
/// The fact collection is exposed as <see cref="BsonDocument"/> so the
/// aggregation-pipeline builder can compose dynamic group/projection stages.
/// </summary>
public sealed class MongoContext(IMongoDatabase database)
{
    public const string StatisticsFactCollection = "statistics_fact";
    public const string MetadataCollection = "metadata";
    public const string SavedQueriesCollection = "saved_queries";

    private readonly IMongoDatabase _database = database;

    public IMongoCollection<BsonDocument> StatisticsFact =>
        _database.GetCollection<BsonDocument>(StatisticsFactCollection);

    public IMongoCollection<MetadataDocument> Metadata =>
        _database.GetCollection<MetadataDocument>(MetadataCollection);

    public IMongoCollection<SavedQueryDocument> SavedQueries =>
        _database.GetCollection<SavedQueryDocument>(SavedQueriesCollection);
}
