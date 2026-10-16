using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Common.Exceptions;

namespace Ordering.API.Infrastructure;

/// <summary>
/// Turns application exceptions into RFC 7807 problem responses (400 for validation
/// failures, 404 for missing orders) instead of letting them surface as 500 errors.
/// </summary>
public sealed class ApplicationExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService problemDetailsService;

    public ApplicationExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        this.problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch
        {
            ValidationException validation => new ValidationProblemDetails(validation.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            },
            NotFoundException notFound => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource not found.",
                Detail = notFound.Message
            },
            _ => null
        };

        if (problem is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}
