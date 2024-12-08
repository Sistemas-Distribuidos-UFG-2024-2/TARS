namespace Analysis.Database;

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDbConnection _dbConnection;

    public MongoDbContext(IMongoDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }


    public IRepository<T> GetRepository<T>() where T : BaseEntity
    {
        return new Repository<T>(_dbConnection);
    }
}