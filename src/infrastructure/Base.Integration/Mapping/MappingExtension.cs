using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Base.Integration.Mapping;

public static class MappingExtension
{
    public static IServiceCollection AddMappingServicesExt(this IServiceCollection services, Action<TypeAdapterConfig>? configure = null, params Assembly[] assembliesToScan)
    {
        // CHECK SERVICES
        ArgumentNullException.ThrowIfNull(services);

        // CONFIGURE
        var config = new TypeAdapterConfig();
        config.Default.NameMatchingStrategy(NameMatchingStrategy.Flexible);
        config.Default.IgnoreNullValues(true);

        // FIND FILES FROM ASSEMBLIES
        var toScan = (assembliesToScan is { Length: > 0 })
            ? assembliesToScan
            : [typeof(MappingExtension).Assembly];


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

        // REGISTER CONFIG AND MAPPER IN DI
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}