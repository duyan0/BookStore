using Microsoft.Extensions.Options;

namespace BookStore.Web.Services
{
    /// <summary>
    /// Mock Email Service for development and testing purposes.
    /// This service simulates email sending without actually sending emails.
    /// </summary>
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string toName, string otpCode)
        {
            try
            {
                _logger.LogInformation("=== MOCK EMAIL SERVICE ===");
                _logger.LogInformation("📧 OTP Email would be sent to: {Email}", toEmail);
                _logger.LogInformation("👤 Recipient Name: {Name}", toName);
                _logger.LogInformation("🔐 OTP Code: {OtpCode}", otpCode);
                _logger.LogInformation("📝 Subject: Xác thực tài khoản BookStore - Mã OTP");
                _logger.LogInformation("⏰ Valid for: 10 minutes");
                _logger.LogInformation("🔄 Max attempts: 5");
                _logger.LogInformation("========================");

                // Simulate email sending delay
                await Task.Delay(500);

                // Always return success for mock service
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mock email service for {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                _logger.LogInformation("=== MOCK EMAIL SERVICE ===");
                _logger.LogInformation("📧 Email would be sent to: {Email}", toEmail);
                _logger.LogInformation("📝 Subject: {Subject}", subject);
                _logger.LogInformation("📄 Body Length: {BodyLength} characters", body.Length);
                _logger.LogInformation("🎨 HTML Format: {IsHtml}", isHtml);
                _logger.LogInformation("========================");

                // Simulate email sending delay
                await Task.Delay(500);

                // Always return success for mock service
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mock email service for {Email}", toEmail);
                return false;
            }
        }
    }
}
