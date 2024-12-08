using MongoDB.Driver;

namespace NotificationSystem.Database;

public class Repository<T>: IRepository<T> where T : BaseEntity
{
    public IMongoCollection<T> Collection { get; }

    public Repository(IMongoDbConnection connection)
    {
        Collection = connection.Collection<T>(typeof(T).Name.ToLower());
    }
    
    public async Task InsertOne(T entity, InsertOneOptions? options = null, CancellationToken cancellationToken = default)
    {
        await Collection.InsertOneAsync(entity, options, cancellationToken);
    }

    public async Task InsertMany(ICollection<T> entities, InsertManyOptions? options = null, CancellationToken cancellationToken = default)
    {
        await Collection.InsertManyAsync(entities, options, cancellationToken);
    }

    public async Task<T?> FindOne(FilterDefinition<T> filter, FindOptions<T>? options = null, CancellationToken cancellationToken = default)
    {
        var cursor = await Collection.FindAsync(filter, options, cancellationToken);
        
        return await cursor.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IList<T>> Find(FilterDefinition<T> filter, FindOptions<T>? options = null, CancellationToken cancellationToken = default)
    {
        var cursor = await Collection.FindAsync(filter, options, cancellationToken);
        return await cursor.ToListAsync(cancellationToken);
    }

    public async Task<T?> FindOneAndUpdate(FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T>? options = null,
        CancellationToken cancellationToken = default)
    {
        return await Collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
    }

    public async Task<bool> UpdateOne(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result =  await Collection.UpdateOneAsync(filter, update, options, cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteOne(FilterDefinition<T> filter, DeleteOptions? options = null, CancellationToken cancellationToken = default)
    {
        var result = await Collection.DeleteOneAsync(filter, options, cancellationToken);
        return result.DeletedCount > 0;
    }

}