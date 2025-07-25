using BookStore.Core.DTOs;

namespace BookStore.Core.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string newPassword, string userName);
        Task<bool> SendPasswordChangeNotificationAsync(string toEmail, string userName);
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);

        // Order Email Notifications
        Task<bool> SendOrderConfirmationEmailAsync(string toEmail, OrderDto order, string customerName);
        Task<bool> SendOrderStatusUpdateEmailAsync(string toEmail, OrderDto order, string customerName, string oldStatus, string newStatus);
        Task<bool> SendOrderCancellationEmailAsync(string toEmail, OrderDto order, string customerName, string cancellationReason);
    }
}
