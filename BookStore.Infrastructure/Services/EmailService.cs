using BookStore.Core.Services;
using BookStore.Core.DTOs;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace BookStore.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string newPassword, string userName)
        {
            var subject = "BookStore - Mật khẩu mới của bạn";
            var body = GeneratePasswordResetEmailBody(userName, newPassword);

            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendPasswordChangeNotificationAsync(string toEmail, string userName)
        {
            var subject = "BookStore - Thông báo thay đổi mật khẩu";
            var body = GeneratePasswordChangeNotificationBody(userName);

            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(subject))
                {
                    Console.WriteLine("❌ Email validation failed: Missing required parameters");
                    return false;
                }

                // Support both appsettings.json and environment variables
                var smtpHost = Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST") ??
                              _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT") ??
                                        _configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ??
                                  _configuration["Email:Username"] ?? "crandi21112004@gmail.com";
                var smtpPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ??
                                  _configuration["Email:Password"] ?? "iljt phwd atnp syis";
                var fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM_EMAIL") ??
                               _configuration["Email:FromEmail"] ?? "crandi21112004@gmail.com";
                var fromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ??
                              _configuration["Email:FromName"] ?? "BookStore";

                // Validate Google App Password format (should be 16 characters without spaces)
                var isAppPassword = smtpPassword.Length == 16 && !smtpPassword.Contains(" ");
                var passwordSource = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") != null ? "Environment Variable" : "Configuration File";

                // Enhanced logging for debugging
                Console.WriteLine($"📧 === PREPARING EMAIL ===");
                Console.WriteLine($"📧 To: {toEmail}");
                Console.WriteLine($"📧 Subject: {subject}");
                Console.WriteLine($"📧 From: {fromName} <{fromEmail}>");
                Console.WriteLine($"📧 SMTP: {smtpHost}:{smtpPort}");
                Console.WriteLine($"📧 Username: {smtpUsername}");
                Console.WriteLine($"📧 Password Source: {passwordSource}");
                Console.WriteLine($"📧 Password: {new string('*', smtpPassword.Length)} (length: {smtpPassword.Length})");
                Console.WriteLine($"📧 App Password Format: {(isAppPassword ? "✅ Valid" : "⚠️ May not be App Password")}");
                Console.WriteLine($"📧 ========================");

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.Timeout = 30000; // 30 seconds timeout

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                Console.WriteLine($"📧 Connecting to SMTP server...");

                // Actually send the email
                await client.SendMailAsync(mailMessage);

                Console.WriteLine($"✅ Email sent successfully to {toEmail}");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"❌ SMTP Error: {smtpEx.Message}");
                Console.WriteLine($"❌ Status Code: {smtpEx.StatusCode}");
                Console.WriteLine($"❌ Inner Exception: {smtpEx.InnerException?.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ General Email Error: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        private string GeneratePasswordResetEmailBody(string userName, string newPassword)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>BookStore - Mật khẩu mới</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #000; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .password-box {{ background-color: #fff; border: 2px solid #000; padding: 20px; margin: 20px 0; text-align: center; }}
        .password {{ font-size: 24px; font-weight: bold; color: #000; letter-spacing: 2px; }}
        .footer {{ background-color: #333; color: white; padding: 15px; text-align: center; font-size: 12px; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; margin: 20px 0; border-radius: 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📚 BookStore</h1>
            <h2>Mật khẩu mới của bạn</h2>
        </div>
        
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            
            <p>Chúng tôi đã tạo mật khẩu mới cho tài khoản BookStore của bạn theo yêu cầu.</p>
            
            <div class='password-box'>
                <p>Mật khẩu mới của bạn là:</p>
                <div class='password'>{newPassword}</div>
            </div>
            
            <div class='warning'>
                <strong>⚠️ Lưu ý bảo mật:</strong>
                <ul>
                    <li>Vui lòng đăng nhập và đổi mật khẩu ngay sau khi nhận được email này</li>
                    <li>Không chia sẻ mật khẩu này với bất kỳ ai</li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng liên hệ với chúng tôi ngay</li>
                </ul>
            </div>
            
            <p>
                <a href='#' style='background-color: #000; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                    Đăng nhập ngay
                </a>
            </p>
            
            <p>Trân trọng,<br>Đội ngũ BookStore</p>
        </div>
        
        <div class='footer'>
            <p>&copy; 2025 BookStore. Tất cả quyền được bảo lưu.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePasswordChangeNotificationBody(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>BookStore - Thông báo thay đổi mật khẩu</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #000; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .success-box {{ background-color: #d4edda; border: 2px solid #28a745; padding: 20px; margin: 20px 0; text-align: center; border-radius: 8px; }}
        .footer {{ background-color: #333; color: white; padding: 15px; text-align: center; font-size: 12px; }}
        .security-tips {{ background-color: #e7f3ff; border: 1px solid #b3d9ff; padding: 15px; margin: 20px 0; border-radius: 5px; }}
        .icon {{ font-size: 48px; margin-bottom: 15px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📚 BookStore</h1>
            <h2>Thông báo thay đổi mật khẩu</h2>
        </div>

        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>

            <div class='success-box'>
                <div class='icon'>✅</div>
                <h3>Mật khẩu đã được thay đổi thành công!</h3>
                <p>Mật khẩu tài khoản BookStore của bạn đã được cập nhật vào lúc <strong>{DateTime.Now:dd/MM/yyyy HH:mm}</strong></p>
            </div>

            <div class='security-tips'>
                <h4>🔒 Lời khuyên bảo mật:</h4>
                <ul>
                    <li>Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ với chúng tôi ngay lập tức</li>
                    <li>Đảm bảo mật khẩu mới của bạn mạnh và không chia sẻ với ai</li>
                    <li>Thường xuyên thay đổi mật khẩu để bảo vệ tài khoản</li>
                    <li>Sử dụng mật khẩu khác nhau cho các tài khoản khác nhau</li>
                </ul>
            </div>

            <p>Cảm ơn bạn đã sử dụng BookStore!</p>
            <p>Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi qua email hoặc hotline hỗ trợ.</p>
        </div>

        <div class='footer'>
            <p>&copy; 2025 BookStore. Tất cả quyền được bảo lưu.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>📞 Hotline: 1900-1234 | 📧 Email: support@bookstore.com</p>
        </div>
    </div>
</body>
</html>";
        }

        #region Order Email Notifications

        public async Task<bool> SendOrderConfirmationEmailAsync(string toEmail, OrderDto order, string customerName)
        {
            var subject = $"BookStore - Xác nhận đơn hàng #{order.Id}";
            var body = GenerateOrderConfirmationEmailBody(order, customerName);

            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendOrderStatusUpdateEmailAsync(string toEmail, OrderDto order, string customerName, string oldStatus, string newStatus)
        {
            var subject = $"BookStore - Cập nhật trạng thái đơn hàng #{order.Id}";
            var body = GenerateOrderStatusUpdateEmailBody(order, customerName, oldStatus, newStatus);

            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendOrderCancellationEmailAsync(string toEmail, OrderDto order, string customerName, string cancellationReason)
        {
            var subject = $"BookStore - Đơn hàng #{order.Id} đã bị hủy";
            var body = GenerateOrderCancellationEmailBody(order, customerName, cancellationReason);

            return await SendEmailAsync(toEmail, subject, body);
        }

        #endregion

        #region Order Email Templates

        private string GenerateOrderConfirmationEmailBody(OrderDto order, string customerName)
        {
            var orderItemsHtml = new StringBuilder();
            foreach (var item in order.OrderDetails)
            {
                orderItemsHtml.AppendLine($@"
                    <tr>
                        <td style='padding: 10px; border-bottom: 1px solid #eee;'>
                            <strong>{item.BookTitle}</strong>
                        </td>
                        <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: center;'>
                            {item.Quantity}
                        </td>
                        <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>
                            {item.UnitPrice:N0} VND
                        </td>
                        <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>
                            <strong>{item.TotalPrice:N0} VND</strong>
                        </td>
                    </tr>");
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>BookStore - Xác nhận đơn hàng</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background-color: #000; color: white; padding: 30px 20px; text-align: center; }}
        .content {{ padding: 30px 20px; }}
        .order-info {{ background-color: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 8px; border-left: 4px solid #28a745; }}
        .order-table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        .order-table th {{ background-color: #f8f9fa; padding: 12px; text-align: left; border-bottom: 2px solid #dee2e6; }}
        .order-table td {{ padding: 10px; border-bottom: 1px solid #eee; }}
        .total-section {{ background-color: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 8px; }}
        .total-amount {{ font-size: 24px; font-weight: bold; color: #28a745; text-align: right; }}
        .footer {{ background-color: #333; color: white; padding: 20px; text-align: center; font-size: 12px; }}
        .btn {{ background-color: #000; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📚 BookStore</h1>
            <h2>Xác nhận đơn hàng</h2>
        </div>

        <div class='content'>
            <p>Xin chào <strong>{customerName}</strong>,</p>

            <p>Cảm ơn bạn đã đặt hàng tại BookStore! Chúng tôi đã nhận được đơn hàng của bạn và đang xử lý.</p>

            <div class='order-info'>
                <h3>📋 Thông tin đơn hàng</h3>
                <p><strong>Mã đơn hàng:</strong> #{order.Id}</p>
                <p><strong>Ngày đặt hàng:</strong> {order.OrderDate:dd/MM/yyyy HH:mm}</p>
                <p><strong>Trạng thái:</strong> <span style='color: #ffc107; font-weight: bold;'>Chờ xử lý</span></p>
                <p><strong>Địa chỉ giao hàng:</strong> {order.ShippingAddress}</p>
                <p><strong>Phương thức thanh toán:</strong> {order.PaymentMethod}</p>
            </div>

            <h3>📚 Chi tiết đơn hàng</h3>
            <table class='order-table'>
                <thead>
                    <tr>
                        <th>Sản phẩm</th>
                        <th style='text-align: center;'>Số lượng</th>
                        <th style='text-align: right;'>Đơn giá</th>
                        <th style='text-align: right;'>Thành tiền</th>
                    </tr>
                </thead>
                <tbody>
                    {orderItemsHtml}
                </tbody>
            </table>

            <div class='total-section'>
                <div class='total-amount'>
                    Tổng cộng: {order.TotalAmount:N0} VND
                </div>
            </div>

            <div style='text-align: center; margin: 30px 0;'>
                <a href='#' class='btn'>Xem chi tiết đơn hàng</a>
            </div>

            <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                <h4>📞 Cần hỗ trợ?</h4>
                <p>Nếu bạn có bất kỳ câu hỏi nào về đơn hàng, vui lòng liên hệ với chúng tôi:</p>
                <p>📧 Email: support@bookstore.com</p>
                <p>📱 Hotline: 1900-1234</p>
            </div>

            <p>Trân trọng,<br><strong>Đội ngũ BookStore</strong></p>
        </div>

        <div class='footer'>
            <p>&copy; 2024 BookStore. Tất cả quyền được bảo lưu.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời trực tiếp.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateOrderStatusUpdateEmailBody(OrderDto order, string customerName, string oldStatus, string newStatus)
        {
            var statusColor = newStatus switch
            {
                "Confirmed" => "#28a745",
                "Processing" => "#17a2b8",
                "Completed" => "#28a745",
                "Cancelled" => "#dc3545",
                _ => "#6c757d"
            };

            var statusText = newStatus switch
            {
                "Confirmed" => "Đã xác nhận",
                "Processing" => "Đang xử lý",
                "Completed" => "Hoàn thành",
                "Cancelled" => "Đã hủy",
                _ => newStatus
            };

            var oldStatusText = oldStatus switch
            {
                "Pending" => "Chờ xử lý",
                "Confirmed" => "Đã xác nhận",
                "Processing" => "Đang xử lý",
                "Completed" => "Hoàn thành",
                "Cancelled" => "Đã hủy",
                _ => oldStatus
            };

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>BookStore - Cập nhật đơn hàng</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background-color: #000; color: white; padding: 30px 20px; text-align: center; }}
        .content {{ padding: 30px 20px; }}
        .status-update {{ background-color: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 8px; border-left: 4px solid {statusColor}; }}
        .status-badge {{ background-color: {statusColor}; color: white; padding: 8px 16px; border-radius: 20px; font-weight: bold; display: inline-block; }}
        .order-info {{ background-color: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 8px; }}
        .footer {{ background-color: #333; color: white; padding: 20px; text-align: center; font-size: 12px; }}
        .btn {{ background-color: #000; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📚 BookStore</h1>
            <h2>Cập nhật trạng thái đơn hàng</h2>
        </div>

        <div class='content'>
            <p>Xin chào <strong>{customerName}</strong>,</p>

            <p>Đơn hàng #{order.Id} của bạn đã được cập nhật trạng thái.</p>

            <div class='status-update'>
                <h3>🔄 Thay đổi trạng thái</h3>
                <p><strong>Từ:</strong> {oldStatusText}</p>
                <p><strong>Thành:</strong> <span class='status-badge'>{statusText}</span></p>
            </div>

            <div class='order-info'>
                <h3>📋 Thông tin đơn hàng</h3>
                <p><strong>Mã đơn hàng:</strong> #{order.Id}</p>
                <p><strong>Ngày đặt hàng:</strong> {order.OrderDate:dd/MM/yyyy HH:mm}</p>
                <p><strong>Tổng tiền:</strong> {order.TotalAmount:N0} VND</p>
                <p><strong>Địa chỉ giao hàng:</strong> {order.ShippingAddress}</p>
            </div>

            <div style='text-align: center; margin: 30px 0;'>
                <a href='#' class='btn'>Xem chi tiết đơn hàng</a>
            </div>

            <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                <h4>📞 Cần hỗ trợ?</h4>
                <p>Nếu bạn có bất kỳ câu hỏi nào về đơn hàng, vui lòng liên hệ với chúng tôi:</p>
                <p>📧 Email: support@bookstore.com</p>
                <p>📱 Hotline: 1900-1234</p>
            </div>

            <p>Trân trọng,<br><strong>Đội ngũ BookStore</strong></p>
        </div>

        <div class='footer'>
            <p>&copy; 2024 BookStore. Tất cả quyền được bảo lưu.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời trực tiếp.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateOrderCancellationEmailBody(OrderDto order, string customerName, string cancellationReason)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>BookStore - Đơn hàng đã hủy</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background-color: #dc3545; color: white; padding: 30px 20px; text-align: center; }}
        .content {{ padding: 30px 20px; }}
        .cancellation-info {{ background-color: #f8d7da; padding: 20px; margin: 20px 0; border-radius: 8px; border-left: 4px solid #dc3545; }}
        .order-info {{ background-color: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 8px; }}
        .footer {{ background-color: #333; color: white; padding: 20px; text-align: center; font-size: 12px; }}
        .btn {{ background-color: #000; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📚 BookStore</h1>
            <h2>Đơn hàng đã bị hủy</h2>
        </div>

        <div class='content'>
            <p>Xin chào <strong>{customerName}</strong>,</p>

            <p>Chúng tôi rất tiếc phải thông báo rằng đơn hàng #{order.Id} của bạn đã bị hủy.</p>

            <div class='cancellation-info'>
                <h3>❌ Thông tin hủy đơn</h3>
                <p><strong>Lý do hủy:</strong> {cancellationReason}</p>
                <p><strong>Thời gian hủy:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
            </div>

            <div class='order-info'>
                <h3>📋 Thông tin đơn hàng đã hủy</h3>
                <p><strong>Mã đơn hàng:</strong> #{order.Id}</p>
                <p><strong>Ngày đặt hàng:</strong> {order.OrderDate:dd/MM/yyyy HH:mm}</p>
                <p><strong>Tổng tiền:</strong> {order.TotalAmount:N0} VND</p>
                <p><strong>Địa chỉ giao hàng:</strong> {order.ShippingAddress}</p>
            </div>

            <div style='background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                <h4>💰 Hoàn tiền</h4>
                <p>Nếu bạn đã thanh toán cho đơn hàng này, chúng tôi sẽ hoàn tiền trong vòng 3-5 ngày làm việc.</p>
                <p>Số tiền hoàn: <strong>{order.TotalAmount:N0} VND</strong></p>
            </div>

            <div style='text-align: center; margin: 30px 0;'>
                <a href='#' class='btn'>Tiếp tục mua sắm</a>
            </div>

            <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                <h4>📞 Cần hỗ trợ?</h4>
                <p>Nếu bạn có bất kỳ câu hỏi nào về việc hủy đơn hàng, vui lòng liên hệ với chúng tôi:</p>
                <p>📧 Email: support@bookstore.com</p>
                <p>📱 Hotline: 1900-1234</p>
            </div>

            <p>Chúng tôi xin lỗi vì sự bất tiện này và hy vọng được phục vụ bạn trong tương lai.</p>
            <p>Trân trọng,<br><strong>Đội ngũ BookStore</strong></p>
        </div>

        <div class='footer'>
            <p>&copy; 2024 BookStore. Tất cả quyền được bảo lưu.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời trực tiếp.</p>
        </div>
    </div>
</body>
</html>";
        }

        #endregion
    }
}
