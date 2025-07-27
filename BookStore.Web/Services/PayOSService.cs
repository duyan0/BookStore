using BookStore.Web.Models;
using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;

namespace BookStore.Web.Services
{
    public interface IPayOSService
    {
        Task<CreatePaymentResult> CreatePaymentAsync(int orderId, decimal amount, string description, List<ItemData> items);
        Task<PaymentLinkInformation> GetPaymentInfoAsync(int paymentId);
        Task<PaymentLinkInformation> CancelPaymentAsync(int paymentId, string reason);
        bool VerifyPaymentWebhookData(string webhookData);
    }

    public class PayOSService : IPayOSService
    {
        private readonly PayOS _payOS;
        private readonly PayOSConfiguration _config;
        private readonly ILogger<PayOSService> _logger;

        public PayOSService(IOptions<PayOSConfiguration> config, ILogger<PayOSService> logger)
        {
            _config = config.Value;
            _logger = logger;
            
            _payOS = new PayOS(_config.ClientId, _config.ApiKey, _config.ChecksumKey);
        }

        public async Task<CreatePaymentResult> CreatePaymentAsync(int orderId, decimal amount, string description, List<ItemData> items)
        {
            try
            {
                // Validate input parameters
                if (orderId <= 0)
                    throw new ArgumentException("Order ID must be greater than 0", nameof(orderId));

                if (amount <= 0)
                    throw new ArgumentException("Amount must be greater than 0", nameof(amount));

                if (string.IsNullOrEmpty(description))
                    throw new ArgumentException("Description cannot be null or empty", nameof(description));

                if (items == null || !items.Any())
                    throw new ArgumentException("Items cannot be null or empty", nameof(items));

                // Validate configuration
                if (string.IsNullOrEmpty(_config.ReturnUrl) || string.IsNullOrEmpty(_config.CancelUrl))
                    throw new InvalidOperationException("PayOS return URL and cancel URL must be configured");

                var paymentData = new PaymentData(
                    orderCode: orderId,
                    amount: (int)amount,
                    description: description,
                    items: items,
                    returnUrl: _config.ReturnUrl,
                    cancelUrl: _config.CancelUrl
                );

                _logger.LogInformation("Creating PayOS payment for order {OrderId} with amount {Amount} VND", orderId, amount);
                _logger.LogInformation("PayOS payment items: {ItemCount} items, Total amount: {Amount}", items.Count, amount);

                var result = await _payOS.createPaymentLink(paymentData);

                if (result == null)
                    throw new InvalidOperationException("PayOS returned null result");

                if (string.IsNullOrEmpty(result.checkoutUrl))
                    throw new InvalidOperationException("PayOS did not return a valid checkout URL");

                _logger.LogInformation("PayOS payment created successfully for order {OrderId}. Payment ID: {PaymentId}, Checkout URL: {CheckoutUrl}",
                    orderId, result.paymentLinkId, result.checkoutUrl);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment for order {OrderId}. Amount: {Amount}, Items: {ItemCount}",
                    orderId, amount, items?.Count ?? 0);
                throw new InvalidOperationException($"Failed to create PayOS payment: {ex.Message}", ex);
            }
        }

        public async Task<PaymentLinkInformation> GetPaymentInfoAsync(int paymentId)
        {
            try
            {
                _logger.LogInformation("Getting PayOS payment info for payment {PaymentId}", paymentId);

                var result = await _payOS.getPaymentLinkInformation(paymentId);

                _logger.LogInformation("PayOS payment info retrieved for payment {PaymentId}. Status: {Status}",
                    paymentId, result.status);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayOS payment info for payment {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<PaymentLinkInformation> CancelPaymentAsync(int paymentId, string reason)
        {
            try
            {
                _logger.LogInformation("Cancelling PayOS payment {PaymentId} with reason: {Reason}", paymentId, reason);

                var result = await _payOS.cancelPaymentLink(paymentId, reason);

                _logger.LogInformation("PayOS payment {PaymentId} cancelled successfully", paymentId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling PayOS payment {PaymentId}", paymentId);
                throw;
            }
        }

        public bool VerifyPaymentWebhookData(string webhookData)
        {
            try
            {
                // For now, we'll implement basic validation
                // In production, you should implement proper webhook signature verification
                _logger.LogInformation("PayOS webhook data received: {WebhookData}", webhookData);

                if (string.IsNullOrEmpty(webhookData))
                {
                    return false;
                }

                // Basic JSON validation
                var isValidJson = webhookData.Trim().StartsWith("{") && webhookData.Trim().EndsWith("}");

                _logger.LogInformation("PayOS webhook data validation result: {IsValid}", isValidJson);
                return isValidJson;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying PayOS webhook data");
                return false;
            }
        }
    }
}
