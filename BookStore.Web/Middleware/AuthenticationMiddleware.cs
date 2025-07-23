using System.Net;

namespace BookStore.Web.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        // Routes that don't require authentication
        private readonly string[] _publicRoutes = {
            "/",
            "/home",
            "/account/login",
            "/account/register",
            "/account/logout",
            "/debug"
        };

        // Routes that require Admin role
        private readonly string[] _adminOnlyRoutes = {
            "/books",
            "/authors",
            "/categories"
        };

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Check if this is a public route
            if (IsPublicRoute(path))
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated
            var token = context.Session.GetString("Token");
            if (string.IsNullOrEmpty(token) || IsTokenExpired(token))
            {
                // Clear session if token is expired
                if (!string.IsNullOrEmpty(token) && IsTokenExpired(token))
                {
                    context.Session.Clear();
                }

                // Redirect to login page
                var returnUrl = context.Request.Path + context.Request.QueryString;
                context.Response.Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                return;
            }

            // Check if route requires Admin role
            if (IsAdminOnlyRoute(path))
            {
                var isAdmin = context.Session.GetString("IsAdmin") == "True";
                if (!isAdmin)
                {
                    // User is authenticated but not admin
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Access Denied: Admin role required");
                    return;
                }
            }

            await _next(context);
        }

        private bool IsPublicRoute(string path)
        {
            // Check exact matches and prefixes
            return _publicRoutes.Any(route =>
                path.Equals(route, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(route + "/", StringComparison.OrdinalIgnoreCase)) ||
                path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsAdminOnlyRoute(string path)
        {
            return _adminOnlyRoutes.Any(route =>
                path.Equals(route, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(route + "/", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsTokenExpired(string token)
        {
            try
            {
                // Simple check for JWT token format and expiration
                var parts = token.Split('.');
                if (parts.Length != 3) return true;

                var payload = parts[1];
                // Add padding if needed
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var jsonBytes = Convert.FromBase64String(payload);
                var jsonString = System.Text.Encoding.UTF8.GetString(jsonBytes);
                var json = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

                if (json != null && json.ContainsKey("exp"))
                {
                    var expElement = (System.Text.Json.JsonElement)json["exp"];
                    var exp = expElement.GetInt64();
                    var expDateTime = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                    return expDateTime <= DateTime.UtcNow;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking token expiration");
                return true;
            }
        }
    }

    // Extension method to register the middleware
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
