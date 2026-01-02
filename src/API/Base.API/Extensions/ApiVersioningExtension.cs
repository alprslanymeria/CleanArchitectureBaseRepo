using Asp.Versioning;

namespace Base.API.Extensions;

public static class ApiVersioningExtension
{
    public static IServiceCollection AddApiVersioningExt(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // DEFAULT VERSION IF NOT SPECIFIED
            options.DefaultApiVersion = new ApiVersion(1, 0);

            // USE DEFAULT VERSION WHEN VERSION IS NOT SPECIFIED
            options.AssumeDefaultVersionWhenUnspecified = true;

            // REPORT AVAILABLE API VERSIONS IN RESPONSE HEADERS
            options.ReportApiVersions = true;

            // READ API VERSION FROM URL SEGMENT, QUERY STRING, OR HEADER
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("X-Api-Version")
            );
        })
        .AddApiExplorer(options =>
        {
            // FORMAT: 'v'major[.minor][-status]
            options.GroupNameFormat = "'v'VVV";

            // SUBSTITUTE THE VERSION IN THE URL
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}

// USAGE
// ROUTE SEGMENT
    // [Route("api/v{version:apiVersion}/[controller]")]
    // [ApiVersion("1.0")]
    // GET /api/v1/DeckWord

// QUERY STRING
    // [Route("api/[controller]")]
    // [ApiVersion("1.0")]
    // GET /api/DeckWord?api-version=1.0

// HEADER
    // [Route("api/[controller]")]
    // [ApiVersion("1.0")]
    // GET /api/DeckWord
    // X-Api-Version: 1.0


// IF ONLY ONE [ApiVersion("1.0")] IS SPECIFIED, ALL ACTION METHODS WILL HAVE THAT VERSION.
// THE ONLY THING THAT IS VALID FOR ALL THREE IS THAT THEY CAN CONTAIN MORE THAN ONE [ApiVersion("1.0")].
// IF MULTIPLE [ApiVersion("1.0"), ApiVersion("2.0")] ARE SPECIFIED, ACTION METHODS MUST MAP TO A SPECIFIC VERSION WITH [MapToApiVersion("2.0")]
// BEST PRACTICE IS THAT A SINGLE CONTROLLER SHOULD ONLY CONTAIN ONE VERSION.