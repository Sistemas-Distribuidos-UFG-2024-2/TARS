using MongoDB.Driver;
using Analysis.Database;

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
}
