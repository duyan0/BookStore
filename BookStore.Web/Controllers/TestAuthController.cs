using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BookStore.Web.Controllers
{
    // DISABLED FOR SECURITY: Only enable in Development environment
    #if DEBUG
    public class TestAuthController : Controller
    #else
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TestAuthController : Controller
    #endif
    {
        private readonly ILogger<TestAuthController> _logger;

        public TestAuthController(ILogger<TestAuthController> logger)
        {
            _logger = logger;
        }

        // Override all actions to require Admin role
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            var hasToken = !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));

            if (!hasToken || !isAdmin)
            {
                context.Result = RedirectToAction("Login", "Account");
                return;
            }

            base.OnActionExecuting(context);
        }

        // GET: TestAuth/TokenInfo
        public IActionResult TokenInfo()
        {
            var token = HttpContext.Session.GetString("Token");
            
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { error = "No token found in session" });
            }

            try
            {
                var tokenInfo = ParseJwtToken(token);
                return Json(new 
                { 
                    success = true,
                    token_length = token.Length,
                    session_data = new
                    {
                        Username = HttpContext.Session.GetString("Username"),
                        FullName = HttpContext.Session.GetString("FullName"),
                        IsAdmin = HttpContext.Session.GetString("IsAdmin"),
                        UserId = HttpContext.Session.GetInt32("UserId")
                    },
                    parsed_token = tokenInfo
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // POST: TestAuth/SetTestToken
        [HttpPost]
        public IActionResult SetTestToken([FromBody] TokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    return Json(new { error = "Token is required" });
                }

                // Remove "Bearer " prefix if present
                var cleanToken = request.Token.StartsWith("Bearer ") 
                    ? request.Token.Substring(7) 
                    : request.Token;

                // Parse token to extract user info
                var tokenInfo = ParseJwtToken(cleanToken);
                
                // Save to session
                HttpContext.Session.SetString("Token", cleanToken);
                
                // Map JWT claims to session data
                var claims = (Dictionary<string, object>)tokenInfo.Claims;

                if (claims.ContainsKey("unique_name"))
                {
                    HttpContext.Session.SetString("Username", claims["unique_name"].ToString() ?? "");
                }

                if (claims.ContainsKey("nameid"))
                {
                    var userIdStr = claims["nameid"].ToString() ?? "0";
                    if (int.TryParse(userIdStr, out var userId))
                    {
                        HttpContext.Session.SetInt32("UserId", userId);
                    }
                }

                if (claims.ContainsKey("email"))
                {
                    var email = claims["email"].ToString() ?? "";
                    // Use email as FullName if no separate name claims
                    HttpContext.Session.SetString("FullName", email);
                }

                if (claims.ContainsKey("role"))
                {
                    var role = claims["role"].ToString() ?? "";
                    var isAdmin = role == "Admin";
                    HttpContext.Session.SetString("IsAdmin", isAdmin.ToString());
                }

                return Json(new 
                { 
                    success = true, 
                    message = "Token set successfully",
                    session_data = new
                    {
                        Username = HttpContext.Session.GetString("Username"),
                        FullName = HttpContext.Session.GetString("FullName"),
                        IsAdmin = HttpContext.Session.GetString("IsAdmin"),
                        UserId = HttpContext.Session.GetInt32("UserId")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // GET: TestAuth/TestApiCall
        public async Task<IActionResult> TestApiCall()
        {
            try
            {
                var httpClient = new HttpClient();
                var token = HttpContext.Session.GetString("Token");
                
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                httpClient.BaseAddress = new Uri("http://localhost:5274/api/");
                
                var response = await httpClient.GetAsync("books");
                var content = await response.Content.ReadAsStringAsync();
                
                return Json(new 
                { 
                    success = response.IsSuccessStatusCode,
                    status_code = (int)response.StatusCode,
                    response_content = content,
                    token_used = !string.IsNullOrEmpty(token)
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private dynamic ParseJwtToken(string token)
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid JWT token format");

            // Decode header
            var headerJson = DecodeBase64Url(parts[0]);
            var header = JsonSerializer.Deserialize<Dictionary<string, object>>(headerJson);

            // Decode payload
            var payloadJson = DecodeBase64Url(parts[1]);
            var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);

            return new
            {
                Header = header,
                Claims = payload,
                IsExpired = IsTokenExpired(payload),
                ExpiresAt = payload.ContainsKey("exp") 
                    ? DateTimeOffset.FromUnixTimeSeconds(((JsonElement)payload["exp"]).GetInt64()).DateTime
                    : (DateTime?)null
            };
        }

        private bool IsTokenExpired(Dictionary<string, object>? payload)
        {
            if (payload != null && payload.ContainsKey("exp"))
            {
                var exp = ((JsonElement)payload["exp"]).GetInt64();
                var expDateTime = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                return expDateTime <= DateTime.UtcNow;
            }
            return true;
        }

        private string DecodeBase64Url(string input)
        {
            // Add padding if needed
            switch (input.Length % 4)
            {
                case 2: input += "=="; break;
                case 3: input += "="; break;
            }

            var bytes = Convert.FromBase64String(input);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public class TokenRequest
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}
