using MongoDB.Bson;
using MongoDB.Driver;
using Analysis.Database;
using Analysis.Entities;

namespace Analysis.Repositories;

public class SensorsRepository : IRepository
{
    private readonly IRepository<Sensor> _repository;

    public SensorsRepository(IMongoDbConnection dbContext)
    {
        _repository = connection.GetRepository<Sensor>();
    }

    public async Task Create(Sensor sensor)
    {
        await _repository.InsertOne(sensor);
    }

    public async Task<List<Sensor>> GetAll()
    {
        var filter = Builders<Sensor>.Filter.Empty;
        return await _repository.Find(filter);
    }

    public async Task<Sensor?> GetById(ObjectId id)
    {
        var filter = Builders<Sensor>.Filter.Eq(s => s.Id, id);
        return await _repository.FindOne(filter);
    }
}
