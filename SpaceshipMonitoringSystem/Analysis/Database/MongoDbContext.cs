namespace Analysis.Database;

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDbConnection _dbConnection;

    public MongoDbContext(IMongoDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }


    public IRepository<T> GetRepository<T>(string? collectionName = null) where T : BaseEntity
    {
        return new Repository<T>(_dbConnection, collectionName);
    }
}