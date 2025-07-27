using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace BookStore.Web.Services
{
    public class EmailConfiguration
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }

    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string toName, string otpCode);
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailConfiguration> emailConfig, ILogger<EmailService> logger)
        {
            _emailConfig = emailConfig.Value;
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string toName, string otpCode)
        {
            try
            {
                var subject = "Xác thực tài khoản BookStore - Mã OTP";
                var body = GenerateOtpEmailBody(toName, otpCode);

                return await SendEmailAsync(toEmail, subject, body, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                // Validate email configuration
                if (string.IsNullOrEmpty(_emailConfig.SmtpServer) ||
                    string.IsNullOrEmpty(_emailConfig.SmtpUsername) ||
                    string.IsNullOrEmpty(_emailConfig.SmtpPassword))
                {
                    _logger.LogError("Email configuration is incomplete. Please check SMTP settings.");
                    return false;
                }

                using var client = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.SmtpPort);

                // Configure SMTP client
                client.EnableSsl = _emailConfig.EnableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Timeout = 30000; // 30 seconds timeout

                using var message = new MailMessage();
                message.From = new MailAddress(_emailConfig.FromEmail, _emailConfig.FromName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;
                message.Priority = MailPriority.High;

                _logger.LogInformation("Attempting to send email to {Email} via SMTP {SmtpServer}:{SmtpPort}",
                    toEmail, _emailConfig.SmtpServer, _emailConfig.SmtpPort);

                await client.SendMailAsync(message);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending email to {Email}. StatusCode: {StatusCode}, Message: {Message}",
                    toEmail, smtpEx.StatusCode, smtpEx.Message);

                // Log specific SMTP error guidance
                if (smtpEx.Message.Contains("Authentication Required") || smtpEx.Message.Contains("5.7.0"))
                {
                    _logger.LogError("SMTP Authentication failed. Please check:");
                    _logger.LogError("1. Gmail: Enable 2FA and use App Password instead of regular password");
                    _logger.LogError("2. Outlook: Enable 'Less secure app access' or use App Password");
                    _logger.LogError("3. Verify SMTP username and password are correct");
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error sending email to {Email}", toEmail);
                return false;
            }
        }

        private string GenerateOtpEmailBody(string userName, string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Xác thực tài khoản BookStore</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f8f9fa; }}
        .otp-code {{ font-size: 32px; font-weight: bold; color: #007bff; text-align: center; 
                     background-color: white; padding: 20px; border: 2px dashed #007bff; 
                     margin: 20px 0; letter-spacing: 5px; }}
        .footer {{ padding: 20px; text-align: center; color: #666; font-size: 12px; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; 
                   border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📚 BookStore</h1>
            <p>Xác thực tài khoản của bạn</p>
        </div>
        
        <div class='content'>
            <h2>Xin chào {userName}!</h2>
            
            <p>Cảm ơn bạn đã đăng ký tài khoản tại BookStore. Để hoàn tất quá trình đăng ký, 
               vui lòng sử dụng mã OTP dưới đây:</p>
            
            <div class='otp-code'>{otpCode}</div>
            
            <div class='warning'>
                <strong>⚠️ Lưu ý quan trọng:</strong>
                <ul>
                    <li>Mã OTP này có hiệu lực trong <strong>10 phút</strong></li>
                    <li>Không chia sẻ mã này với bất kỳ ai</li>
                    <li>Nếu bạn không yêu cầu đăng ký, vui lòng bỏ qua email này</li>
                </ul>
            </div>
            
            <p>Nếu mã OTP hết hạn, bạn có thể yêu cầu gửi lại mã mới từ trang đăng ký.</p>
            
            <p>Trân trọng,<br>
               <strong>Đội ngũ BookStore</strong></p>
        </div>
        
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>&copy; 2024 BookStore. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
