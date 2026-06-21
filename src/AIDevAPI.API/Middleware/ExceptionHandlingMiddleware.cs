using System.Net;
using System.Text.Json;
using AIDevAPI.Application.Common;
using AIDevAPI.Application.Common.Exceptions;

namespace AIDevAPI.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            NotFoundException => new ApiResponse { Success = false, Message = exception.Message },
            BadRequestException => new ApiResponse { Success = false, Message = exception.Message },
            UnauthorizedAppException => new ApiResponse { Success = false, Message = exception.Message },
            ForbiddenException => new ApiResponse { Success = false, Message = exception.Message },
            ValidationAppException validationEx => new ApiResponse
            {
                Success = false,
                Message = "Validasiya xətası baş verdi.",
                Errors = validationEx.Errors
            },
            _ => new ApiResponse { Success = false, Message = "Daxili server xətası baş verdi." }
        };

        context.Response.StatusCode = exception switch
        {
            NotFoundException => (int)HttpStatusCode.NotFound,
            BadRequestException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAppException => (int)HttpStatusCode.Unauthorized,
            ForbiddenException => (int)HttpStatusCode.Forbidden,
            ValidationAppException => (int)HttpStatusCode.UnprocessableEntity,
            _ => (int)HttpStatusCode.InternalServerError
        };

        if (context.Response.StatusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Gözlənilməyən xəta baş verdi.");
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
