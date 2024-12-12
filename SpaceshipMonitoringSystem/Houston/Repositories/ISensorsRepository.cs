using Houston.Database;

namespace Houston.Repositories;

public interface ISensorsRepository<T> where T : BaseEntity
{
    Task<IList<T>> GetAll(int limit = 10, int offset = 0);
}