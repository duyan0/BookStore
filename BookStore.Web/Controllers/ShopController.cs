using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Core.DTOs;
using BookStore.Web.Helpers;
using Newtonsoft.Json;
using Net.payOS.Types;

namespace BookStore.Web.Controllers
{
    public class ShopController : BaseController
    {
        private readonly ApiService _apiService;
        private readonly ILogger<ShopController> _logger;
        private readonly IPayOSService _payOSService;

        public ShopController(ApiService apiService, ILogger<ShopController> logger, IPayOSService payOSService)
        {
            _apiService = apiService;
            _logger = logger;
            _payOSService = payOSService;
        }

        // GET: Shop
        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1, int pageSize = 15)
        {
            // Redirect admin users to admin panel
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            try
            {
                var books = await _apiService.GetAsync<List<BookDto>>("books");
                var categories = await _apiService.GetAsync<List<CategoryDto>>("categories");

                if (books == null) books = new List<BookDto>();
                if (categories == null) categories = new List<CategoryDto>();

                // Filter by search
                if (!string.IsNullOrEmpty(search))
                {
                    books = books.Where(b =>
                        b.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (b.AuthorName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (b.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                }

                // Filter by category
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    books = books.Where(b => b.CategoryId == categoryId.Value).ToList();
                }

                // Pagination
                var totalBooks = books.Count;
                var totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);
                var pagedBooks = books.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var viewModel = new ShopViewModel
                {
                    Books = pagedBooks.Select(MappingHelper.MapBookToViewModel).ToList(),
                    Categories = categories.Select(MappingHelper.MapCategoryToViewModel).ToList(),
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    TotalBooks = totalBooks,
                    SearchTerm = search ?? "",
                    SelectedCategoryId = categoryId,
                    HasPreviousPage = page > 1,
                    HasNextPage = page < totalPages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shop");
                TempData["Error"] = "Không thể tải danh sách sách. Vui lòng thử lại sau.";
                return View(new ShopViewModel());
            }
        }

        // GET: Shop/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Redirect admin users to admin panel
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            try
            {
                var book = await _apiService.GetAsync<BookDto>($"books/{id}");
                
                if (book == null)
                {
                    TempData["Error"] = "Không tìm thấy sách.";
                    return RedirectToAction("Index");
                }

                // Get related books (same category)
                var allBooks = await _apiService.GetAsync<List<BookDto>>("books");
                var relatedBooks = allBooks?
                    .Where(b => b.CategoryId == book.CategoryId && b.Id != book.Id)
                    .Take(4)
                    .ToList() ?? new List<BookDto>();

                var viewModel = new BookDetailsViewModel
                {
                    Book = MappingHelper.MapBookToViewModel(book),
                    RelatedBooks = relatedBooks.Select(MappingHelper.MapBookToViewModel).ToList(),
                    Quantity = 1
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book details for ID: {BookId}", id);
                TempData["Error"] = "Không thể tải thông tin sách.";
                return RedirectToAction("Index");
            }
        }

        // POST: Shop/AddToCart
        [HttpPost]
        public IActionResult AddToCart(int bookId, int quantity = 1)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sách vào giỏ hàng." });
                    }
                    TempData["Warning"] = "Vui lòng đăng nhập để thêm sách vào giỏ hàng.";
                    return RedirectToAction("Login", "Account");
                }

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
                    return Json(new { success = true, message = "Đã thêm sách vào giỏ hàng!" });
                }

                TempData["Success"] = "Đã thêm sách vào giỏ hàng!";
                return RedirectToAction("Details", new { id = bookId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding book to cart: {BookId}", bookId);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Không thể thêm sách vào giỏ hàng." });
                }

                TempData["Error"] = "Không thể thêm sách vào giỏ hàng.";
                return RedirectToAction("Details", new { id = bookId });
            }
        }

        // GET: Shop/GetCartCount
        [HttpGet]
        public IActionResult GetCartCount()
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return Json(new { count = 0 });
                }

                var cart = GetCartFromSession();
                var totalItems = cart.Sum(c => c.Quantity);

                return Json(new { count = totalItems });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count");
                return Json(new { count = 0 });
            }
        }

        // GET: Shop/Cart
        public async Task<IActionResult> Cart()
        {
            // Redirect admin users to admin panel
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                var cart = GetCartFromSession();
                var cartViewModel = new CartViewModel();

                if (cart.Any())
                {
                    // Get book details for cart items
                    var books = await _apiService.GetAsync<List<BookDto>>("books");
                    
                    if (books != null)
                    {
                        foreach (var cartItem in cart)
                        {
                            var book = books.FirstOrDefault(b => b.Id == cartItem.BookId);
                            if (book != null)
                            {
                                cartViewModel.Items.Add(new CartItemDetailViewModel
                                {
                                    BookId = book.Id,
                                    BookTitle = book.Title,
                                    BookPrice = book.Price,
                                    BookImageUrl = book.ImageUrl ?? "/images/no-image.jpg",
                                    Quantity = cartItem.Quantity,
                                    MaxQuantity = book.Quantity,
                                    // Discount information
                                    DiscountPercentage = book.DiscountPercentage ?? 0,
                                    DiscountAmount = book.DiscountAmount,
                                    IsOnSale = book.IsOnSale,
                                    SaleStartDate = book.SaleStartDate,
                                    SaleEndDate = book.SaleEndDate,
                                    DiscountedPrice = book.DiscountedPrice,
                                    IsDiscountActive = book.IsDiscountActive
                                });
                            }
                        }
                    }
                }

                return View(cartViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart");
                TempData["Error"] = "Không thể tải giỏ hàng.";
                return View(new CartViewModel());
            }
        }

        // POST: Shop/UpdateCart
        [HttpPost]
        public IActionResult UpdateCart(int bookId, int quantity)
        {
            try
            {
                var cart = GetCartFromSession();
                var item = cart.FirstOrDefault(c => c.BookId == bookId);

                if (item != null)
                {
                    if (quantity <= 0)
                    {
                        cart.Remove(item);
                    }
                    else
                    {
                        item.Quantity = quantity;
                    }
                }

                SaveCartToSession(cart);
                TempData["Success"] = "Đã cập nhật giỏ hàng!";

                return RedirectToAction("Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart");
                TempData["Error"] = "Không thể cập nhật giỏ hàng.";
                return RedirectToAction("Cart");
            }
        }

        // POST: Shop/RemoveFromCart
        [HttpPost]
        public IActionResult RemoveFromCart(int bookId)
        {
            try
            {
                var cart = GetCartFromSession();
                var item = cart.FirstOrDefault(c => c.BookId == bookId);

                if (item != null)
                {
                    cart.Remove(item);
                    SaveCartToSession(cart);
                    TempData["Success"] = "Đã xóa sách khỏi giỏ hàng!";
                }

                return RedirectToAction("Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart");
                TempData["Error"] = "Không thể xóa sách khỏi giỏ hàng.";
                return RedirectToAction("Cart");
            }
        }

        // GET: Shop/Checkout
        public async Task<IActionResult> Checkout()
        {
            // Redirect admin users to admin panel
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            try
            {
                if (!IsUserLoggedIn())
                {
                    TempData["Warning"] = "Vui lòng đăng nhập để thanh toán.";
                    return RedirectToAction("Login", "Account");
                }

                var cart = GetCartFromSession();
                if (!cart.Any())
                {
                    TempData["Warning"] = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Cart");
                }

                // Get user info for checkout form
                var userId = GetCurrentUserId();
                var user = await _apiService.GetAsync<UserDto>($"auth/users/{userId}");

                // Get book details for cart items
                var books = await _apiService.GetAsync<List<BookDto>>("books");
                var checkoutViewModel = new CheckoutViewModel
                {
                    ShippingAddress = user?.Address ?? "",
                    PaymentMethod = "COD", // Default to Cash on Delivery
                    Items = new List<CartItemDetailViewModel>()
                };

                if (books != null)
                {
                    foreach (var cartItem in cart)
                    {
                        var book = books.FirstOrDefault(b => b.Id == cartItem.BookId);
                        if (book != null)
                        {
                            checkoutViewModel.Items.Add(new CartItemDetailViewModel
                            {
                                BookId = book.Id,
                                BookTitle = book.Title,
                                BookPrice = book.Price,
                                BookImageUrl = book.ImageUrl ?? "/images/no-image.jpg",
                                Quantity = cartItem.Quantity,
                                MaxQuantity = book.Quantity,
                                // Discount information
                                DiscountPercentage = book.DiscountPercentage ?? 0,
                                DiscountAmount = book.DiscountAmount,
                                IsOnSale = book.IsOnSale,
                                SaleStartDate = book.SaleStartDate,
                                SaleEndDate = book.SaleEndDate,
                                DiscountedPrice = book.DiscountedPrice,
                                IsDiscountActive = book.IsDiscountActive
                            });
                        }
                    }
                }

                return View(checkoutViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout");
                TempData["Error"] = "Không thể tải trang thanh toán.";
                return RedirectToAction("Cart");
            }
        }

        // POST: Shop/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            // Redirect admin users to admin panel
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!ModelState.IsValid)
                {
                    // Rebuild the model with book details if validation fails
                    await RebuildCheckoutModel(model);
                    return View(model);
                }

                var cart = GetCartFromSession();
                if (!cart.Any())
                {
                    TempData["Warning"] = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Cart");
                }

                var userId = GetCurrentUserId();

                // Create order
                // Handle PayOS payment first (before creating order)
                if (model.PaymentMethod == "PayOS")
                {
                    try
                    {
                        // Generate a temporary order ID for PayOS (using timestamp)
                        var tempOrderId = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % int.MaxValue);

                        // Create PayOS payment items
                        var paymentItems = model.Items.Select(item => new ItemData(
                            name: item.BookTitle,
                            quantity: item.Quantity,
                            price: (int)item.EffectivePrice
                        )).ToList();

                        // Test PayOS payment creation first
                        var paymentResult = await _payOSService.CreatePaymentAsync(
                            orderId: tempOrderId,
                            amount: model.FinalAmount,
                            description: $"BookStore #{tempOrderId}",
                            items: paymentItems
                        );

                        // If PayOS payment creation successful, create the actual order
                        var createOrderDto = new CreateOrderDto
                        {
                            UserId = userId,
                            ShippingAddress = model.ShippingAddress,
                            PaymentMethod = model.PaymentMethod,

                            // Voucher information
                            VoucherCode = model.VoucherCode,
                            VoucherDiscount = model.VoucherDiscount,
                            FreeShipping = model.VoucherFreeShipping,
                            ShippingFee = model.ShippingFee,
                            SubTotal = model.TotalAmount,

                            OrderDetails = cart.Select(c => new CreateOrderDetailDto
                            {
                                BookId = c.BookId,
                                Quantity = c.Quantity,
                                UnitPrice = model.Items.FirstOrDefault(i => i.BookId == c.BookId)?.EffectivePrice ?? 0
                            }).ToList()
                        };

                        var createdOrder = await _apiService.PostAsync<OrderDto>("orders", createOrderDto);

                        if (createdOrder != null)
                        {
                            // Store order ID in session for payment return handling
                            HttpContext.Session.SetInt32("PendingOrderId", createdOrder.Id);

                            // Store cart in session temporarily (in case payment fails)
                            HttpContext.Session.SetString("TempCart", JsonConvert.SerializeObject(cart));

                            _logger.LogInformation("PayOS payment created successfully for order {OrderId}. Redirecting to: {PaymentUrl}",
                                createdOrder.Id, paymentResult.checkoutUrl);

                            // Redirect to PayOS payment page
                            return Redirect(paymentResult.checkoutUrl);
                        }
                        else
                        {
                            _logger.LogError("Failed to create order after successful PayOS payment creation");
                            TempData["Error"] = "Không thể tạo đơn hàng. Vui lòng thử lại.";
                            await RebuildCheckoutModel(model);
                            return View(model);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating PayOS payment");
                        TempData["Error"] = "Không thể tạo liên kết thanh toán PayOS. Vui lòng thử lại hoặc chọn phương thức thanh toán khác.";
                        await RebuildCheckoutModel(model);
                        return View(model);
                    }
                }
                else
                {
                    // Traditional payment methods (COD, Bank Transfer, etc.)
                    var createOrderDto = new CreateOrderDto
                    {
                        UserId = userId,
                        ShippingAddress = model.ShippingAddress,
                        PaymentMethod = model.PaymentMethod,

                        // Voucher information
                        VoucherCode = model.VoucherCode,
                        VoucherDiscount = model.VoucherDiscount,
                        FreeShipping = model.VoucherFreeShipping,
                        ShippingFee = model.ShippingFee,
                        SubTotal = model.TotalAmount,

                        OrderDetails = cart.Select(c => new CreateOrderDetailDto
                        {
                            BookId = c.BookId,
                            Quantity = c.Quantity,
                            UnitPrice = model.Items.FirstOrDefault(i => i.BookId == c.BookId)?.EffectivePrice ?? 0
                        }).ToList()
                    };

                    var createdOrder = await _apiService.PostAsync<OrderDto>("orders", createOrderDto);

                    if (createdOrder != null)
                    {
                        // Clear cart after successful order
                        HttpContext.Session.Remove("Cart");

                        TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng: #{createdOrder.Id}";
                        return RedirectToAction("OrderConfirmation", new { orderId = createdOrder.Id });
                    }
                    else
                    {
                        TempData["Error"] = "Không thể đặt hàng. Vui lòng thử lại.";
                        await RebuildCheckoutModel(model);
                        return View(model);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout");
                TempData["Error"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.";
                await RebuildCheckoutModel(model);
                return View(model);
            }
        }

        // GET: Shop/PaymentReturn - PayOS success callback
        public async Task<IActionResult> PaymentReturn(int orderCode, string status)
        {
            try
            {
                _logger.LogInformation("PayOS payment return: OrderCode={OrderCode}, Status={Status}", orderCode, status);

                // Get the pending order ID from session
                var pendingOrderId = HttpContext.Session.GetInt32("PendingOrderId");

                if (pendingOrderId == null)
                {
                    _logger.LogWarning("No pending order ID found in session");
                    TempData["Error"] = "Phiên thanh toán không hợp lệ.";
                    return RedirectToAction("Cart");
                }

                if (status == "PAID" || status == "SUCCESS")
                {
                    // Payment successful - clear cart and session data
                    HttpContext.Session.Remove("Cart");
                    HttpContext.Session.Remove("TempCart");
                    HttpContext.Session.Remove("PendingOrderId");

                    _logger.LogInformation("PayOS payment successful for order {OrderId}", pendingOrderId);
                    TempData["Success"] = $"Thanh toán thành công! Mã đơn hàng: #{pendingOrderId}";
                    return RedirectToAction("OrderConfirmation", new { orderId = pendingOrderId });
                }
                else
                {
                    // Payment failed - restore cart and clean up
                    _logger.LogWarning("PayOS payment failed for order {OrderCode} with status {Status}", orderCode, status);

                    // Restore cart from temp storage
                    var tempCartJson = HttpContext.Session.GetString("TempCart");
                    if (!string.IsNullOrEmpty(tempCartJson))
                    {
                        HttpContext.Session.SetString("Cart", tempCartJson);
                        HttpContext.Session.Remove("TempCart");
                    }

                    HttpContext.Session.Remove("PendingOrderId");

                    TempData["Error"] = "Thanh toán không thành công. Giỏ hàng của bạn đã được khôi phục. Vui lòng thử lại.";
                    return RedirectToAction("Checkout");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS payment return");

                // Restore cart in case of error
                var tempCartJson = HttpContext.Session.GetString("TempCart");
                if (!string.IsNullOrEmpty(tempCartJson))
                {
                    HttpContext.Session.SetString("Cart", tempCartJson);
                    HttpContext.Session.Remove("TempCart");
                }

                TempData["Error"] = "Có lỗi xảy ra khi xử lý thanh toán. Giỏ hàng đã được khôi phục.";
                return RedirectToAction("Cart");
            }
        }

        // GET: Shop/PaymentCancel - PayOS cancel callback
        public IActionResult PaymentCancel(int orderCode)
        {
            try
            {
                _logger.LogInformation("PayOS payment cancelled for order {OrderCode}", orderCode);

                // Restore cart from temp storage
                var tempCartJson = HttpContext.Session.GetString("TempCart");
                if (!string.IsNullOrEmpty(tempCartJson))
                {
                    HttpContext.Session.SetString("Cart", tempCartJson);
                    HttpContext.Session.Remove("TempCart");
                    _logger.LogInformation("Cart restored after payment cancellation");
                }

                // Clear pending order from session
                HttpContext.Session.Remove("PendingOrderId");

                TempData["Warning"] = "Thanh toán đã bị hủy. Giỏ hàng của bạn đã được khôi phục. Bạn có thể thử lại hoặc chọn phương thức thanh toán khác.";
                return RedirectToAction("Checkout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS payment cancellation");

                // Try to restore cart even in error case
                var tempCartJson = HttpContext.Session.GetString("TempCart");
                if (!string.IsNullOrEmpty(tempCartJson))
                {
                    HttpContext.Session.SetString("Cart", tempCartJson);
                    HttpContext.Session.Remove("TempCart");
                }

                TempData["Error"] = "Có lỗi xảy ra khi hủy thanh toán. Giỏ hàng đã được khôi phục.";
                return RedirectToAction("Cart");
            }
        }

        // GET: Shop/OrderConfirmation
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            // Redirect admin users to admin panel
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                var order = await _apiService.GetAsync<OrderDto>($"orders/{orderId}");

                if (order == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("Index");
                }

                // Check if user owns this order
                var userId = GetCurrentUserId();
                if (order.UserId != userId)
                {
                    TempData["Error"] = "Bạn không có quyền xem đơn hàng này.";
                    return RedirectToAction("Index");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order confirmation for order: {OrderId}", orderId);
                TempData["Error"] = "Không thể tải thông tin đơn hàng.";
                return RedirectToAction("Index");
            }
        }

        // POST: Shop/ValidateVoucher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateVoucher(string voucherCode, decimal orderAmount)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng voucher." });
                }

                if (string.IsNullOrEmpty(voucherCode))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mã voucher." });
                }

                var userId = GetCurrentUserId();
                var validationDto = new
                {
                    Code = voucherCode,
                    OrderAmount = orderAmount,
                    UserId = userId
                };

                var result = await _apiService.PostAsync<VoucherValidationResultDto>("vouchers/validate", validationDto);

                if (result != null)
                {
                    _logger.LogInformation("Voucher validation result: IsValid={IsValid}, Message={Message}, DiscountAmount={DiscountAmount}, FreeShipping={FreeShipping}",
                        result.IsValid, result.Message, result.DiscountAmount, result.FreeShipping);

                    return Json(new {
                        success = result.IsValid,
                        message = result.Message,
                        discountAmount = result.DiscountAmount,
                        freeShipping = result.FreeShipping
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể kiểm tra voucher. Vui lòng thử lại." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating voucher: {VoucherCode}", voucherCode);
                return Json(new { success = false, message = "Có lỗi xảy ra khi kiểm tra voucher." });
            }
        }

        // GET: Shop/SearchProducts - AJAX endpoint for autocomplete
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string term)
        {
            try
            {
                if (string.IsNullOrEmpty(term) || term.Length < 2)
                {
                    return Json(new { success = false, message = "Search term too short" });
                }

                var books = await _apiService.GetAsync<List<BookDto>>($"books/search?term={Uri.EscapeDataString(term)}");

                if (books != null)
                {
                    var results = books.Take(10).Select(book => new
                    {
                        id = book.Id,
                        title = book.Title,
                        author = book.AuthorName,
                        category = book.CategoryName,
                        price = book.Price,
                        priceFormatted = CurrencyHelper.FormatVND(book.Price),
                        imageUrl = book.ImageUrl ?? "/images/no-image.jpg",
                        isInStock = book.Quantity > 0,
                        quantity = book.Quantity,
                        // Discount information
                        isOnSale = book.IsOnSale,
                        discountedPrice = book.DiscountedPrice,
                        discountedPriceFormatted = CurrencyHelper.FormatVND(book.DiscountedPrice),
                        isDiscountActive = book.IsDiscountActive
                    }).ToList();

                    return Json(new { success = true, results });
                }

                return Json(new { success = false, message = "No results found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products: {Term}", term);
                return Json(new { success = false, message = "Search error occurred" });
            }
        }

        // Helper methods
        private List<CartItemViewModel> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItemViewModel>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson) ?? new List<CartItemViewModel>();
            }
            catch
            {
                return new List<CartItemViewModel>();
            }
        }

        private void SaveCartToSession(List<CartItemViewModel> cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }

        private async Task RebuildCheckoutModel(CheckoutViewModel model)
        {
            try
            {
                var cart = GetCartFromSession();
                if (cart.Any())
                {
                    // Get book details for cart items
                    var books = await _apiService.GetAsync<List<BookDto>>("books");
                    model.Items = new List<CartItemDetailViewModel>();

                    if (books != null)
                    {
                        foreach (var cartItem in cart)
                        {
                            var book = books.FirstOrDefault(b => b.Id == cartItem.BookId);
                            if (book != null)
                            {
                                model.Items.Add(new CartItemDetailViewModel
                                {
                                    BookId = book.Id,
                                    BookTitle = book.Title,
                                    BookPrice = book.Price,
                                    BookImageUrl = book.ImageUrl ?? "/images/no-image.jpg",
                                    Quantity = cartItem.Quantity,
                                    MaxQuantity = book.Quantity,
                                    // Discount information
                                    DiscountPercentage = book.DiscountPercentage ?? 0,
                                    DiscountAmount = book.DiscountAmount,
                                    IsOnSale = book.IsOnSale,
                                    SaleStartDate = book.SaleStartDate,
                                    SaleEndDate = book.SaleEndDate,
                                    DiscountedPrice = book.DiscountedPrice,
                                    IsDiscountActive = book.IsDiscountActive
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding checkout model");
                // If rebuild fails, at least ensure Items is not null
                model.Items ??= new List<CartItemDetailViewModel>();
            }
        }


    }
}
