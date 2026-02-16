using Base.Application.Contracts.Persistence;
using Base.Persistence;
using Base.Persistence.Interceptors;

namespace Base.API.Extensions;

public static class PersistenceExtension
{
    public static IServiceCollection AddPersistenceServicesExt(this IServiceCollection services, IConfiguration configuration)
    {

        // CONNECTION STRING
        var connString = configuration.GetConnectionString("SqlServer");

        // HTTP CONTEXT ACCESSOR (REQUIRED FOR AUDIT INTERCEPTOR)
        services.AddHttpContextAccessor();

        // AUDIT INTERCEPTOR
        services.AddScoped<AuditSaveChangesInterceptor>();

        // DB CONTEXT WITH INTERCEPTOR
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();
            DbContextConfigurator.Configure(options, connString!, typeof(PersistenceAssembly).Assembly.FullName!);
            options.AddInterceptors(auditInterceptor);
        });

        // UNIT OF WORK
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
