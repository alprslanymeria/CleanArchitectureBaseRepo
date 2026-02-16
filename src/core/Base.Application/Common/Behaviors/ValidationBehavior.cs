using Base.Application.Common;
using FluentValidation;
using MediatR;

namespace Base.Application.Common.Behaviors;

/// <summary>
/// VALIDATION BEHAVIOR FOR MEDIATR PIPELINE.
/// AUTOMATICALLY VALIDATES REQUESTS USING FLUENT VALIDATION BEFORE HANDLER EXECUTION.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(

        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken

        )
    {
        // FAST FAIL
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .Select(e => e.ErrorMessage)
            .ToList();

        if (failures.Count > 0)
        {
            return (TResponse)(object)ServiceResult.Fail(failures);
        }

        return await next(cancellationToken);
    }
}
