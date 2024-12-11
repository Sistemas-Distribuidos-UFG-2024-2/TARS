namespace Analysis.Database;

public interface IMongoDbContext
{
    IRepository<T> GetRepository<T>(string? collectionName = null) where T : BaseEntity;
}