using Analysis.Database;
using MongoDB.Bson;

namespace Analysis.Repositories;

public interface ISensorsRepository<T> where T : BaseEntity
{
    Task Create(T sensor);
    Task<IList<T>> GetAll();
    Task<T?> GetById(ObjectId id);
}