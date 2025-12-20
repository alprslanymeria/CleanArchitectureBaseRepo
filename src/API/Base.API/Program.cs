using Base.API.ExceptionHandlers;
using Base.API.Extensions;
using Base.API.Filters;
using Base.API.Middlewares;
using Base.Application;
using Base.Application.Contracts.Infrastructure.Caching;
using Base.Domain.Options;
using Base.Infrastructure.Caching;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

// OPEN TELEMETRY
builder.AddOpenTelemetryLog();

// OPTIONS PATTERN
builder.Services.Configure<DistributedCacheConfig>(builder.Configuration.GetSection("DistributedCacheConfig"));
builder.Services.Configure<CacheConfig>(builder.Configuration.GetSection("CacheConfig"));

// SERVICES
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services
    .AddOpenTelemetryExtension(builder.Configuration)
    .AddCustomTokenAuth(builder.Configuration)
    .AddRepositories(builder.Configuration)
    .AddMapster();

// CACHING SERVICES
builder.Services.AddTransient(typeof(ICacheKeyStore<>), typeof(CacheKeyStore<>));
builder.Services.AddSingleton<ICacheKeyFactory, CacheKeyFactory>(); // --> FACTORY TO CREATE CACHE KEYS
builder.Services.AddSingleton<ICacheKeyManager, CacheKeyManager>();
// LOCKER
builder.Services.AddSingleton<ILocker, DistributedCacheLocker>();
// SHORT TERM
builder.Services.AddScoped<IShortTermCacheManager, PerRequestCacheManager>();
// REDIS
builder.Services.AddSingleton<IRedisConnectionWrapper, RedisConnectionWrapper>();
builder.Services.AddScoped<IStaticCacheManager, RedisCacheManager>();
builder.Services.AddSingleton<ICacheKeyService, RedisCacheManager>();



// EXCEPTION HANDLERS
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// FLUENT VALIDATION
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
})
    .AddFluentValidationAutoValidation(cfg =>
    {
        cfg.OverrideDefaultResultFactoryWith<FluentValidationFilter>();
    })
    .AddValidatorsFromAssembly(typeof(ApplicationAssembly).Assembly);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// MIDDLEWARES
app.UseExceptionHandler(x => { });
app.UseHttpsRedirection();
//app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseMiddleware<RequestAndResponseActivityMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();