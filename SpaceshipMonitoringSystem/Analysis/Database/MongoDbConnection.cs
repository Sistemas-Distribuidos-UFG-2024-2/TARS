using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;

namespace Analysis.Database;

public class MongoDbConnection : IMongoDbConnection, IDisposable
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;

    public MongoDbConnection(IMongoDbConfig config, ILoggerFactory loggerFactory)
    {
        var settings = MongoClientSettings.FromConnectionString(config.ConnectionString);
        settings.LoggingSettings = new LoggingSettings(loggerFactory);
        
        _client = new MongoClient(settings);
        _database = _client.GetDatabase(config.DatabaseName);
    }
    
    public IMongoCollection<T> Collection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    public void Dispose()
    {
        ClusterRegistry.Instance.UnregisterAndDisposeCluster(_client.Cluster);
        _client.Dispose();
    }
}