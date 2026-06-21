namespace DeepSearch.Infrastructure.Persistence;

/// <summary>
/// Strongly-typed MongoDB configuration bound from the "MongoDb" config section.
/// The connection string is supplied via configuration / environment variables /
/// GCP Secret Manager and is never committed to source control.
/// </summary>
public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "deepsearch";
}
