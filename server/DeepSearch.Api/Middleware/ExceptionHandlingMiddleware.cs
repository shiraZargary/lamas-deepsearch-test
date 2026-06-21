using System.Net;
using System.Text.Json;
using FluentValidation;

namespace DeepSearch.Api.Middleware;

/// <summary>
/// Converts unhandled exceptions into a consistent JSON error contract.
/// Validation failures map to 400 with field-level details; everything else to 500.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for {Path}", context.Request.Path);
            await WriteResponseAsync(context, HttpStatusCode.BadRequest, "Validation failed",
                ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }));
        }
        catch (DeepSearch.Application.Common.Exceptions.NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found for {Path}", context.Request.Path);
            await WriteResponseAsync(context, HttpStatusCode.NotFound, ex.Message, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            await WriteResponseAsync(context, HttpStatusCode.InternalServerError,
                "An unexpected error occurred.", null);
        }
    }

    private static async Task WriteResponseAsync(
        HttpContext context, HttpStatusCode status, string message, object? details)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var payload = JsonSerializer.Serialize(new
        {
            status = (int)status,
            error = message,
            details,
            traceId = context.TraceIdentifier
        });

        await context.Response.WriteAsync(payload);
    }
}
