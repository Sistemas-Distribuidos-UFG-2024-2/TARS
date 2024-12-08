namespace Analysis.Database;

public interface IMongoDbContext
{
    IRepository<T> GetRepository<T>() where T : BaseEntity;
}