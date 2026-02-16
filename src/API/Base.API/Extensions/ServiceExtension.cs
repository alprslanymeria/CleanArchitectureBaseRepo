using Base.Application.Contracts.Services;
using Base.Integration;

namespace Base.API.Extensions;

/// <summary>
/// EXTENSION FOR REGISTERING APPLICATION SERVICES WITH DEPENDENCY INJECTION.
/// NOTE: SERVICES THAT USE CQRS PATTERN ARE REGISTERED VIA MEDIATR AUTOMATICALLY.
/// THIS FILE ONLY CONTAINS SERVICES THAT STILL USE THE TRADITIONAL SERVICE PATTERN.
/// </summary>
public static class ServiceExtension
{
    public static IServiceCollection AddApplicationServicesExt(this IServiceCollection services)
    {
        // COMMON HANDLER SERVICES
        services.AddScoped<IFileStorageHelper, FileStorageHelper>();

        return services;
    }
}
