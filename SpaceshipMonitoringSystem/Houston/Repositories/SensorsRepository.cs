using Houston.Database;
using MongoDB.Driver;

namespace Houston.Repositories;

public class SensorsRepository<T> : ISensorsRepository<T> where T : BaseEntity
{
    private readonly IRepository<T> _repository;

    public SensorsRepository(IMongoDbContext dbContext)
    {
        _repository = dbContext.GetRepository<T>();
    }

    public async Task<IList<T>> GetAll(int limit = 10, int offset = 0)
    {
        var filter = Builders<T>.Filter.Empty;
        var sort = Builders<T>.Sort.Descending("_id");
        var options = new FindOptions<T> { Limit = limit, Skip = offset, Sort = sort };
        
        return await _repository.Find(filter, options);
    }
}
