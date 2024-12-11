using Analysis.Database;
using MongoDB.Bson;
using Analysis.Entities;

namespace Analysis.Repositories;

public interface ISensorsRepository<T> where T : BaseEntity
{
    Task Create(Sensor sensor);
    Task<IList<Sensor>> GetAll();
    Task<Sensor?> GetById(ObjectId id);
    // Task<Person?> Update(ObjectId id, string? name, string? email);
    // Task<bool> Delete(ObjectId id);
}