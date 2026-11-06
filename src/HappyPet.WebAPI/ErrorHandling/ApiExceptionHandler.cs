using HappyPet.Application.Exceptions;
using HappyPet.WebAPI.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace HappyPet.WebAPI.ErrorHandling
{
    /// <summary>
    /// Converts unhandled exceptions into the unified API error envelope.
    /// Unexpected exceptions never expose internal details to the client.
    /// </summary>
    public sealed class ApiExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ApiExceptionHandler> _logger;

        public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // The client disconnected; there is nobody to write a response to.
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                return true;
            }

            var (statusCode, code, message) = Map(exception);

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception. TraceId: {TraceId}",
                    httpContext.TraceIdentifier);
            }
            else
            {
                _logger.LogInformation(
                    "Request failed with {Code}. TraceId: {TraceId}",
                    code,
                    httpContext.TraceIdentifier);
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(
                new ApiErrorResponse
                {
                    Code = code,
                    Message = message,
                    TraceId = httpContext.TraceIdentifier
                },
                cancellationToken);

            return true;
        }

        private static (int StatusCode, string Code, string Message) Map(Exception exception)
        {
            return exception switch
            {
                AirsAnimalNotFoundException ex => (StatusCodes.Status400BadRequest, ex.Code, ex.Message),
                AirsOwnerMismatchException ex => (StatusCodes.Status400BadRequest, ex.Code, ex.Message),
                AirsUnavailableException ex => (StatusCodes.Status503ServiceUnavailable, ex.Code, ex.Message),
                EmailNotVerifiedException ex => (StatusCodes.Status403Forbidden, "EMAIL_NOT_VERIFIED", ex.Message),
                UnauthorizedAccessException ex => (StatusCodes.Status401Unauthorized, "UNAUTHORIZED", ex.Message),
                KeyNotFoundException ex => (StatusCodes.Status404NotFound, "NOT_FOUND", ex.Message),
                InvalidOperationException ex => (StatusCodes.Status400BadRequest, "BUSINESS_RULE_VIOLATION", ex.Message),
                ArgumentException ex => (StatusCodes.Status400BadRequest, "INVALID_ARGUMENT", ex.Message),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "INTERNAL_ERROR",
                    "დაფიქსირდა შიდა შეცდომა. გთხოვთ სცადოთ მოგვიანებით.")
            };
        }
    }
}
