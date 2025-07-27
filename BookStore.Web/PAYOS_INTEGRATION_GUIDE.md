# PayOS Payment Gateway Integration Guide

## Overview
This guide documents the complete PayOS payment gateway integration in the BookStore application.

## Features Implemented

### ✅ **1. PayOS Payment Method**
- Added PayOS as a payment option in checkout form
- Integrated with existing payment method selection
- Dynamic UI showing PayOS information when selected

### ✅ **2. Secure Configuration**
- PayOS credentials stored in appsettings.json
- Environment-based configuration support
- Secure API key management

### ✅ **3. Payment Processing**
- Create PayOS payment links for orders
- Handle payment success/failure callbacks
- Integrate with existing order management system

### ✅ **4. User Experience**
- Seamless payment flow integration
- Clear payment status feedback
- Proper error handling and user notifications

## Configuration

### **appsettings.json**
```json
{
  "PayOS": {
    "ClientId": "df5317bd-9fbf-4ce3-8fc6-e606d0a7322f",
    "ApiKey": "793bd99b-a2ae-4632-8a1b-718e12ec0181",
    "ChecksumKey": "3ba76aeb28118ed73211376aba7ad76f46f9114f9c139947943ae1637204fffd",
    "ReturnUrl": "http://localhost:5106/Shop/PaymentReturn",
    "CancelUrl": "http://localhost:5106/Shop/PaymentCancel"
  }
}
```

### **Service Registration (Program.cs)**
```csharp
// PayOS Configuration
builder.Services.Configure<PayOSConfiguration>(builder.Configuration.GetSection("PayOS"));
builder.Services.AddScoped<IPayOSService, PayOSService>();
```

## Architecture

### **1. PayOS Service Layer**
```
BookStore.Web/
├── Models/
│   └── PayOSConfiguration.cs
├── Services/
│   └── PayOSService.cs
└── Controllers/
    └── ShopController.cs (PayOS integration)
```

### **2. Payment Flow**
```
1. User selects PayOS payment method
2. User submits checkout form
3. Order is created in database
4. PayOS payment link is generated
5. User is redirected to PayOS payment page
6. User completes payment on PayOS
7. PayOS redirects back to BookStore
8. Payment status is processed
9. Order status is updated
10. User sees confirmation page
```

## API Integration

### **PayOS Service Methods**
```csharp
public interface IPayOSService
{
    Task<CreatePaymentResult> CreatePaymentAsync(int orderId, decimal amount, string description, List<ItemData> items);
    Task<PaymentData> GetPaymentInfoAsync(int paymentId);
    Task<PaymentData> CancelPaymentAsync(int paymentId, string reason);
    string VerifyPaymentWebhookData(string webhookData);
}
```

### **Payment Creation**
```csharp
var paymentItems = model.Items.Select(item => new ItemData(
    name: item.BookTitle,
    quantity: item.Quantity,
    price: (int)item.EffectivePrice
)).ToList();

var paymentResult = await _payOSService.CreatePaymentAsync(
    orderId: createdOrder.Id,
    amount: model.FinalAmount,
    description: $"Thanh toán đơn hàng #{createdOrder.Id} - BookStore",
    items: paymentItems
);
```

## Controller Actions

### **1. Checkout Processing**
```csharp
[HttpPost]
public async Task<IActionResult> Checkout(CheckoutViewModel model)
{
    // Create order first
    var createdOrder = await _apiService.PostAsync<OrderDto>("orders", createOrderDto);
    
    if (model.PaymentMethod == "PayOS")
    {
        // Create PayOS payment and redirect
        var paymentResult = await _payOSService.CreatePaymentAsync(...);
        return Redirect(paymentResult.checkoutUrl);
    }
    else
    {
        // Handle traditional payment methods
        return RedirectToAction("OrderConfirmation", new { orderId = createdOrder.Id });
    }
}
```

### **2. Payment Return Handling**
```csharp
public async Task<IActionResult> PaymentReturn(int orderCode, string status)
{
    if (status == "PAID" || status == "SUCCESS")
    {
        // Payment successful
        HttpContext.Session.Remove("Cart");
        return RedirectToAction("OrderConfirmation", new { orderId = orderCode });
    }
    else
    {
        // Payment failed
        return RedirectToAction("Checkout");
    }
}
```

### **3. Payment Cancellation**
```csharp
public IActionResult PaymentCancel(int orderCode)
{
    HttpContext.Session.Remove("PendingOrderId");
    TempData["Warning"] = "Thanh toán đã bị hủy.";
    return RedirectToAction("Checkout");
}
```

## Frontend Integration

### **1. Payment Method Selection**
```html
<select asp-for="PaymentMethod" class="form-select" id="paymentMethodSelect">
    <option value="COD">Thanh toán khi nhận hàng (COD)</option>
    <option value="PayOS">Thanh toán online qua PayOS</option>
    <!-- Other payment methods -->
</select>
```

### **2. PayOS Information Display**
```html
<div id="payosInfo" class="mt-2 p-3 bg-light rounded d-none">
    <div class="d-flex align-items-center mb-2">
        <i class="fas fa-credit-card text-primary me-2"></i>
        <strong class="text-primary">Thanh toán online qua PayOS</strong>
    </div>
    <ul class="mb-0 small text-muted">
        <li>Hỗ trợ thanh toán qua ATM, Internet Banking, Ví điện tử</li>
        <li>Bảo mật cao với công nghệ mã hóa SSL</li>
        <li>Giao dịch được xử lý ngay lập tức</li>
    </ul>
</div>
```

### **3. JavaScript Integration**
```javascript
function initializePaymentMethodHandling() {
    const paymentMethodSelect = document.getElementById('paymentMethodSelect');
    const payosInfo = document.getElementById('payosInfo');
    
    paymentMethodSelect.addEventListener('change', function() {
        if (this.value === 'PayOS') {
            payosInfo.classList.remove('d-none');
        } else {
            payosInfo.classList.add('d-none');
        }
    });
}
```

## Security Considerations

### **1. API Key Management**
- Store credentials in appsettings.json (development)
- Use environment variables in production
- Never expose API keys in client-side code

### **2. Payment Verification**
- Verify payment status on return
- Use checksum validation for webhooks
- Implement proper session management

### **3. Error Handling**
- Graceful handling of payment failures
- Proper logging of payment transactions
- User-friendly error messages

## Testing

### **1. Test Payment Flow**
1. Add items to cart
2. Go to checkout page
3. Select PayOS payment method
4. Fill in shipping address
5. Submit checkout form
6. Verify redirect to PayOS payment page
7. Complete payment on PayOS
8. Verify return to order confirmation

### **2. Test Error Scenarios**
- Payment cancellation
- Payment failure
- Network errors
- Invalid payment data

## URLs and Endpoints

### **PayOS Callback URLs**
- **Return URL**: `http://localhost:5106/Shop/PaymentReturn`
- **Cancel URL**: `http://localhost:5106/Shop/PaymentCancel`

### **Application URLs**
- **Checkout**: `http://localhost:5106/Shop/Checkout`
- **Order Confirmation**: `http://localhost:5106/Shop/OrderConfirmation/{orderId}`

## Supported Payment Methods

### **PayOS Supports:**
- ATM Cards (Domestic)
- Internet Banking
- QR Code Payment
- E-Wallets (MoMo, ZaloPay, etc.)
- Credit/Debit Cards

## Logging and Monitoring

### **Payment Transaction Logs**
```csharp
_logger.LogInformation("Creating PayOS payment for order {OrderId} with amount {Amount}", orderId, amount);
_logger.LogInformation("PayOS payment created successfully. Payment ID: {PaymentId}", result.paymentLinkId);
_logger.LogWarning("PayOS payment failed for order {OrderCode} with status {Status}", orderCode, status);
```

## Production Deployment

### **1. Environment Configuration**
- Update PayOS URLs to production endpoints
- Configure proper return/cancel URLs
- Set up environment variables for credentials

### **2. SSL Requirements**
- Ensure HTTPS for all payment-related pages
- Configure proper SSL certificates
- Update PayOS callback URLs to HTTPS

### **3. Monitoring**
- Set up payment transaction monitoring
- Configure alerts for payment failures
- Monitor PayOS API response times

## Troubleshooting

### **Common Issues**
1. **Payment redirect fails**: Check PayOS credentials and URLs
2. **Return callback not working**: Verify return URL configuration
3. **Payment status not updating**: Check session management and logging

### **Debug Steps**
1. Check application logs for PayOS API calls
2. Verify PayOS dashboard for transaction status
3. Test with PayOS sandbox environment first
4. Validate callback URL accessibility
