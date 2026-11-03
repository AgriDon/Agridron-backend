using Agridron.Backend.API.Shared.Domain.Model;
using AspNetProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Agridron.Backend.API.Shared.Interfaces.Rest.ProblemDetails;

/// <summary>
///     Translates a domain <see cref="Error" /> into an RFC 7807 Problem Details payload.
/// </summary>
/// <remarks>
///     This is the only place where an error category becomes an HTTP status code, so the domain
///     stays free of HTTP concerns.
/// </remarks>
public static class ErrorProblemDetailsMapper
{
    /// <summary>Maps the error category to its HTTP status code.</summary>
    public static int ToStatusCode(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    /// <summary>Maps the error category to a human-readable title.</summary>
    public static string ToTitle(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return error.Type switch
        {
            ErrorType.Validation => "Validation Error",
            ErrorType.NotFound => "Resource Not Found",
            ErrorType.Conflict => "Resource Conflict",
            ErrorType.BusinessRule => "Business Rule Violation",
            _ => "Internal Server Error"
        };
    }

    /// <summary>
    ///     Builds the Problem Details payload for the given error.
    /// </summary>
    /// <param name="error">The domain error.</param>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>The Problem Details payload.</returns>
    public static AspNetProblemDetails ToProblemDetails(this Error error, HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(context);

        var statusCode = error.ToStatusCode();

        var problemDetails = new AspNetProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = error.ToTitle(),
            Status = statusCode,
            Detail = error.Message,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["code"] = error.Code;

        if (!string.IsNullOrWhiteSpace(error.Details))
        {
            problemDetails.Extensions["details"] = error.Details;
        }

        return problemDetails;
    }
}
