using Microsoft.AspNetCore.Mvc;
using BookStore.Core.DTOs;
using System.Text;
using System.Text.Json;

namespace BookStore.Web.Controllers
{
    public class DebugController : Controller
    {
        private readonly ILogger<DebugController> _logger;
        private readonly IConfiguration _configuration;

        public DebugController(ILogger<DebugController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        // GET: Debug/ApiTest
        public IActionResult ApiTest()
        {
            return View();
        }

        // POST: Debug/TestLogin
        [HttpPost]
        public async Task<IActionResult> TestLogin([FromBody] LoginRequest request)
        {
            try
            {
                var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5274/api";

                // Ensure base URL ends with trailing slash for proper concatenation
                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }

                _logger.LogInformation("Testing API connection to: {BaseUrl}", baseUrl);

                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(baseUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                // Test 1: Check if API is reachable
                var healthCheck = await TestApiHealth(httpClient);
                
                // Test 2: Test login endpoint
                var loginTest = await TestLoginEndpoint(httpClient, request.Username, request.Password);

                return Json(new
                {
                    success = true,
                    baseUrl,
                    healthCheck,
                    loginTest,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing API connection");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        private async Task<object> TestApiHealth(HttpClient httpClient)
        {
            try
            {
                // Test basic connectivity
                var response = await httpClient.GetAsync("");
                return new
                {
                    success = true,
                    statusCode = (int)response.StatusCode,
                    statusDescription = response.StatusCode.ToString(),
                    headers = response.Headers.Select(h => new { h.Key, Value = string.Join(", ", h.Value) }).ToList()
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    error = ex.Message
                };
            }
        }

        private async Task<object> TestLoginEndpoint(HttpClient httpClient, string username, string password)
        {
            try
            {
                var loginDto = new LoginUserDto
                {
                    Username = username,
                    Password = password
                };

                var json = JsonSerializer.Serialize(loginDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending POST request to: {Endpoint}", "auth/login");
                _logger.LogInformation("Request body: {Body}", json);

                var response = await httpClient.PostAsync("auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Response status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Response content: {Content}", responseContent);

                return new
                {
                    success = response.IsSuccessStatusCode,
                    statusCode = (int)response.StatusCode,
                    statusDescription = response.StatusCode.ToString(),
                    responseContent,
                    requestUrl = $"{httpClient.BaseAddress}auth/login",
                    requestBody = json,
                    headers = response.Headers.Select(h => new { h.Key, Value = string.Join(", ", h.Value) }).ToList()
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                };
            }
        }

        // GET: Debug/CheckApiRoutes
        public async Task<IActionResult> CheckApiRoutes()
        {
            try
            {
                var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5274/api";

                // Ensure base URL ends with trailing slash for proper concatenation
                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }

                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(baseUrl);

                var routes = new[]
                {
                    "",
                    "auth",
                    "auth/login",
                    "books",
                    "authors",
                    "categories"
                };

                var results = new List<object>();

                foreach (var route in routes)
                {
                    try
                    {
                        var response = await httpClient.GetAsync(route);
                        var content = await response.Content.ReadAsStringAsync();
                        
                        results.Add(new
                        {
                            route,
                            fullUrl = $"{httpClient.BaseAddress}{route}",
                            statusCode = (int)response.StatusCode,
                            statusDescription = response.StatusCode.ToString(),
                            contentLength = content.Length,
                            contentPreview = content.Length > 200 ? content.Substring(0, 200) + "..." : content
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new
                        {
                            route,
                            fullUrl = $"{httpClient.BaseAddress}{route}",
                            error = ex.Message
                        });
                    }
                }

                return Json(new
                {
                    success = true,
                    baseUrl,
                    results,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        public class LoginRequest
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
        }
    }
}
