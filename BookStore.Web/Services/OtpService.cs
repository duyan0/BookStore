using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace BookStore.Web.Services
{
    public class OtpData
    {
        public string Code { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserData { get; set; } = string.Empty; // JSON string of user registration data
        public int AttemptCount { get; set; } = 0;
        public const int MaxAttempts = 5;
    }

    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string email, string userData);
        Task<bool> ValidateOtpAsync(string email, string otpCode);
        Task<string?> GetUserDataAsync(string email);
        Task<bool> ResendOtpAsync(string email);
        Task<bool> IsOtpValidAsync(string email);
        Task ClearOtpAsync(string email);
        Task<int> GetRemainingAttemptsAsync(string email);
    }

    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;
        private readonly ILogger<OtpService> _logger;
        private const int OtpExpiryMinutes = 10;
        private const int OtpLength = 6;

        public OtpService(IMemoryCache cache, IEmailService emailService, ILogger<OtpService> logger)
        {
            _cache = cache;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<string> GenerateOtpAsync(string email, string userData)
        {
            try
            {
                var otpCode = GenerateRandomOtp();
                var expiryTime = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);

                var otpData = new OtpData
                {
                    Code = otpCode,
                    ExpiryTime = expiryTime,
                    Email = email,
                    UserData = userData,
                    AttemptCount = 0
                };

                var cacheKey = GetCacheKey(email);
                _cache.Set(cacheKey, otpData, TimeSpan.FromMinutes(OtpExpiryMinutes + 1));

                _logger.LogInformation("OTP generated for email {Email}, expires at {ExpiryTime}", email, expiryTime);

                return otpCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP for email {Email}", email);
                throw;
            }
        }

        public async Task<bool> ValidateOtpAsync(string email, string otpCode)
        {
            try
            {
                var cacheKey = GetCacheKey(email);
                
                if (!_cache.TryGetValue(cacheKey, out OtpData? otpData) || otpData == null)
                {
                    _logger.LogWarning("OTP not found for email {Email}", email);
                    return false;
                }

                // Check if OTP has expired
                if (DateTime.UtcNow > otpData.ExpiryTime)
                {
                    _logger.LogWarning("OTP expired for email {Email}", email);
                    _cache.Remove(cacheKey);
                    return false;
                }

                // Check attempt count
                if (otpData.AttemptCount >= OtpData.MaxAttempts)
                {
                    _logger.LogWarning("Maximum OTP attempts exceeded for email {Email}", email);
                    _cache.Remove(cacheKey);
                    return false;
                }

                // Increment attempt count
                otpData.AttemptCount++;
                _cache.Set(cacheKey, otpData, TimeSpan.FromMinutes(OtpExpiryMinutes + 1));

                // Validate OTP code
                if (otpData.Code == otpCode)
                {
                    _logger.LogInformation("OTP validated successfully for email {Email}", email);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Invalid OTP provided for email {Email}. Attempt {Attempt}/{MaxAttempts}", 
                        email, otpData.AttemptCount, OtpData.MaxAttempts);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OTP for email {Email}", email);
                return false;
            }
        }

        public async Task<string?> GetUserDataAsync(string email)
        {
            try
            {
                var cacheKey = GetCacheKey(email);
                
                if (_cache.TryGetValue(cacheKey, out OtpData? otpData) && otpData != null)
                {
                    return otpData.UserData;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user data for email {Email}", email);
                return null;
            }
        }

        public async Task<bool> ResendOtpAsync(string email)
        {
            try
            {
                var cacheKey = GetCacheKey(email);
                
                if (!_cache.TryGetValue(cacheKey, out OtpData? existingOtpData) || existingOtpData == null)
                {
                    _logger.LogWarning("No existing OTP data found for resend request for email {Email}", email);
                    return false;
                }

                // Generate new OTP but keep the same user data
                var newOtpCode = GenerateRandomOtp();
                var newExpiryTime = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);

                existingOtpData.Code = newOtpCode;
                existingOtpData.ExpiryTime = newExpiryTime;
                existingOtpData.AttemptCount = 0; // Reset attempt count

                _cache.Set(cacheKey, existingOtpData, TimeSpan.FromMinutes(OtpExpiryMinutes + 1));

                _logger.LogInformation("OTP resent for email {Email}", email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP for email {Email}", email);
                return false;
            }
        }

        public async Task<bool> IsOtpValidAsync(string email)
        {
            try
            {
                var cacheKey = GetCacheKey(email);
                
                if (_cache.TryGetValue(cacheKey, out OtpData? otpData) && otpData != null)
                {
                    return DateTime.UtcNow <= otpData.ExpiryTime && otpData.AttemptCount < OtpData.MaxAttempts;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking OTP validity for email {Email}", email);
                return false;
            }
        }

        public async Task ClearOtpAsync(string email)
        {
            try
            {
                var cacheKey = GetCacheKey(email);
                _cache.Remove(cacheKey);
                _logger.LogInformation("OTP cleared for email {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing OTP for email {Email}", email);
            }
        }

        public async Task<int> GetRemainingAttemptsAsync(string email)
        {
            try
            {
                var cacheKey = GetCacheKey(email);
                
                if (_cache.TryGetValue(cacheKey, out OtpData? otpData) && otpData != null)
                {
                    return Math.Max(0, OtpData.MaxAttempts - otpData.AttemptCount);
                }

                return OtpData.MaxAttempts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting remaining attempts for email {Email}", email);
                return 0;
            }
        }

        private string GenerateRandomOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = Math.Abs(BitConverter.ToInt32(bytes, 0));
            return (randomNumber % (int)Math.Pow(10, OtpLength)).ToString($"D{OtpLength}");
        }

        private string GetCacheKey(string email)
        {
            return $"otp_{email.ToLowerInvariant()}";
        }
    }
}
