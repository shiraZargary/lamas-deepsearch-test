using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Infrastructure.FreeSearch;
using DeepSearch.Infrastructure.Persistence;
using DeepSearch.Infrastructure.Persistence.Repositories;
using DeepSearch.Infrastructure.Querying;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace DeepSearch.Infrastructure;

/// <summary>
/// Registers the Infrastructure layer: MongoDB client/database and (in later phases)
/// repositories, the aggregation-pipeline builder and the LLM parser implementations.
/// </summary>
public static class DependencyInjection
{
    private static bool _conventionsRegistered;

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        RegisterBsonConventions();

        services.Configure<MongoDbSettings>(
            configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddScoped<IMongoDatabase>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return sp.GetRequiredService<IMongoClient>().GetDatabase(settings.Database);
        });

        services.AddScoped<MongoContext>();

        services.AddSingleton<AggregationPipelineBuilder>();
        services.AddScoped<IQueryExecutor, MongoQueryExecutor>();
        services.AddScoped<IMetadataRepository, MetadataRepository>();
        services.AddScoped<ISavedQueryRepository, SavedQueryRepository>();

        AddFreeSearch(services, configuration);

        return services;
    }

    /// <summary>
    /// Registers both free-search implementations and resolves <see cref="IFreeSearchService"/>
    /// based on <c>FreeSearch:Provider</c> — swapping to Gemini needs only a config change.
    /// </summary>
    private static void AddFreeSearch(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FreeSearchSettings>(configuration.GetSection(FreeSearchSettings.SectionName));

        services.AddScoped<RuleBasedFreeSearchService>();
        services.AddHttpClient<GeminiFreeSearchService>();

        services.AddScoped<IFreeSearchService>(sp =>
        {
            var provider = configuration[$"{FreeSearchSettings.SectionName}:Provider"];
            return FreeSearchServiceSelector.Select(
                provider,
                sp.GetRequiredService<RuleBasedFreeSearchService>(),
                sp.GetRequiredService<GeminiFreeSearchService>());
        });
    }

    /// <summary>
    /// Camel-cases element names and stores enums as strings so the C# documents
    /// line up with the JS-seeded data and remain human-readable in the database.
    /// </summary>
    private static void RegisterBsonConventions()
    {
        if (_conventionsRegistered)
        {
            return;
        }

        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new EnumRepresentationConvention(MongoDB.Bson.BsonType.String),
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register("DeepSearchConventions", pack, _ => true);
        _conventionsRegistered = true;
    }
}

