using System.Net;
using System.Threading.RateLimiting;
using Base.Application.Common;
using Microsoft.AspNetCore.RateLimiting;

namespace Base.API.Extensions;

public static class RateLimitingExtension
{
    public const string FixedPolicy = "fixed";
    public const string SlidingPolicy = "sliding";
    public const string TokenBucketPolicy = "token";
    public const string ConcurrencyPolicy = "concurrency";

    public static IServiceCollection AddRateLimitingExt(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // GLOBAL LIMITER - APPLIES TO ALL REQUESTS - EVERY REQUESTS PASS FROM THIS LIMITER EVEN POLICY NOT APPLIED
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // FIXED WINDOW POLICY - FIXED NUMBER OF REQUESTS PER TIME WINDOW
            options.AddFixedWindowLimiter(policyName: FixedPolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = 10;
                limiterOptions.Window = TimeSpan.FromSeconds(10);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 2;
            });

            // SLIDING WINDOW POLICY - SMOOTHER RATE LIMITING
            options.AddSlidingWindowLimiter(policyName: SlidingPolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = 30;
                limiterOptions.Window = TimeSpan.FromSeconds(30);
                limiterOptions.SegmentsPerWindow = 3;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 5;
            });

            // TOKEN BUCKET POLICY - ALLOWS BURSTS OF TRAFFIC
            options.AddTokenBucketLimiter(policyName: TokenBucketPolicy, limiterOptions =>
            {
                limiterOptions.TokenLimit = 50;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 10;
                limiterOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
                limiterOptions.TokensPerPeriod = 10;
                limiterOptions.AutoReplenishment = true;
            });

            // CONCURRENCY POLICY - LIMITS CONCURRENT REQUESTS
            options.AddConcurrencyLimiter(policyName: ConcurrencyPolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = 10;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 5;
            });

            // CUSTOM REJECTION RESPONSE
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? retryAfterValue.TotalSeconds
                    : 60;

                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                var serviceResult = ServiceResult.Fail(
                    "Too many requests. Please try again later.",
                    HttpStatusCode.TooManyRequests
                );

                await context.HttpContext.Response
                    .WriteAsJsonAsync(serviceResult, cancellationToken);
            };
        });

        return services;
    }
}


// USAGE
// WITH [EnableRateLimiting(RateLimitingExtension.SlidingPolicy)] WE CAN USE IT IN CONTROLLER LEVEL OR ACTION LEVEL