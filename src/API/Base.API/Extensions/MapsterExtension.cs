using System.Reflection;
using Mapster;
using MapsterMapper;

namespace Base.API.Extensions;

public static class MapsterExtension
{
    public static IServiceCollection AddMapster(this IServiceCollection services, Action<TypeAdapterConfig>? configure = null, params Assembly[] assembliesToScan)
    {
        // CHECK SERVICES
        ArgumentNullException.ThrowIfNull(services);

        // CONFIGURE MAPSTER
        var config = new TypeAdapterConfig();
        config.Default.NameMatchingStrategy(NameMatchingStrategy.Flexible);
        config.Default.IgnoreNullValues(true);

        // FIND FILES FROM ASSEMBLIES
        var toScan = (assembliesToScan is { Length: > 0 })
            ? assembliesToScan
            : [typeof(MapsterExtension).Assembly];


        // SCAN FILES
        foreach (var asm in toScan.Distinct())
        {
            if (asm is not null)
            {
                config.Scan(asm);
            }
        }

        // ALLOW CALLER TO FURTHER TWEAK THE CONFIG
        configure?.Invoke(config);

        //// REGISTER CONFIG AND MAPPER IN DI
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}