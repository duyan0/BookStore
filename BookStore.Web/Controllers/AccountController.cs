using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Core.DTOs;
using Newtonsoft.Json;

namespace BookStore.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<AccountController> _logger;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public AccountController(ApiService apiService, ILogger<AccountController> logger,
            IOtpService otpService, IEmailService emailService)
        {
            _apiService = apiService;
            _logger = logger;
            _otpService = otpService;
            _emailService = emailService;
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

                        // Set AvatarUrl in session if available
                        if (!string.IsNullOrEmpty(response.User.AvatarUrl))
                        {
                            HttpContext.Session.SetString("AvatarUrl", response.User.AvatarUrl);
                        }
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

        // POST: Account/Register - Step 1: Generate and send OTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            _logger.LogInformation("=== REGISTER POST ACTION STARTED ===");
            _logger.LogInformation("Received registration request for email: {Email}", model?.Email ?? "NULL");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Returning view with errors.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("ModelState error: {Error}", error.ErrorMessage);
                }
                return View(model);
            }

            _logger.LogInformation("ModelState is valid. Processing registration for {Email}", model.Email);

            try
            {
                _logger.LogInformation("Step 1: Serializing user data for OTP storage");
                // Serialize user data for OTP storage
                var userData = System.Text.Json.JsonSerializer.Serialize(model);
                _logger.LogInformation("User data serialized successfully");

                _logger.LogInformation("Step 2: Generating OTP for email {Email}", model.Email);
                // Generate OTP
                var otpCode = await _otpService.GenerateOtpAsync(model.Email, userData);
                _logger.LogInformation("OTP generated successfully: {OtpCode}", otpCode);

                _logger.LogInformation("Step 3: Attempting to send OTP email to {Email}", model.Email);
                // Send OTP email
                var emailSent = await _emailService.SendOtpEmailAsync(model.Email, $"{model.FirstName} {model.LastName}", otpCode);

                _logger.LogInformation("Step 4: Email service result for {Email}: {EmailSent}", model.Email, emailSent);

                if (emailSent)
                {
                    _logger.LogInformation("✅ SUCCESS: OTP sent successfully to {Email}. Preparing redirect to VerifyOtp.", model.Email);
                    TempData["Success"] = "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra và nhập mã để hoàn tất đăng ký.";

                    _logger.LogInformation("Step 5: About to redirect to VerifyOtp with email: {Email}", model.Email);
                    var redirectResult = RedirectToAction("VerifyOtp", new { email = model.Email });
                    _logger.LogInformation("✅ REDIRECT CREATED: Returning RedirectToAction result");
                    return redirectResult;
                }
                else
                {
                    _logger.LogError("❌ FAILED: Email service returned false for {Email}", model.Email);
                    ModelState.AddModelError("", "Không thể gửi email xác thực. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ EXCEPTION: Error during registration process for email {Email}", model.Email);
                ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại.");
            }

            _logger.LogWarning("⚠️ FALLBACK: Returning view due to error or email send failure");
            return View(model);
        }

        // GET: Account/VerifyOtp
        public async Task<IActionResult> VerifyOtp(string email)
        {
            _logger.LogInformation("VerifyOtp GET action called with email: {Email}", email);

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("VerifyOtp called with empty email. Redirecting to Register.");
                TempData["Error"] = "Email không hợp lệ.";
                return RedirectToAction("Register");
            }

            // Check if OTP is still valid
            var isOtpValid = await _otpService.IsOtpValidAsync(email);
            _logger.LogInformation("OTP validity check for {Email}: {IsValid}", email, isOtpValid);

            if (!isOtpValid)
            {
                _logger.LogWarning("OTP not valid for {Email}. Redirecting to Register.", email);
                TempData["Error"] = "Mã OTP đã hết hạn hoặc không tồn tại. Vui lòng đăng ký lại.";
                return RedirectToAction("Register");
            }

            var remainingAttempts = await _otpService.GetRemainingAttemptsAsync(email);
            _logger.LogInformation("Creating VerifyOtp view model for {Email} with {RemainingAttempts} attempts", email, remainingAttempts);

            var model = new OtpVerificationViewModel
            {
                Email = email,
                RemainingAttempts = remainingAttempts,
                CanResend = true
            };

            _logger.LogInformation("Returning VerifyOtp view for {Email}", email);
            return View(model);
        }

        // POST: Account/VerifyOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(OtpVerificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RemainingAttempts = await _otpService.GetRemainingAttemptsAsync(model.Email);
                return View(model);
            }

            try
            {
                var isValid = await _otpService.ValidateOtpAsync(model.Email, model.OtpCode);

                if (isValid)
                {
                    // Get user data from OTP service
                    var userDataJson = await _otpService.GetUserDataAsync(model.Email);

                    if (string.IsNullOrEmpty(userDataJson))
                    {
                        TempData["Error"] = "Dữ liệu đăng ký không tồn tại. Vui lòng đăng ký lại.";
                        await _otpService.ClearOtpAsync(model.Email);
                        return RedirectToAction("Register");
                    }

                    // Deserialize user data
                    var registerModel = System.Text.Json.JsonSerializer.Deserialize<RegisterViewModel>(userDataJson);

                    if (registerModel == null)
                    {
                        TempData["Error"] = "Dữ liệu đăng ký không hợp lệ. Vui lòng đăng ký lại.";
                        await _otpService.ClearOtpAsync(model.Email);
                        return RedirectToAction("Register");
                    }

                    // Now create the actual user account
                    var success = await CreateUserAccountAsync(registerModel);

                    if (success)
                    {
                        // Clear OTP after successful registration
                        await _otpService.ClearOtpAsync(model.Email);

                        TempData["Success"] = "Đăng ký thành công! Chào mừng bạn đến với BookStore.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        TempData["Error"] = "Không thể tạo tài khoản. Vui lòng thử lại.";
                        model.RemainingAttempts = await _otpService.GetRemainingAttemptsAsync(model.Email);
                        return View(model);
                    }
                }
                else
                {
                    var remainingAttempts = await _otpService.GetRemainingAttemptsAsync(model.Email);

                    if (remainingAttempts <= 0)
                    {
                        TempData["Error"] = "Bạn đã nhập sai mã OTP quá nhiều lần. Vui lòng đăng ký lại.";
                        await _otpService.ClearOtpAsync(model.Email);
                        return RedirectToAction("Register");
                    }

                    ModelState.AddModelError("OtpCode", $"Mã OTP không đúng. Còn lại {remainingAttempts} lần thử.");
                    model.RemainingAttempts = remainingAttempts;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for email {Email}", model.Email);
                ModelState.AddModelError("", "Có lỗi xảy ra khi xác thực OTP. Vui lòng thử lại.");
                model.RemainingAttempts = await _otpService.GetRemainingAttemptsAsync(model.Email);
                return View(model);
            }
        }

        // POST: Account/ResendOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return Json(new { success = false, message = "Email không hợp lệ." });
                }

                var resent = await _otpService.ResendOtpAsync(request.Email);

                if (resent)
                {
                    var userData = await _otpService.GetUserDataAsync(request.Email);
                    if (!string.IsNullOrEmpty(userData))
                    {
                        var registerModel = System.Text.Json.JsonSerializer.Deserialize<RegisterViewModel>(userData);
                        if (registerModel != null)
                        {
                            var newOtpCode = await _otpService.GenerateOtpAsync(request.Email, userData);
                            var emailSent = await _emailService.SendOtpEmailAsync(request.Email,
                                $"{registerModel.FirstName} {registerModel.LastName}", newOtpCode);

                            if (emailSent)
                            {
                                return Json(new { success = true, message = "Mã OTP mới đã được gửi đến email của bạn." });
                            }
                        }
                    }
                }

                return Json(new { success = false, message = "Không thể gửi lại mã OTP. Vui lòng thử lại." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP for email {Email}", request.Email);
                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại." });
            }
        }

        // Helper method to create user account after OTP verification
        private async Task<bool> CreateUserAccountAsync(RegisterViewModel model)
        {
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
                    _logger.LogInformation("User account created successfully for email {Email}", model.Email);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to create user account for email {Email}. Response: {Message}",
                        model.Email, response?.Message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user account for email {Email}", model.Email);
                return false;
            }
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
                UserId = HttpContext.Session.GetInt32("UserId"),
                AvatarUrl = HttpContext.Session.GetString("AvatarUrl")
            };

            return View(userInfo);
        }

        // POST: Account/UploadAvatar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (avatarFile == null || avatarFile.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn một file ảnh.";
                return RedirectToAction("Profile");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["Error"] = "Chỉ chấp nhận file ảnh có định dạng: JPG, JPEG, PNG, GIF, WEBP.";
                return RedirectToAction("Profile");
            }

            // Validate file size (max 5MB)
            if (avatarFile.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "Kích thước file không được vượt quá 5MB.";
                return RedirectToAction("Profile");
            }

            try
            {
                // Create avatars directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var userId = HttpContext.Session.GetInt32("UserId");
                var fileName = $"avatar_{userId}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Delete old avatar if exists
                var oldAvatarUrl = HttpContext.Session.GetString("AvatarUrl");
                if (!string.IsNullOrEmpty(oldAvatarUrl))
                {
                    var oldFileName = Path.GetFileName(oldAvatarUrl);
                    var oldFilePath = Path.Combine(uploadsPath, oldFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Save new avatar
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                // Update avatar URL in session
                var avatarUrl = $"/uploads/avatars/{fileName}";
                HttpContext.Session.SetString("AvatarUrl", avatarUrl);

                // Update avatar URL in database via API
                try
                {
                    var updateData = new { AvatarUrl = avatarUrl };
                    await _apiService.PutAsync<object>($"auth/users/{userId}/avatar", updateData);
                }
                catch (Exception)
                {
                    // If API call fails, we still have the file uploaded locally
                    // The avatar will work for this session
                }

                TempData["Success"] = "Cập nhật ảnh đại diện thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra khi tải lên ảnh: {ex.Message}";
            }

            return RedirectToAction("Profile");
        }

        // GET: Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string emailOrUsername)
        {
            if (string.IsNullOrEmpty(emailOrUsername))
            {
                TempData["Error"] = "Vui lòng nhập email hoặc tên đăng nhập.";
                return View();
            }

            try
            {
                var requestData = new
                {
                    Email = emailOrUsername.Contains("@") ? emailOrUsername : null,
                    Username = !emailOrUsername.Contains("@") ? emailOrUsername : null
                };

                var response = await _apiService.PostAsync<object>("auth/reset-password", requestData);

                if (response != null)
                {
                    TempData["Success"] = "Mật khẩu mới đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.";
                    return RedirectToAction("Login");
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy tài khoản với thông tin đã cung cấp.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi gửi yêu cầu. Vui lòng thử lại sau.";
            }

            return View();
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

        // GET: Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            return View(new ChangePasswordViewModel());
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var changePasswordDto = new ChangePasswordDto
                {
                    CurrentPassword = model.CurrentPassword,
                    NewPassword = model.NewPassword,
                    ConfirmNewPassword = model.ConfirmNewPassword
                };

                var response = await _apiService.PostAsync<object>("auth/change-password", changePasswordDto);

                if (response != null)
                {
                    TempData["Success"] = "Đổi mật khẩu thành công! Email thông báo đã được gửi đến địa chỉ email của bạn.";
                    return RedirectToAction("Profile");
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không đúng hoặc có lỗi xảy ra.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                return View(model);
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
