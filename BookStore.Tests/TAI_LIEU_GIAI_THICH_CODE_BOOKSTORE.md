# Tài Liệu Giải Thích Code Website BookStore

## 📋 Tổng Quan Dự Án

Website BookStore là một trang web bán sách trực tuyến được xây dựng bằng **ASP.NET Core MVC**. Dự án có cấu trúc như sau:

```
BookStore/
├── BookStore.Web/          # Phần giao diện người dùng (Frontend)
│   ├── Controllers/        # Xử lý các yêu cầu từ người dùng
│   ├── Views/             # Các trang hiển thị
│   ├── Models/            # Các lớp dữ liệu cho giao diện
│   ├── Services/          # Các dịch vụ xử lý logic
│   └── wwwroot/           # File tĩnh (CSS, JS, hình ảnh)
├── BookStore.API/          # Phần xử lý dữ liệu (Backend API)
├── BookStore.Core/         # Các lớp dữ liệu chung
└── BookStore.Data/         # Lớp truy cập cơ sở dữ liệu
```

## 🔍 Phân Tích Code Chi Tiết

### 1. BaseController - Lớp Controller Cơ Bản (Độ khó: Trung bình)

```csharp
public class BaseController : Controller
{
    // Kiểm tra người dùng đã đăng nhập chưa
    protected bool IsUserLoggedIn()
    {
        return !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));
    }
    
    // Lấy ID của người dùng hiện tại
    protected int GetCurrentUserId()
    {
        return HttpContext.Session.GetInt32("UserId") ?? 0;
    }
    
    // Kiểm tra quyền admin
    protected IActionResult? VerifyAdminRole()
    {
        if (!IsUserLoggedIn())
            return HandleUnauthorized();
        
        if (!IsCurrentUserAdmin())
            return HandleAccessDenied();
            
        return null; // Người dùng là admin, tiếp tục thực hiện
    }
}
```

**Giải thích chi tiết:**

1. **Kiểm tra đăng nhập:**
   - `HttpContext.Session.GetString("Token")`: Lấy token từ session
   - Nếu token rỗng hoặc null → người dùng chưa đăng nhập
   - Session là nơi lưu trữ thông tin tạm thời của người dùng

2. **Lấy thông tin người dùng:**
   - `HttpContext.Session.GetInt32("UserId")`: Lấy ID người dùng từ session
   - `?? 0`: Nếu null thì trả về 0 (toán tử null coalescing)

3. **Kiểm tra quyền admin:**
   - Sử dụng "Guard Clause" pattern: kiểm tra điều kiện và thoát sớm nếu không hợp lệ
   - Trả về `null` nghĩa là người dùng có quyền, tiếp tục thực hiện

### 2. ApiService - Dịch Vụ Gọi API (Độ khó: Khó)

```csharp
public async Task<T?> GetAsync<T>(string endpoint)
{
    try
    {
        // Thêm token xác thực vào header
        SetAuthorizationHeader();
        
        // Gửi yêu cầu GET đến API
        var response = await _httpClient.GetAsync(endpoint);
        
        if (response.IsSuccessStatusCode)
        {
            // Đọc nội dung trả về và chuyển đổi thành object
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
        
        // Xử lý lỗi 401 (Không có quyền)
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            HandleUnauthorized();
            throw new UnauthorizedAccessException("Token đã hết hạn...");
        }
        
        // Xử lý các lỗi khác
        var errorContent = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"API call failed: {errorContent}");
    }
    catch (UnauthorizedAccessException)
    {
        throw; // Ném lại exception để controller xử lý
    }
}
```

**Giải thích chi tiết:**

1. **Generic Method (Phương thức tổng quát):**
   - `<T>`: Cho phép method này trả về bất kỳ kiểu dữ liệu nào
   - Ví dụ: `GetAsync<List<BookDto>>()` sẽ trả về danh sách sách
   - `T?`: Cho phép trả về null an toàn

2. **Async/Await Pattern:**
   - `async`: Đánh dấu method này chạy bất đồng bộ
   - `await`: Chờ kết quả mà không block UI thread
   - Giúp ứng dụng không bị "đơ" khi gọi API

3. **Xử lý lỗi phân tầng:**
   - Tầng 1: Kiểm tra HTTP status code
   - Tầng 2: Xử lý lỗi xác thực (401)
   - Tầng 3: Xử lý lỗi chung
   - Tầng 4: Ném lại exception cụ thể

4. **JSON Deserialization:**
   - `JsonConvert.DeserializeObject<T>()`: Chuyển JSON string thành object
   - Tự động map các property từ JSON vào object

### 3. Shopping Cart Logic - Logic Giỏ Hàng (Độ khó: Khó)

```csharp
[HttpPost]
public IActionResult AddToCart(int bookId, int quantity = 1)
{
    try
    {
        // Kiểm tra đăng nhập
        if (!IsUserLoggedIn())
        {
            // Nếu là AJAX request, trả về JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập..." });
            }
            // Nếu là request thường, chuyển hướng đến trang đăng nhập
            return RedirectToAction("Login", "Account");
        }

        // Lấy giỏ hàng từ session
        var cart = GetCartFromSession();
        
        // Tìm sản phẩm đã có trong giỏ hàng
        var existingItem = cart.FirstOrDefault(c => c.BookId == bookId);
        
        if (existingItem != null)
        {
            // Nếu đã có, tăng số lượng
            existingItem.Quantity += quantity;
        }
        else
        {
            // Nếu chưa có, thêm mới
            cart.Add(new CartItemViewModel { BookId = bookId, Quantity = quantity });
        }
        
        // Lưu giỏ hàng vào session
        SaveCartToSession(cart);
        
        // Phân biệt AJAX và request thường
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
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error adding to cart");
        return Json(new { success = false, message = "Có lỗi xảy ra" });
    }
}
```

**Giải thích chi tiết:**

1. **Session Management (Quản lý Session):**
   ```csharp
   private List<CartItemViewModel> GetCartFromSession()
   {
       var cartJson = HttpContext.Session.GetString("Cart");
       if (string.IsNullOrEmpty(cartJson))
           return new List<CartItemViewModel>();
       
       try
       {
           return JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson) 
                  ?? new List<CartItemViewModel>();
       }
       catch
       {
           return new List<CartItemViewModel>(); // Trả về giỏ hàng rỗng nếu lỗi
       }
   }
   ```
   - Session lưu trữ dữ liệu tạm thời của người dùng
   - Chuyển đổi object thành JSON để lưu trong session
   - Có xử lý lỗi nếu dữ liệu JSON bị hỏng

2. **AJAX Detection (Phát hiện AJAX):**
   ```csharp
   if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
   {
       return Json(new { success = true, cartCount = cart.Sum(c => c.Quantity) });
   }
   return RedirectToAction("Cart");
   ```
   - Kiểm tra header để biết request từ JavaScript hay không
   - AJAX: Trả về JSON, trang không reload
   - Request thường: Chuyển hướng đến trang mới

3. **Business Logic (Logic nghiệp vụ):**
   ```csharp
   var existingItem = cart.FirstOrDefault(c => c.BookId == bookId);
   if (existingItem != null)
   {
       existingItem.Quantity += quantity; // Cộng dồn số lượng
   }
   else
   {
       cart.Add(new CartItemViewModel { BookId = bookId, Quantity = quantity });
   }
   ```
   - `FirstOrDefault()`: Tìm phần tử đầu tiên hoặc null
   - Không tạo duplicate items, chỉ tăng số lượng
   - Logic đơn giản nhưng hiệu quả

### 4. Discount Calculation - Tính Toán Giảm Giá (Độ khó: Khó)

```csharp
public class CartViewModel
{
    public List<CartItemDetailViewModel> Items { get; set; } = new();

    // Tính tổng tiền (sử dụng Expression-bodied members)
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    public decimal TotalOriginalAmount => Items.Sum(i => i.BookPrice * i.Quantity);
    public decimal TotalSavings => Items.Sum(i => i.TotalSavings);
    public int TotalItems => Items.Sum(i => i.Quantity);
    public bool HasDiscounts => Items.Any(i => i.IsDiscountActive);
    
    // Format tiền tệ cho hiển thị
    public string TotalAmountFormatted => CurrencyHelper.FormatVND(TotalAmount);
}

public class CartItemDetailViewModel
{
    // Kiểm tra khuyến mãi có hiệu lực không
    public bool IsDiscountActive => DiscountPercentage > 0 && 
                                   (!SaleStartDate.HasValue || SaleStartDate <= DateTime.Now) &&
                                   (!SaleEndDate.HasValue || SaleEndDate >= DateTime.Now);
    
    // Tính giá hiệu lực
    public decimal EffectivePrice => IsDiscountActive ? DiscountedPrice : BookPrice;
    public decimal TotalPrice => EffectivePrice * Quantity;
    public decimal TotalSavings => IsDiscountActive ? (BookPrice - DiscountedPrice) * Quantity : 0;
    
    // Tính phần trăm giảm giá
    public decimal DiscountPercentageDisplay
    {
        get
        {
            if (!IsDiscountActive || BookPrice == 0) return 0;
            return Math.Round(((BookPrice - DiscountedPrice) / BookPrice) * 100, 0);
        }
    }
}
```

**Giải thích chi tiết:**

1. **Expression-bodied Members:**
   ```csharp
   public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
   ```
   - Cú pháp ngắn gọn cho computed properties
   - Tính toán mỗi khi được gọi (không cache)
   - Sử dụng LINQ để tính tổng

2. **Logic kiểm tra khuyến mãi:**
   ```csharp
   public bool IsDiscountActive => DiscountPercentage > 0 && 
                                  (!SaleStartDate.HasValue || SaleStartDate <= DateTime.Now) &&
                                  (!SaleEndDate.HasValue || SaleEndDate >= DateTime.Now);
   ```
   - 3 điều kiện phải thỏa mãn:
     - Có phần trăm giảm giá > 0
     - Chưa có ngày bắt đầu HOẶC đã đến ngày bắt đầu
     - Chưa có ngày kết thúc HOẶC chưa quá ngày kết thúc
   - `HasValue`: Kiểm tra DateTime nullable có giá trị không

3. **Tính toán phần trăm:**
   ```csharp
   return Math.Round(((BookPrice - DiscountedPrice) / BookPrice) * 100, 0);
   ```
   - Công thức: (Giá gốc - Giá giảm) / Giá gốc * 100
   - `Math.Round(..., 0)`: Làm tròn không có số thập phân
   - Kiểm tra chia cho 0 để tránh lỗi

4. **Checkout Logic phức tạp:**
   ```csharp
   public class CheckoutViewModel
   {
       public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
       public decimal ShippingFee => (TotalAmount >= 500000 || VoucherFreeShipping) ? 0 : 30000;
       public decimal FinalAmount => Math.Max(0, TotalAmount - DiscountAmount + ShippingFee);
   }
   ```
   - Miễn phí ship cho đơn hàng >= 500k VND
   - `Math.Max(0, ...)`: Đảm bảo tổng tiền không âm
   - Logic nghiệp vụ thực tế của e-commerce

### 5. Checkout Process - Quy Trình Thanh Toán (Độ khó: Rất khó)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Checkout(CheckoutViewModel model)
{
    try
    {
        // Bước 1: Kiểm tra xác thực và validation
        if (!IsUserLoggedIn())
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
            return View(model);

        // Bước 2: Kiểm tra giỏ hàng
        var cart = GetCartFromSession();
        if (!cart.Any())
        {
            TempData["Warning"] = "Giỏ hàng của bạn đang trống.";
            return RedirectToAction("Cart");
        }

        // Bước 3: Chuyển đổi dữ liệu từ giỏ hàng sang đơn hàng
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
                    Price = book.FinalPrice // Sử dụng giá hiện tại
                });
            }
        }

        // Bước 4: Tạo đơn hàng
        var createOrderDto = new CreateOrderDto
        {
            UserId = GetCurrentUserId(),
            Items = orderItems,
            ShippingAddress = model.ShippingAddress,
            PaymentMethod = model.PaymentMethod,
            TotalAmount = model.FinalAmount
        };

        var createdOrder = await _apiService.PostAsync<OrderDto>("orders", createOrderDto);

        // Bước 5: Xử lý kết quả
        if (createdOrder != null)
        {
            HttpContext.Session.Remove("Cart"); // Xóa giỏ hàng
            TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng: #{createdOrder.Id}";
            return RedirectToAction("OrderConfirmation", new { orderId = createdOrder.Id });
        }
        else
        {
            TempData["Error"] = "Không thể đặt hàng. Vui lòng thử lại.";
            return View(model);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing checkout");
        TempData["Error"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.";
        return View(model);
    }
}
```

**Giải thích chi tiết:**

1. **Multi-step Validation (Kiểm tra nhiều bước):**
   - Bước 1: Kiểm tra đăng nhập
   - Bước 2: Kiểm tra dữ liệu form hợp lệ
   - Bước 3: Kiểm tra logic nghiệp vụ (giỏ hàng không rỗng)
   - Mỗi bước thất bại sẽ dừng và trả về lỗi cụ thể

2. **Data Transformation (Chuyển đổi dữ liệu):**
   ```csharp
   foreach (var item in cart)
   {
       var book = await _apiService.GetAsync<BookDto>($"books/{item.BookId}");
       if (book != null)
       {
           orderItems.Add(new CreateOrderItemDto
           {
               BookId = item.BookId,
               Quantity = item.Quantity,
               Price = book.FinalPrice // Lấy giá hiện tại, không phải giá trong cart
           });
       }
   }
   ```
   - Gọi API để lấy thông tin sách mới nhất
   - Sử dụng giá hiện tại thay vì giá cached trong cart
   - Đảm bảo tính chính xác của đơn hàng

3. **Transaction-like Behavior (Hành vi giống giao dịch):**
   ```csharp
   if (createdOrder != null)
   {
       HttpContext.Session.Remove("Cart"); // Chỉ xóa cart khi thành công
       TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng: #{createdOrder.Id}";
       return RedirectToAction("OrderConfirmation", new { orderId = createdOrder.Id });
   }
   ```
   - Cart chỉ được xóa khi đơn hàng tạo thành công
   - Tránh mất dữ liệu khi có lỗi
   - Atomic operation: Tất cả thành công hoặc tất cả thất bại

4. **Comprehensive Error Handling (Xử lý lỗi toàn diện):**
   ```csharp
   catch (Exception ex)
   {
       _logger.LogError(ex, "Error processing checkout");
       TempData["Error"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.";
       return View(model);
   }
   ```
   - Ghi log chi tiết cho developer
   - Hiển thị thông báo thân thiện cho user
   - Trả về form với dữ liệu đã nhập

### 6. Advanced Search với AJAX - Tìm Kiếm Nâng Cao (Độ khó: Khó)

```javascript
function performShopSearch(term) {
    const loadingIndicator = document.getElementById('shopSearchLoading');
    const searchResults = document.getElementById('shopSearchResults');

    // Hiển thị loading
    loadingIndicator.classList.remove('d-none');
    searchResults.classList.add('d-none');

    // Gọi API tìm kiếm
    fetch(`@Url.Action("SearchProducts", "Shop")?term=${encodeURIComponent(term)}`)
        .then(response => response.json())
        .then(data => {
            loadingIndicator.classList.add('d-none');

            if (data.success && data.results && data.results.length > 0) {
                displayShopSearchResults(data.results);
            } else {
                displayShopNoResults();
            }
        })
        .catch(error => {
            console.error('Search error:', error);
            loadingIndicator.classList.add('d-none');
            displayShopSearchError();
        });
}

// Debounced search - Tìm kiếm có độ trễ
searchInput.addEventListener('input', function(e) {
    const term = e.target.value.trim();

    if (term.length < 2) {
        hideShopSearchResults();
        return;
    }

    // Hủy timeout cũ và tạo timeout mới
    clearTimeout(shopSearchTimeout);
    shopSearchTimeout = setTimeout(() => {
        performShopSearch(term);
    }, 300); // Chờ 300ms sau khi user ngừng gõ
});
```

**Giải thích chi tiết:**

1. **Debounced Search Pattern (Tìm kiếm có độ trễ):**
   ```javascript
   clearTimeout(shopSearchTimeout);
   shopSearchTimeout = setTimeout(() => {
       performShopSearch(term);
   }, 300);
   ```
   - Tránh gọi API liên tục khi user đang gõ
   - Chỉ gọi API sau khi user ngừng gõ 300ms
   - Tiết kiệm băng thông và giảm tải server
   - Cải thiện trải nghiệm người dùng

2. **Fetch API với Error Handling:**
   ```javascript
   fetch(`@Url.Action("SearchProducts", "Shop")?term=${encodeURIComponent(term)}`)
       .then(response => response.json())
       .then(data => {
           // Xử lý kết quả thành công
       })
       .catch(error => {
           console.error('Search error:', error);
           displayShopSearchError();
       });
   ```
   - `encodeURIComponent()`: Mã hóa ký tự đặc biệt trong URL
   - Promise chain để xử lý async
   - Catch error để hiển thị thông báo lỗi thân thiện

3. **Dynamic DOM Manipulation (Thao tác DOM động):**
   ```javascript
   function displayShopSearchResults(results) {
       let html = '';
       results.forEach(book => {
           const isDisabled = !book.isInStock;
           const priceDisplay = book.isDiscountActive ?
               `<span class="text-muted text-decoration-line-through">${book.priceFormatted}</span>
                <span class="text-danger fw-bold">${book.discountedPriceFormatted}</span>` :
               `<span class="text-danger fw-bold">${book.priceFormatted}</span>`;

           html += `
               <div class="search-result-item ${isDisabled ? 'disabled' : ''}" data-book-id="${book.id}">
                   <div class="d-flex align-items-center">
                       <img src="${book.imageUrl}" alt="${book.title}" class="me-3 rounded">
                       <div class="flex-grow-1">
                           <h6 class="mb-1">${book.title}</h6>
                           <p class="mb-1 text-muted small">Tác giả: ${book.author}</p>
                           <div>${priceDisplay}</div>
                       </div>
                   </div>
               </div>
           `;
       });

       searchResults.innerHTML = html;
       searchResults.classList.remove('d-none');
   }
   ```
   - Template literals (backticks) để tạo HTML string
   - Conditional rendering: Hiển thị khác nhau cho sách còn/hết hàng
   - Bootstrap classes cho styling
   - Data attributes để lưu thông tin

4. **Keyboard Navigation (Điều hướng bằng bàn phím):**
   ```javascript
   searchInput.addEventListener('keydown', function(e) {
       const items = searchResults.querySelectorAll('.search-result-item:not(.disabled)');
       const activeItem = searchResults.querySelector('.search-result-item.active');

       if (e.key === 'ArrowDown') {
           e.preventDefault();
           navigateShopSearchResults(items, activeItem, 'down');
       } else if (e.key === 'ArrowUp') {
           e.preventDefault();
           navigateShopSearchResults(items, activeItem, 'up');
       } else if (e.key === 'Enter') {
           e.preventDefault();
           if (activeItem && !activeItem.classList.contains('disabled')) {
               const bookId = activeItem.dataset.bookId;
               window.location.href = `@Url.Action("Details", "Shop")/${bookId}`;
           }
       }
   });
   ```
   - Hỗ trợ accessibility (khả năng tiếp cận)
   - `preventDefault()`: Ngăn hành vi mặc định của trình duyệt
   - Quản lý trạng thái active item bằng CSS classes
   - Arrow keys + Enter để điều hướng

### 7. Currency Helper - Trợ Giúp Định Dạng Tiền Tệ (Độ khó: Trung bình)

```csharp
public static class CurrencyHelper
{
    // Định dạng tiền VND
    public static string FormatVND(decimal amount)
    {
        return amount.ToString("N0", CultureInfo.InvariantCulture) + " VNĐ";
    }

    // Chuyển đổi string VND về decimal
    public static decimal ParseVND(string vndString)
    {
        if (string.IsNullOrEmpty(vndString))
            return 0;

        // Loại bỏ các ký tự định dạng
        var cleanString = vndString.Replace("VNĐ", "")
                                  .Replace(" ", "")
                                  .Replace(",", "");

        // Thử chuyển đổi, nếu thất bại trả về 0
        return decimal.TryParse(cleanString, out decimal result) ? result : 0;
    }

    // Định dạng giá có giảm giá
    public static string FormatVNDWithDiscount(decimal originalPrice, decimal discountedPrice)
    {
        if (originalPrice == discountedPrice)
            return FormatVND(originalPrice);

        return $"<span class='text-muted text-decoration-line-through'>{FormatVND(originalPrice)}</span> " +
               $"<span class='text-danger fw-bold'>{FormatVND(discountedPrice)}</span>";
    }
}
```

**Giải thích chi tiết:**

1. **Globalization Best Practices:**
   ```csharp
   amount.ToString("N0", CultureInfo.InvariantCulture)
   ```
   - `"N0"`: Number format, 0 chữ số thập phân
   - `CultureInfo.InvariantCulture`: Định dạng không phụ thuộc vào locale
   - Tự động thêm dấu phẩy phân cách hàng nghìn
   - Ví dụ: 1000000 → "1,000,000"

2. **Safe String Parsing:**
   ```csharp
   var cleanString = vndString.Replace("VNĐ", "")
                             .Replace(" ", "")
                             .Replace(",", "");

   return decimal.TryParse(cleanString, out decimal result) ? result : 0;
   ```
   - Method chaining: Gọi nhiều Replace() liên tiếp
   - `TryParse()`: Không ném exception nếu thất bại
   - Fallback value: Trả về 0 cho input không hợp lệ
   - Input sanitization: Loại bỏ ký tự định dạng

3. **HTML Generation trong Helper:**
   ```csharp
   return $"<span class='text-muted text-decoration-line-through'>{FormatVND(originalPrice)}</span> " +
          $"<span class='text-danger fw-bold'>{FormatVND(discountedPrice)}</span>";
   ```
   - String interpolation với `$"..."`
   - Tạo HTML với Bootstrap classes
   - Strikethrough cho giá gốc, đỏ đậm cho giá giảm
   - Reuse method `FormatVND()` để đảm bảo consistency

### 8. Session Management - Quản Lý Session (Độ khó: Trung bình)

```csharp
private List<CartItemViewModel> GetCartFromSession()
{
    var cartJson = HttpContext.Session.GetString("Cart");
    if (string.IsNullOrEmpty(cartJson))
    {
        return new List<CartItemViewModel>();
    }

    try
    {
        return JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson)
               ?? new List<CartItemViewModel>();
    }
    catch
    {
        // Nếu JSON bị hỏng, trả về giỏ hàng rỗng
        return new List<CartItemViewModel>();
    }
}

private void SaveCartToSession(List<CartItemViewModel> cart)
{
    var cartJson = JsonConvert.SerializeObject(cart);
    HttpContext.Session.SetString("Cart", cartJson);
}
```

**Giải thích chi tiết:**

1. **JSON Serialization Pattern:**
   ```csharp
   // Lưu: Object → JSON String → Session
   var cartJson = JsonConvert.SerializeObject(cart);
   HttpContext.Session.SetString("Cart", cartJson);

   // Lấy: Session → JSON String → Object
   var cartJson = HttpContext.Session.GetString("Cart");
   return JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);
   ```
   - Session chỉ lưu được string, không lưu được object phức tạp
   - Sử dụng JSON để chuyển đổi object thành string
   - `Newtonsoft.Json` library mạnh mẽ cho serialization

2. **Defensive Programming (Lập trình phòng thủ):**
   ```csharp
   try
   {
       return JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson)
              ?? new List<CartItemViewModel>();
   }
   catch
   {
       return new List<CartItemViewModel>(); // Fallback
   }
   ```
   - Try-catch để bắt lỗi JSON parsing
   - Null coalescing `??` để handle null result
   - Graceful degradation: Trả về empty cart thay vì crash app
   - User experience: App tiếp tục hoạt động dù có lỗi data

3. **Session Configuration (Cấu hình Session):**
   ```csharp
   // Trong Program.cs
   builder.Services.AddSession(options =>
   {
       options.IdleTimeout = TimeSpan.FromMinutes(30); // Hết hạn sau 30 phút không hoạt động
       options.Cookie.HttpOnly = true; // Chỉ server mới truy cập được
       options.Cookie.IsEssential = true; // Bỏ qua GDPR consent
   });
   ```
   - `IdleTimeout`: Session tự động xóa sau thời gian không hoạt động
   - `HttpOnly`: Bảo mật, JavaScript không thể truy cập cookie
   - `IsEssential`: Cookie cần thiết cho chức năng, không cần consent

### 9. Parallel API Calls - Gọi API Song Song (Độ khó: Khó)

```csharp
public async Task<IActionResult> Index()
{
    try
    {
        var model = new HomePageViewModel();

        // Tạo các task song song thay vì gọi tuần tự
        var slidersTask = _apiService.GetAsync<List<SliderDto>>("sliders/active");
        var bannersTask = _apiService.GetAsync<List<BannerDto>>("banners/position/home");
        var categoriesTask = _apiService.GetAsync<List<CategoryDto>>("categories");
        var booksTask = _apiService.GetAsync<List<BookDto>>("books");

        // Chờ tất cả tasks hoàn thành
        await Task.WhenAll(slidersTask, bannersTask, categoriesTask, booksTask);

        // Lấy kết quả từ các tasks
        model.Sliders = await slidersTask ?? new List<SliderDto>();
        model.PromotionalBanners = (await bannersTask)?.Take(4).ToList() ?? new List<BannerDto>();
        model.Categories = await categoriesTask ?? new List<CategoryDto>();

        var allBooks = await booksTask ?? new List<BookDto>();
        model.BestSellerBooks = allBooks.Take(8).Select(MappingHelper.MapBookToViewModel).ToList();
        model.FeaturedBooks = allBooks.OrderByDescending(b => b.CreatedAt).Take(4)
                                    .Select(MappingHelper.MapBookToViewModel).ToList();

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading homepage data");
        return View(new HomePageViewModel()); // Fallback empty model
    }
}
```

**Giải thích chi tiết:**

1. **Task Composition Pattern:**
   ```csharp
   // ❌ Cách cũ (Chậm): Tổng thời gian = Tổng thời gian tất cả API calls
   var sliders = await _apiService.GetAsync<List<SliderDto>>("sliders/active");
   var banners = await _apiService.GetAsync<List<BannerDto>>("banners/position/home");
   var categories = await _apiService.GetAsync<List<CategoryDto>>("categories");

   // ✅ Cách mới (Nhanh): Tổng thời gian = Thời gian API call lâu nhất
   var slidersTask = _apiService.GetAsync<List<SliderDto>>("sliders/active");
   var bannersTask = _apiService.GetAsync<List<BannerDto>>("banners/position/home");
   var categoriesTask = _apiService.GetAsync<List<CategoryDto>>("categories");

   await Task.WhenAll(slidersTask, bannersTask, categoriesTask);
   ```
   - Sequential: API calls chạy lần lượt, chậm
   - Parallel: API calls chạy đồng thời, nhanh hơn nhiều
   - Time complexity: O(max) thay vì O(sum)

2. **Task.WhenAll() Usage:**
   ```csharp
   await Task.WhenAll(slidersTask, bannersTask, categoriesTask, booksTask);

   // Lấy kết quả - tasks đã hoàn thành, await trả về ngay lập tức
   model.Sliders = await slidersTask ?? new List<SliderDto>();
   model.PromotionalBanners = (await bannersTask)?.Take(4).ToList() ?? new List<BannerDto>();
   ```
   - `Task.WhenAll()`: Chờ tất cả tasks hoàn thành
   - Nếu có task nào fail, WhenAll sẽ throw exception
   - Sau WhenAll, await các task sẽ return ngay lập tức
   - Null safety với `??` và `?.`

3. **LINQ Optimization:**
   ```csharp
   var allBooks = await booksTask ?? new List<BookDto>();

   // Tái sử dụng data cho nhiều collections
   model.BestSellerBooks = allBooks.Take(8).Select(MappingHelper.MapBookToViewModel).ToList();
   model.FeaturedBooks = allBooks.OrderByDescending(b => b.CreatedAt).Take(4)
                               .Select(MappingHelper.MapBookToViewModel).ToList();
   ```
   - Sử dụng cùng data source cho nhiều collections
   - `Take()` trước `Select()` để giảm số lượng mapping
   - Lazy evaluation: LINQ chỉ execute khi `ToList()`
   - Memory efficient: Không tạo intermediate collections

### 10. Complex ViewModel Mapping - Ánh Xạ ViewModel Phức Tạp (Độ khó: Khó)

```csharp
public static class MappingHelper
{
    public static BookViewModel MapBookToViewModel(BookDto book)
    {
        return new BookViewModel
        {
            Id = book.Id,
            Title = book.Title,
            Description = book.Description ?? "", // Null-safe với default value
            Price = book.Price,

            // Ánh xạ các trường discount phức tạp
            DiscountPercentage = book.DiscountPercentage ?? 0,
            DiscountAmount = book.DiscountAmount,
            IsOnSale = book.IsOnSale,
            SaleStartDate = book.SaleStartDate,
            SaleEndDate = book.SaleEndDate,

            // Null-safe property mapping với fallback values
            ImageUrl = book.ImageUrl ?? "/images/no-image.jpg",
            AuthorName = book.AuthorName ?? "Unknown Author",
            CategoryName = book.CategoryName ?? "Unknown Category",

            // Direct property assignment
            CategoryId = book.CategoryId,
            AuthorId = book.AuthorId,
            ISBN = book.ISBN ?? "",
            Publisher = book.Publisher ?? "",
            PublicationYear = book.PublicationYear ?? 0,
            Quantity = book.Quantity,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt
        };
    }
}
```

**Giải thích chi tiết:**

1. **Null-Safe Mapping Pattern:**
   ```csharp
   ImageUrl = book.ImageUrl ?? "/images/no-image.jpg",
   AuthorName = book.AuthorName ?? "Unknown Author",
   Description = book.Description ?? "",
   ```
   - Null coalescing operator `??` cho default values
   - Defensive programming: Handle null data từ API
   - User experience: Hiển thị fallback values thay vì empty/null
   - Consistency: Standardized default values

2. **Complex Business Logic Mapping:**
   ```csharp
   // Discount fields mapping
   DiscountPercentage = book.DiscountPercentage ?? 0,
   DiscountAmount = book.DiscountAmount,
   IsOnSale = book.IsOnSale,
   SaleStartDate = book.SaleStartDate,
   SaleEndDate = book.SaleEndDate,
   ```
   - Nullable handling: `??` cho nullable decimals
   - Business state: Map tất cả discount-related properties
   - DateTime mapping: Preserve nullable DateTime values
   - Data integrity: Đảm bảo discount data nhất quán

3. **Static Helper Pattern:**
   ```csharp
   public static class MappingHelper
   {
       public static BookViewModel MapBookToViewModel(BookDto book)
       public static CategoryViewModel MapCategoryToViewModel(CategoryDto category)
   }
   ```
   - Stateless design: Không có instance state, pure functions
   - Reusability: Sử dụng ở nhiều controllers
   - Performance: Không có object instantiation overhead
   - Testability: Dễ dàng unit test

## 🎯 Tổng Kết Các Patterns và Kỹ Thuật

### 1. **Repository Pattern (Ngầm định)**
- ApiService hoạt động như repository layer
- Abstraction giữa Controllers và data source
- Centralized API communication logic

### 2. **DTO Pattern với Mapping**
- Tách biệt: API DTOs ↔ ViewModels
- MappingHelper cho clean transformations
- Type safety across layers

### 3. **Session-based State Management**
- Cart persistence qua các requests
- User authentication state
- JSON serialization cho complex objects

### 4. **Comprehensive Error Handling**
- Layered exception handling
- User-friendly error messages
- Logging cho debugging
- Graceful degradation strategies

### 5. **Async/Await Best Practices**
- Non-blocking API calls
- Proper exception propagation
- Task composition với Task.WhenAll

### 6. **Performance Optimization**
- Parallel API execution
- Memory caching strategies
- LINQ optimization
- Debounced search

## 📚 Kết Luận

Website BookStore thể hiện một **kiến trúc MVC chuyên nghiệp** với nhiều **kỹ thuật lập trình nâng cao**:

### 🔴 **Những phần code khó nhất:**
- **Checkout Process**: Multi-step workflow với error handling phức tạp
- **Discount Calculation**: Business logic với time-based validation
- **Parallel API Calls**: Task composition và performance optimization
- **Advanced Search**: Real-time AJAX với debouncing và keyboard navigation

### 🟡 **Những phần code trung bình:**
- **Session Management**: JSON serialization với defensive programming
- **Currency Helper**: Globalization và string manipulation
- **BaseController**: Authentication patterns với guard clauses
- **ViewModel Mapping**: Null-safe mapping với fallback values

### ✅ **Điểm mạnh của code:**
- **Clean Architecture**: Tách biệt rõ ràng các concerns
- **Error Handling**: Comprehensive và user-friendly
- **Performance**: Optimized với parallel execution
- **Security**: Proper authentication và input validation
- **Maintainability**: Code dễ đọc, dễ maintain và extend

### 🎯 **Bài học quan trọng:**
1. **Async/Await**: Cần thiết cho performance và user experience
2. **Error Handling**: Phải có strategy toàn diện
3. **Session Management**: Quan trọng cho state persistence
4. **Business Logic**: Cần tách biệt và test kỹ lưỡng
5. **User Experience**: AJAX và real-time features tăng tính tương tác

Đây là một **foundation tuyệt vời** để học hỏi các kỹ thuật **enterprise-level web development**! 🚀
