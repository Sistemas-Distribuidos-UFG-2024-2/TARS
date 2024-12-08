using MongoDB.Driver;

namespace Houston.Database;

public interface IMongoDbConnection
{
    IMongoCollection<T> Collection<T>(string collectionName);
}