using Houston.Database;

namespace Houston.Services;

public interface ISensorsService
{
    Task<IList<T>> GetAll<T>(int limit, int offset) where T : BaseEntity;
}