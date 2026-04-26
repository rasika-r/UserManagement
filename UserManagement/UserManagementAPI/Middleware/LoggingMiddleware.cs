using System.Diagnostics;

namespace UserManagementAPI.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Store the original response stream
            var originalBodyStream = context.Response.Body;

            // Create a new stream to capture response content
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                // Log incoming request
                var stopwatch = Stopwatch.StartNew();
                var request = context.Request;
                var requestPath = $"{request.Method} {request.Path}{request.QueryString}";

                _logger.LogInformation(
                    "Incoming Request: Method={Method}, Path={Path}, QueryString={QueryString}",
                    request.Method,
                    request.Path,
                    request.QueryString);

                try
                {
                    // Call the next middleware
                    await _next(context);
                }
                finally
                {
                    stopwatch.Stop();

                    // Log outgoing response
                    var response = context.Response;
                    _logger.LogInformation(
                        "Outgoing Response: Method={Method}, Path={Path}, StatusCode={StatusCode}, Duration={DurationMs}ms",
                        request.Method,
                        request.Path,
                        response.StatusCode,
                        stopwatch.ElapsedMilliseconds);

                    // Copy the response stream back to the original stream
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
        }
    }
}
