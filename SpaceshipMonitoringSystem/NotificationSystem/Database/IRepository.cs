using MongoDB.Driver;

namespace NotificationSystem.Database;

public interface IRepository<T> where T : BaseEntity
{
    Task InsertOne(T entity, InsertOneOptions? options = null, CancellationToken cancellationToken = default);

    Task InsertMany(ICollection<T> entities, InsertManyOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<T?> FindOne(FilterDefinition<T> filter, FindOptions<T>? options = null,
        CancellationToken cancellationToken = default);

    Task<IList<T>> Find(FilterDefinition<T> filter, FindOptions<T>? options = null,
        CancellationToken cancellationToken = default);

    Task<T?> FindOneAndUpdate(FilterDefinition<T> filter, UpdateDefinition<T> update,
        FindOneAndUpdateOptions<T>? options = null, CancellationToken cancellationToken = default);

    Task<bool> UpdateOne(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteOne(FilterDefinition<T> filter, DeleteOptions? options = null,
        CancellationToken cancellationToken = default);

    IMongoCollection<T> Collection { get; }
}