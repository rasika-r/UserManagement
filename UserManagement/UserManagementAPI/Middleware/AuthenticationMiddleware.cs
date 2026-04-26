using System.Text.Json;

namespace UserManagementAPI.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Check if route is public (doesn't require authentication)
            if (IsPublicRoute(path))
            {
                _logger.LogInformation("Public route accessed (no auth required): {Path}", path);
                await _next(context);
                return;
            }

            // For protected routes, check for Authorization header
            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                _logger.LogWarning("Missing Authorization header for protected route: {Path}", path);
                await RespondWithUnauthorized(context, "Missing Authorization header");
                return;
            }

            // Validate token format (Bearer token)
            var token = authHeader.ToString();
            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid Authorization header format for path: {Path}", path);
                await RespondWithUnauthorized(context, "Invalid Authorization header format. Expected 'Bearer <token>'");
                return;
            }

            // Extract token value
            var tokenValue = token.Substring("Bearer ".Length).Trim();

            // Validate token (simple validation - in production, verify JWT)
            if (!ValidateToken(tokenValue))
            {
                _logger.LogWarning("Invalid or expired token for path: {Path}", path);
                await RespondWithUnauthorized(context, "Invalid or expired token");
                return;
            }

            _logger.LogInformation("Valid token authenticated for path: {Path}", path);

            // Token is valid, continue to next middleware
            await _next(context);
        }

        private bool IsPublicRoute(string path)
        {
            // Define public routes that don't require authentication
            var publicPrefixes = new[] 
            { 
                "/swagger",
                "/health",
                "/api/health",
                "/.well-known"
            };

            return publicPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        private bool ValidateToken(string token)
        {
            // Simple token validation:
            // - Token must be at least 10 characters
            // - In production, implement JWT validation
            
            if (string.IsNullOrWhiteSpace(token) || token.Length < 10)
            {
                return false;
            }

            // For demo purposes, accept any token that's at least 10 chars
            // In production, validate JWT signature and expiration
            return true;
        }

        private static Task RespondWithUnauthorized(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var response = new UnauthorizedResponse
            {
                Error = message,
                TraceId = context.TraceIdentifier
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, jsonOptions);

            return context.Response.WriteAsync(json);
        }
    }

    public class UnauthorizedResponse
    {
        public string Error { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }
    }
}
