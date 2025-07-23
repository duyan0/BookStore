using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Core.DTOs;

namespace BookStore.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApiService apiService, ILogger<AccountController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Account/Login
        public IActionResult Login(string? returnUrl = null)
        {
            // If user is already logged in, redirect to home
            if (IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var loginDto = new LoginUserDto
                {
                    Username = model.Username,
                    Password = model.Password
                };

                var response = await _apiService.PostAsync<AuthResponseDto>("auth/login", loginDto);

                if (response != null && response.Success)
                {
                    // Save token and user info to session
                    HttpContext.Session.SetString("Token", response.Token);

                    // Try to extract info from JWT token if User object is null
                    if (response.User != null)
                    {
                        HttpContext.Session.SetString("Username", response.User.Username);
                        HttpContext.Session.SetString("FullName", response.User.FullName);
                        HttpContext.Session.SetString("IsAdmin", response.User.IsAdmin.ToString());
                        HttpContext.Session.SetInt32("UserId", response.User.Id);
                    }
                    else
                    {
                        // Fallback: extract from JWT token
                        var tokenInfo = ExtractInfoFromJwtToken(response.Token);
                        HttpContext.Session.SetString("Username", tokenInfo.Username);
                        HttpContext.Session.SetString("FullName", tokenInfo.FullName);
                        HttpContext.Session.SetString("IsAdmin", tokenInfo.IsAdmin.ToString());
                        HttpContext.Session.SetInt32("UserId", tokenInfo.UserId);
                    }

                    TempData["Success"] = "Đăng nhập thành công!";

                    // Check if there's a return URL
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // Redirect based on user role
                    var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
                    if (isAdmin)
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" }); // Admin dashboard
                    }
                    else
                    {
                        return RedirectToAction("Dashboard", "User"); // User dashboard
                    }
                }
                else
                {
                    ModelState.AddModelError("", response?.Message ?? "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", model.Username);
                ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình đăng nhập. Vui lòng thử lại sau.");
            }

            return View(model);
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            // If user is already logged in, redirect to home
            if (IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var registerDto = new RegisterUserDto
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone ?? string.Empty,
                    Address = model.Address ?? string.Empty,
                    IsAdmin = false // Default to regular user
                };

                var response = await _apiService.PostAsync<AuthResponseDto>("auth/register", registerDto);

                if (response != null && response.Success)
                {
                    // Save token and user info to session
                    HttpContext.Session.SetString("Token", response.Token);
                    HttpContext.Session.SetString("Username", response.User?.Username ?? "");
                    HttpContext.Session.SetString("FullName", response.User?.FullName ?? "");
                    HttpContext.Session.SetString("IsAdmin", response.User?.IsAdmin.ToString() ?? "false");
                    HttpContext.Session.SetInt32("UserId", response.User?.Id ?? 0);

                    TempData["Success"] = "Đăng ký thành công! Chào mừng bạn đến với BookStore.";

                    // Redirect based on user role (new users are typically not admin)
                    var isAdmin = response.User?.IsAdmin == true;
                    if (isAdmin)
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" }); // Admin dashboard
                    }
                    else
                    {
                        return RedirectToAction("Dashboard", "User"); // User dashboard
                    }
                }
                else
                {
                    ModelState.AddModelError("", response?.Message ?? "Đăng ký thất bại. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Username}", model.Username);
                ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại sau.");
            }

            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            // Clear session
            HttpContext.Session.Clear();

            TempData["Info"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogoutPost()
        {
            // Clear session
            HttpContext.Session.Clear();

            TempData["Info"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        public IActionResult Profile()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var userInfo = new
            {
                Username = HttpContext.Session.GetString("Username"),
                FullName = HttpContext.Session.GetString("FullName"),
                IsAdmin = HttpContext.Session.GetString("IsAdmin") == "true",
                UserId = HttpContext.Session.GetInt32("UserId")
            };

            return View(userInfo);
        }

        // Helper method to check if user is logged in
        private bool IsUserLoggedIn()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                // Simple check for JWT token format and expiration
                var parts = token.Split('.');
                if (parts.Length != 3) return false;

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
                    return expDateTime > DateTime.UtcNow;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Helper method to extract user info from JWT token
        private (string Username, string FullName, bool IsAdmin, int UserId) ExtractInfoFromJwtToken(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3)
                    throw new ArgumentException("Invalid JWT token format");

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

                if (json == null)
                    throw new ArgumentException("Invalid JWT payload");

                // Extract claims with fallbacks
                var username = GetClaimValue(json, "unique_name") ?? GetClaimValue(json, "name") ?? "";
                var email = GetClaimValue(json, "email") ?? "";
                var fullName = email; // Use email as fullname fallback
                var isAdmin = GetClaimValue(json, "role") == "Admin";
                var userIdStr = GetClaimValue(json, "nameid") ?? GetClaimValue(json, "sub") ?? "0";
                var userId = int.TryParse(userIdStr, out var id) ? id : 0;

                return (username, fullName, isAdmin, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting info from JWT token");
                return ("", "", false, 0);
            }
        }

        private string? GetClaimValue(Dictionary<string, object> claims, string claimType)
        {
            if (claims.ContainsKey(claimType))
            {
                var value = claims[claimType];
                if (value is System.Text.Json.JsonElement element)
                {
                    return element.GetString();
                }
                return value?.ToString();
            }
            return null;
        }
    }
}
