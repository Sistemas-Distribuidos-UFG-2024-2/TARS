using Houston.Database;
using Houston.Repositories;

namespace Houston.Services;

public class SensorsService : ISensorsService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SensorsService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    public async Task<IList<T>> GetAll<T>(int limit, int offset) where T : BaseEntity
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var scopedProcessingService =
            scope.ServiceProvider.GetRequiredService<ISensorsRepository<T>>();
        
        return await scopedProcessingService.GetAll(limit, offset);
    }
}