# Tài Liệu Kỹ Thuật - Website BookStore

## Tổng Quan Dự Án

BookStore là một ứng dụng web bán sách trực tuyến được xây dựng bằng ASP.NET Core MVC với kiến trúc phân tầng rõ ràng. Dự án bao gồm:

- **BookStore.Web**: Frontend MVC application
- **BookStore.API**: Backend API services  
- **BookStore.Core**: Shared DTOs và business logic
- **BookStore.Data**: Data access layer

## Kiến Trúc Hệ Thống

### 1. Mô Hình MVC Pattern
```
Controllers → Services → API → Database
     ↓
   Views ← ViewModels
```

### 2. Dependency Injection Container
- ApiService: Giao tiếp với backend API
- EmailService: Gửi email OTP và thông báo
- PayOSService: Xử lý thanh toán trực tuyến
- OtpService: Quản lý mã OTP

## Phân Tích Code Khó & Trung Bình

### 1. BaseController - Quản Lý Authentication (Mức Độ: Trung Bình)

```csharp
public class BaseController : Controller
{
    protected bool IsUserLoggedIn()
    {
        return !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));
    }
    
    protected IActionResult? VerifyAdminRole()
    {
        if (!IsUserLoggedIn())
            return HandleUnauthorized();
        
        if (!IsCurrentUserAdmin())
            return HandleAccessDenied();
            
        return null; // User is admin, continue
    }
}
```

**Giải thích:**
- BaseController cung cấp các phương thức chung cho authentication
- Sử dụng Session để lưu trữ thông tin user (Token, UserId, IsAdmin)
- Pattern "Guard Clause" để kiểm tra quyền trước khi thực hiện action

### 2. ApiService - HTTP Client Wrapper (Mức Độ: Khó)

```csharp
public async Task<T?> GetAsync<T>(string endpoint)
{
    try
    {
        SetAuthorizationHeader();
        var response = await _httpClient.GetAsync(endpoint);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
        
        // Handle 401 Unauthorized
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            HandleUnauthorized();
            throw new UnauthorizedAccessException("Token đã hết hạn...");
        }
        
        var errorContent = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"API call failed: {errorContent}");
    }
    catch (UnauthorizedAccessException)
    {
        throw; // Re-throw để controller xử lý
    }
}
```

**Điểm Phức Tạp:**
- **Generic Methods**: Sử dụng `<T>` để deserialize response thành bất kỳ type nào
- **Exception Handling**: Phân biệt các loại exception và xử lý riêng biệt
- **Authorization Header**: Tự động thêm JWT token vào mỗi request
- **Async/Await Pattern**: Xử lý bất đồng bộ để không block UI thread

### 3. Shopping Cart Logic (Mức Độ: Khó)

```csharp
[HttpPost]
public IActionResult AddToCart(int bookId, int quantity = 1)
{
    var cart = GetCartFromSession();
    var existingItem = cart.FirstOrDefault(c => c.BookId == bookId);
    
    if (existingItem != null)
    {
        existingItem.Quantity += quantity;
    }
    else
    {
        cart.Add(new CartItemViewModel
        {
            BookId = bookId,
            Quantity = quantity
        });
    }
    
    SaveCartToSession(cart);
    
    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
    {
        return Json(new { 
            success = true, 
            cartCount = cart.Sum(c => c.Quantity),
            message = "Đã thêm sách vào giỏ hàng!" 
        });
    }
    
    return RedirectToAction("Cart");
}
```

**Phân Tích Kỹ Thuật:**
- **Session Management**: Lưu cart trong session thay vì database
- **AJAX Detection**: Kiểm tra header để trả về JSON hoặc redirect
- **Business Logic**: Xử lý merge quantity khi sản phẩm đã tồn tại
- **Response Differentiation**: Trả về format khác nhau cho AJAX vs normal request

### 4. Discount Calculation System (Mức Độ: Khó)

```csharp
public class CartViewModel
{
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    public decimal TotalOriginalAmount => Items.Sum(i => i.BookPrice * i.Quantity);
    public decimal TotalSavings => Items.Sum(i => i.TotalSavings);
    public decimal ShippingFee => (TotalAmount >= 500000 || VoucherFreeShipping) ? 0 : 30000;
    public decimal FinalAmount => Math.Max(0, TotalAmount - DiscountAmount + ShippingFee);
}

public class CartItemDetailViewModel
{
    public decimal TotalPrice => IsDiscountActive ? DiscountedPrice * Quantity : BookPrice * Quantity;
    public decimal TotalSavings => IsDiscountActive ? (BookPrice - DiscountedPrice) * Quantity : 0;
    public bool IsDiscountActive => DiscountPercentage > 0 && 
                                   (!SaleStartDate.HasValue || SaleStartDate <= DateTime.Now) &&
                                   (!SaleEndDate.HasValue || SaleEndDate >= DateTime.Now);
}
```

**Complexity Analysis:**
- **Computed Properties**: Sử dụng expression-bodied members cho calculated fields
- **Conditional Logic**: Phức tạp trong việc tính toán discount dựa trên thời gian
- **Business Rules**: Free shipping logic và voucher system
- **Math Operations**: Đảm bảo không có giá trị âm với Math.Max()

### 5. Checkout Process (Mức Độ: Khó)

```csharp
[HttpPost]
public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
{
    if (!ModelState.IsValid)
        return View("Checkout", model);
    
    var cart = GetCartFromSession();
    if (!cart.Any())
    {
        TempData["Warning"] = "Giỏ hàng của bạn đang trống.";
        return RedirectToAction("Cart");
    }
    
    // Create order items
    var orderItems = new List<CreateOrderItemDto>();
    foreach (var item in cart)
    {
        var book = await _apiService.GetAsync<BookDto>($"books/{item.BookId}");
        if (book != null)
        {
            orderItems.Add(new CreateOrderItemDto
            {
                BookId = item.BookId,
                Quantity = item.Quantity,
                Price = book.FinalPrice // Use discounted price
            });
        }
    }
    
    var createOrderDto = new CreateOrderDto
    {
        UserId = GetCurrentUserId(),
        Items = orderItems,
        ShippingAddress = model.ShippingAddress,
        PaymentMethod = model.PaymentMethod,
        TotalAmount = model.FinalAmount
    };
    
    var createdOrder = await _apiService.PostAsync<OrderDto>("orders", createOrderDto);
    
    if (createdOrder != null)
    {
        HttpContext.Session.Remove("Cart"); // Clear cart
        return RedirectToAction("OrderConfirmation", new { orderId = createdOrder.Id });
    }
}
```

**Phức Tạp Ở:**
- **Multi-step Process**: Validation → Cart check → API calls → Order creation
- **Data Transformation**: Convert cart items thành order items
- **Error Handling**: Multiple failure points cần xử lý
- **State Management**: Clear cart sau khi order thành công
- **Async Operations**: Multiple await calls có thể gây performance issues

### 6. Currency Formatting Helper (Mức Độ: Trung Bình)

```csharp
public static class CurrencyHelper
{
    public static string FormatVND(decimal amount)
    {
        return amount.ToString("N0", CultureInfo.InvariantCulture) + " VNĐ";
    }
    
    public static decimal ParseVND(string vndString)
    {
        if (string.IsNullOrEmpty(vndString))
            return 0;
            
        var cleanString = vndString.Replace("VNĐ", "")
                                  .Replace(" ", "")
                                  .Replace(",", "");
        
        return decimal.TryParse(cleanString, out decimal result) ? result : 0;
    }
}
```

**Kỹ Thuật:**
- **Globalization**: Sử dụng CultureInfo.InvariantCulture
- **String Manipulation**: Chain multiple Replace operations
- **Safe Parsing**: TryParse pattern để tránh exception

## Patterns & Best Practices Được Sử Dụng

### 1. Repository Pattern (Implicit)
- ApiService hoạt động như repository layer
- Abstraction giữa Controllers và data source

### 2. DTO Pattern
- Tách biệt API models và View models
- MappingHelper để convert giữa các layers

### 3. Session-based State Management
- Cart data lưu trong session
- User authentication info trong session

### 4. Error Handling Strategy
- Try-catch blocks với specific exception types
- TempData để hiển thị messages
- Logging cho debugging

### 5. Async/Await Pattern
- Tất cả API calls đều async
- Proper exception handling trong async methods

## Điểm Mạnh & Yếu Của Architecture

### Điểm Mạnh:
- **Separation of Concerns**: Rõ ràng giữa các layers
- **Reusability**: BaseController, Helpers có thể tái sử dụng
- **Maintainability**: Code structure dễ maintain
- **Scalability**: Có thể mở rộng thêm features

### Điểm Yếu:
- **Session Dependency**: Cart data mất khi session expire
- **API Coupling**: Frontend phụ thuộc nhiều vào API structure
- **Error Handling**: Chưa có centralized error handling
- **Performance**: Multiple API calls trong một action

## Các Tính Năng Nâng Cao

### 1. OTP Verification System
```csharp
public class OtpService
{
    public async Task<bool> SendOtpAsync(string email)
    {
        var otp = GenerateOtp();
        await _emailService.SendOtpEmailAsync(email, otp);

        // Store OTP in cache with expiration
        _memoryCache.Set($"otp_{email}", otp, TimeSpan.FromMinutes(5));
        return true;
    }

    public bool VerifyOtp(string email, string inputOtp)
    {
        var cachedOtp = _memoryCache.Get($"otp_{email}")?.ToString();
        return cachedOtp == inputOtp;
    }
}
```

**Phức Tạp:**
- **Memory Caching**: Sử dụng IMemoryCache với expiration time
- **Security**: OTP có thời hạn 5 phút để tăng bảo mật
- **Email Integration**: Tích hợp với SMTP service

### 2. PayOS Integration (Payment Gateway)
```csharp
public class PayOSService
{
    public async Task<PaymentLinkResponse> CreatePaymentLinkAsync(CreatePaymentRequest request)
    {
        var paymentData = new
        {
            orderCode = request.OrderCode,
            amount = request.Amount,
            description = request.Description,
            returnUrl = request.ReturnUrl,
            cancelUrl = request.CancelUrl
        };

        var signature = GenerateSignature(paymentData);
        var response = await _httpClient.PostAsync("payment-requests", paymentData);

        return await response.Content.ReadFromJsonAsync<PaymentLinkResponse>();
    }

    private string GenerateSignature(object data)
    {
        // Complex signature generation using HMAC-SHA256
        var jsonData = JsonConvert.SerializeObject(data);
        var keyBytes = Encoding.UTF8.GetBytes(_apiKey);
        var dataBytes = Encoding.UTF8.GetBytes(jsonData);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
```

**Kỹ Thuật Cao:**
- **Cryptographic Signatures**: HMAC-SHA256 để bảo mật
- **Third-party Integration**: Giao tiếp với external payment service
- **Webhook Handling**: Xử lý callback từ payment gateway

### 3. Advanced Search & Filtering
```csharp
public async Task<IActionResult> Index(string? search, int? categoryId,
                                     decimal? minPrice, decimal? maxPrice,
                                     int page = 1, int pageSize = 15)
{
    var books = await _apiService.GetAsync<List<BookDto>>("books");

    // Complex filtering logic
    if (!string.IsNullOrEmpty(search))
    {
        books = books.Where(b =>
            b.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            b.AuthorName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            b.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
        ).ToList();
    }

    if (categoryId.HasValue)
        books = books.Where(b => b.CategoryId == categoryId.Value).ToList();

    if (minPrice.HasValue)
        books = books.Where(b => b.FinalPrice >= minPrice.Value).ToList();

    if (maxPrice.HasValue)
        books = books.Where(b => b.FinalPrice <= maxPrice.Value).ToList();

    // Pagination logic
    var totalBooks = books.Count;
    var totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);
    var pagedBooks = books.Skip((page - 1) * pageSize).Take(pageSize).ToList();

    return View(new ShopViewModel
    {
        Books = pagedBooks.Select(MappingHelper.MapBookToViewModel).ToList(),
        CurrentPage = page,
        TotalPages = totalPages,
        // ... other properties
    });
}
```

**Complexity:**
- **Multi-criteria Filtering**: Kết hợp nhiều điều kiện search
- **String Comparison**: Case-insensitive search
- **Pagination Math**: Tính toán pages và skip/take
- **LINQ Chaining**: Multiple Where clauses

### 4. Image Management System
```csharp
public class ImageController : Controller
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (!IsValidImage(file))
            return BadRequest("Invalid image file");

        var fileName = GenerateUniqueFileName(file.FileName);
        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Json(new { success = true, fileName = fileName, url = $"/uploads/{fileName}" });
    }

    private bool IsValidImage(IFormFile file)
    {
        if (file == null || file.Length == 0) return false;
        if (file.Length > _maxFileSize) return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        return $"{Guid.NewGuid()}{extension}";
    }
}
```

**Security & Performance:**
- **File Validation**: Kiểm tra extension và file size
- **Unique Naming**: Sử dụng GUID để tránh conflict
- **Stream Processing**: Efficient file handling
- **Path Security**: Proper path combination

### 5. Admin Authorization Attribute
```csharp
public class AdminOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var isLoggedIn = !string.IsNullOrEmpty(session.GetString("Token"));
        var isAdmin = session.GetString("IsAdmin") == "True";

        if (!isLoggedIn)
        {
            context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
            return;
        }

        if (!isAdmin)
        {
            context.Result = new ViewResult { ViewName = "AccessDenied" };
            return;
        }

        base.OnActionExecuting(context);
    }
}
```

**Advanced Concepts:**
- **Action Filters**: Intercepting requests before action execution
- **Custom Attributes**: Creating reusable authorization logic
- **Context Manipulation**: Changing action result based on conditions

## Performance Optimization Techniques

### 1. Async/Await Best Practices
```csharp
// ❌ Bad: Blocking async calls
var books = _apiService.GetAsync<List<BookDto>>("books").Result;

// ✅ Good: Proper async/await
var books = await _apiService.GetAsync<List<BookDto>>("books");

// ✅ Better: Parallel execution
var slidersTask = _apiService.GetAsync<List<SliderDto>>("sliders/active");
var bannersTask = _apiService.GetAsync<List<BannerDto>>("banners/position/home");
var categoriesTask = _apiService.GetAsync<List<CategoryDto>>("categories");

await Task.WhenAll(slidersTask, bannersTask, categoriesTask);
```

### 2. Memory Management
```csharp
// Session-based cart instead of database calls
private List<CartItemViewModel> GetCartFromSession()
{
    var cartJson = HttpContext.Session.GetString("Cart");
    return string.IsNullOrEmpty(cartJson)
        ? new List<CartItemViewModel>()
        : JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);
}
```

### 3. Caching Strategy
```csharp
// OTP caching with expiration
_memoryCache.Set($"otp_{email}", otp, TimeSpan.FromMinutes(5));

// Category caching (could be implemented)
var categories = _memoryCache.GetOrCreate("categories", async entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
    return await _apiService.GetAsync<List<CategoryDto>>("categories");
});
```

## Security Considerations

### 1. Input Validation
- **Model Validation**: DataAnnotations trên ViewModels
- **Anti-forgery Tokens**: [ValidateAntiForgeryToken] attributes
- **File Upload Security**: Extension và size validation

### 2. Authentication & Authorization
- **JWT Tokens**: Stored in session, sent in API headers
- **Role-based Access**: Admin vs User separation
- **Session Management**: Proper cleanup on logout

### 3. API Security
- **HTTPS Only**: Secure communication với API
- **Token Expiration**: Automatic redirect khi token hết hạn
- **Error Information**: Không expose sensitive data trong error messages

## Troubleshooting Common Issues

### 1. Session Lost Issues
```csharp
// Problem: Session expires unexpectedly
// Solution: Check session timeout configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

### 2. API Connection Problems
```csharp
// Problem: API calls fail
// Solution: Proper error handling and logging
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "API call failed for endpoint: {Endpoint}", endpoint);
    throw new ApplicationException("Service temporarily unavailable");
}
```

### 3. Memory Leaks
```csharp
// Problem: HttpClient not disposed properly
// Solution: Use IHttpClientFactory
builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

## Kết Luận

BookStore project thể hiện một kiến trúc MVC chuẩn với nhiều patterns phức tạp. Code có độ khó trung bình đến cao, đặc biệt ở phần xử lý business logic như discount calculation, checkout process, payment integration, và security implementation.

**Key Takeaways:**
- **Async Programming**: Critical for performance
- **Security**: Multi-layered approach
- **Error Handling**: Comprehensive strategy needed
- **Performance**: Caching và parallel execution
- **Maintainability**: Clean architecture patterns

Việc hiểu rõ các concepts này là chìa khóa để maintain và extend project thành công.
