using Base.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Base.Persistence.Interceptors;

/// <summary>
/// EF CORE INTERCEPTOR THAT AUTOMATICALLY SETS AUDIT FIELDS (CreatedAt)
/// FOR ENTITIES IMPLEMENTING IAuditableEntity
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(

        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditFields(DbContext? context)
    {
        if (context is null) return;

        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            entry.Entity.CreatedAt = entry.State switch
            {
                EntityState.Added => utcNow,
                _ => entry.Entity.CreatedAt
            };
        }
    }
}
