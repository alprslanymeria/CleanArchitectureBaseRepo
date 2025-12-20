using Base.Application.Contracts.Infrastructure.Caching;
using Base.Application.Contracts.Infrastructure.Tasks;

namespace Base.Infrastructure.Caching;

public class ClearCacheTask(

    IStaticCacheManager staticCacheManager

    ) : IScheduleTask
{

    // FIELDS
    protected readonly IStaticCacheManager _staticCacheManager = staticCacheManager;

    // IMPLEMENTATION OF ISCHEDULETASK
    public virtual async Task ExecuteAsync()
    {
        await _staticCacheManager.ClearAsync();
    }
}
