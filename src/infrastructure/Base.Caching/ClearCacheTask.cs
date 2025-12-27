using Base.Application.Contracts.Infrastructure.Caching;
using Base.Application.Contracts.Infrastructure.Tasks;

namespace Base.Caching;

public class ClearCacheTask(

    IStaticCacheManager staticCacheManager

    ) : IScheduleTask
{

    // FIELDS
    protected readonly IStaticCacheManager StaticCacheManager = staticCacheManager;

    // IMPLEMENTATION OF IScheduleTask
    public virtual async Task ExecuteAsync()
    {
        await StaticCacheManager.ClearAsync();
    }
}
