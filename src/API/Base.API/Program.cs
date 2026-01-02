using Base.API.ExceptionHandlers;
using Base.API.Extensions;
using Base.API.Filters;
using Base.API.Middlewares;
using Base.Application;
using Base.Caching;
using Base.Integration.Mapping;
using Base.Observability;
using Base.Storage;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

// OPEN TELEMETRY
builder.AddOpenTelemetryLogExt();

// SERVICES
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services
    .AddPersistenceServicesExt(builder.Configuration)
    .AddOpenTelemetryServicesExt(builder.Configuration)
    .AddCachingServicesExt(builder.Configuration)
    .AddStorageServicesExt()
    .AddMappingServicesExt()
    .AddCustomTokenAuthExt(builder.Configuration)
    .AddOptionsPatternExt(builder.Configuration)
    .AddApplicationServicesExt()
    .AddApiVersioningExt()
    .AddRateLimitingExt();

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
app.UseRateLimiter();
app.UseMiddleware<RequestAndResponseActivityMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();