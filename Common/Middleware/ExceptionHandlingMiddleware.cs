using DulceAtardecer.Common.Exceptions;
using DulceAtardecer.Common.Responses;

namespace DulceAtardecer.Common.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            logger.LogWarning(ex, "Error de negocio: {Code}", ex.Code);
            IDictionary<string, string[]>? details = (ex as ValidationException)?.Errors;
            await WriteErrorAsync(context, ex.StatusCode, ex.Code, ex.Message, details);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Acceso no autorizado");
            await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, "UNAUTHORIZED", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado");
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "Ocurrió un error inesperado.");
        }
    }

    private static Task WriteErrorAsync(
        HttpContext context, int statusCode, string code, string message, IDictionary<string, string[]>? details = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        var body = new ApiErrorResponse(false, new ApiError(code, message, details));
        return context.Response.WriteAsJsonAsync(body);
    }
}
