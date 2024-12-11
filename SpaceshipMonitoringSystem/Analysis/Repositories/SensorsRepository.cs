using MongoDB.Bson;
using MongoDB.Driver;
using Analysis.Database;
using Analysis.Entities;
using Microsoft.AspNetCore.Components.Web;

namespace Analysis.Repositories;

public class SensorsRepository<T> : ISensorsRepository<T> where T : BaseEntity
{
    private readonly IRepository<T> _repository;

    public SensorsRepository(IMongoDbContext dbContext)
    {
        _repository = dbContext.GetRepository<T>();
    }

    public async Task Create(T sensor)
    {
        await _repository.InsertOne(sensor);
    }

    public async Task<IList<T>> GetAll()
    {
        var filter = Builders<T>.Filter.Empty;
        return await _repository.Find(filter);
    }

    public async Task<T?> GetById(ObjectId id)
    {
        var filter = Builders<T>.Filter.Eq(s => s.Id, id);
        return await _repository.FindOne(filter);
    }
}
