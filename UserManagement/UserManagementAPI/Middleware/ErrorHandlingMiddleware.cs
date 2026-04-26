using System.Net;
using System.Text.Json;

namespace UserManagementAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred: {ExceptionMessage}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();
            var statusCode = HttpStatusCode.InternalServerError;

            switch (exception)
            {
                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Error = "Invalid argument: " + argEx.Message;
                    break;

                case InvalidOperationException invalidEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Error = "Invalid operation: " + invalidEx.Message;
                    break;

                case KeyNotFoundException notFoundEx:
                    statusCode = HttpStatusCode.NotFound;
                    response.Error = "Resource not found: " + notFoundEx.Message;
                    break;

                case UnauthorizedAccessException unauthorizedEx:
                    statusCode = HttpStatusCode.Unauthorized;
                    response.Error = "Unauthorized: " + unauthorizedEx.Message;
                    break;

                default:
                    response.Error = "An unexpected error occurred. Please try again later.";
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, jsonOptions);

            return context.Response.WriteAsync(json);
        }
    }

    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }
    }
}
