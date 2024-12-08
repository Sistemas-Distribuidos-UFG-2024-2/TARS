using MongoDB.Driver;

namespace Analysis.Database;

public interface IMongoDbConnection
{
    IMongoCollection<T> Collection<T>(string collectionName);
}