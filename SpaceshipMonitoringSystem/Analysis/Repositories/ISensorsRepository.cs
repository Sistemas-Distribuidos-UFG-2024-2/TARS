using Analysis.Database;

namespace Analysis.Repositories;

public interface ISensorsRepository<T> where T : BaseEntity
{
    Task Create(T sensor);
}