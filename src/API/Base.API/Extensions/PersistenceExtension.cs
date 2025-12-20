using App.Persistence;
using Base.Application.Contracts.Persistence;
using Base.Persistence;

namespace Base.API.Extensions;

public static class PersistenceExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {

        // CONNECTION STRING
        var connString = configuration.GetConnectionString("SqlServer");

        // DB CONTEXT
        services.AddDbContext<AppDbContext>(options =>
        {

            DbContextConfigurator.Configure(options, connString!, typeof(PersistenceAssembly).Assembly.FullName!);
        });

        // REPOSITORIES
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));


        return services;
    }
}
