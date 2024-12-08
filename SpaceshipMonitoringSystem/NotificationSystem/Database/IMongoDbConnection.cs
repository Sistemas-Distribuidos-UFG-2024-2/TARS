using MongoDB.Driver;

namespace NotificationSystem.Database;

public interface IMongoDbConnection
{
    IMongoCollection<T> Collection<T>(string collectionName);
}