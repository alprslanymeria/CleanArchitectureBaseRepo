
using Base.API.ExceptionHandlers;
using Base.API.Extensions;
using Base.API.Filters;
using Base.API.Middlewares;
using Base.API.ModelBinding;
using Base.Application;
using Base.Application.Common.Behaviors;
using Base.Caching;
using Base.Integration.Mapping;
using Base.Observability;
using Base.Storage;
using FluentValidation;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

// OPEN TELEMETRY
builder.AddOpenTelemetryLogExt();

// SERVICES
builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new FileUploadModelBinderProvider());
});
builder.Services.AddOpenApi();

// HEALTH CHECKS
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("SqlServer")!,
        name: "sqlserver",
        tags: ["db", "ready"]);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            var allowedOrigins = builder.Configuration
                .GetSection("AllowedOrigins")
                .Get<string[]>() ?? [];

            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});


builder.Services
    .AddPersistenceServicesExt(builder.Configuration)
    .AddOpenTelemetryServicesExt(builder.Configuration)
    .AddCachingServicesExt(builder.Configuration)
    .AddStorageServicesExt(builder.Configuration)
    .AddMappingServicesExt(assembliesToScan: [typeof(ApplicationAssembly).Assembly])
    .AddCustomTokenAuthExt(builder.Configuration)
    .AddApplicationServicesExt()
    .AddApiVersioningExt()
    .AddRateLimitingExt();

// MEDIATR WITH PIPELINE BEHAVIORS
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssembly).Assembly);
    cfg.AddOpenBehavior(typeof(ExceptionHandlerBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
});

// EXCEPTION HANDLERS
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// FLUENT VALIDATION
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssembly).Assembly);
builder.Services.AddFluentValidationAutoValidation(cfg =>
{

    cfg.OverrideDefaultResultFactoryWith<FluentValidationFilter>();
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// HEALTH CHECK ENDPOINTS
app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
});

// MIDDLEWARES
app.UseExceptionHandler(x => { });
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRateLimiter();
app.UseMiddleware<OpenTelemetryTraceIdMiddleware>();
app.UseMiddleware<RequestAndResponseActivityMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
