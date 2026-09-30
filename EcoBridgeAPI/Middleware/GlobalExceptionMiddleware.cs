using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace EcoBridge.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            // Domain rule violations are expected — log at Warning, no stack trace noise.
            _logger.LogWarning(
                "Domain rule violation: {Code} on {Method} {Path}",
                ex.Code,
                context.Request.Method,
                context.Request.Path);

            await WriteResponseAsync(context, ResolveStatusCode(ex), ApiResult.Fail(ex.Code));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex,
                "Unhandled DbUpdateConcurrencyException on {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteResponseAsync(
                context,
                HttpStatusCode.Conflict,
                ApiResult.Fail(AppMessageCodes.CONCURRENT_UPDATE_ERROR));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception on {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                ApiResult.Fail(AppMessageCodes.INTERNAL_SERVER_ERROR));
        }
    }

    private static HttpStatusCode ResolveStatusCode(DomainException ex) => ex switch
    {
        DomainException.NotFoundException => HttpStatusCode.NotFound,           // 404
        DomainException.ValidationException => HttpStatusCode.BadRequest,         // 400
        DomainException.ConflictException => HttpStatusCode.Conflict,           // 409
        DomainException.ForbiddenException => HttpStatusCode.Forbidden,          // 403
        DomainException.UnauthorizedException => HttpStatusCode.Unauthorized,       // 401
        DomainException.FailureException => HttpStatusCode.InternalServerError,// 500
        _ => HttpStatusCode.InternalServerError
    };

    private static async Task WriteResponseAsync(HttpContext context, HttpStatusCode status, ApiResult response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var json = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(json);
    }
}