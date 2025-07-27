using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Web.Attributes;
using BookStore.Core.DTOs;

namespace BookStore.Web.Controllers
{
    [UserOnly]
    public class UserController : BaseController
    {
        private readonly ApiService _apiService;
        private readonly ILogger<UserController> _logger;

        public UserController(ApiService apiService, ILogger<UserController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: User/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var username = GetCurrentUsername();
                var userId = GetCurrentUserId();
                
                // Get user info
                var user = await _apiService.GetAsync<UserDto>($"auth/users/{userId}");
                
                // Get recent books for recommendations
                var books = await _apiService.GetAsync<List<BookDto>>("books");
                var recentBooks = books?.Take(6).ToList() ?? new List<BookDto>();

                var viewModel = new UserDashboardViewModel
                {
                    User = user,
                    RecentBooks = recentBooks.Select(MapBookToViewModel).ToList(),
                    WelcomeMessage = $"Chào mừng {user?.FirstName ?? username} đến với BookStore!"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user dashboard");
                TempData["Error"] = "Không thể tải trang chính. Vui lòng thử lại sau.";
                return View(new UserDashboardViewModel());
            }
        }

        // GET: User/Profile
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _apiService.GetAsync<UserDto>($"auth/users/{userId}");

                if (user == null)
                {
                    return HandleUnauthorized();
                }

                var viewModel = new UserProfileViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone ?? "",
                    Address = user.Address ?? ""
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                TempData["Error"] = "Không thể tải thông tin cá nhân.";
                return RedirectToAction("Dashboard");
            }
        }

        // POST: User/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = GetCurrentUserId();
                
                var updateDto = new UpdateUserDto
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    Address = model.Address
                };

                var updatedUser = await _apiService.PutAsync<UserDto>($"auth/users/{userId}", updateDto);
                
                if (updatedUser != null)
                {
                    // Update session info
                    HttpContext.Session.SetString("FullName", $"{updatedUser.FirstName} {updatedUser.LastName}");
                    
                    TempData["Success"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Profile");
                }
                else
                {
                    TempData["Error"] = "Không thể cập nhật thông tin. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin.";
            }

            return View(model);
        }

        // GET: User/Orders
        public async Task<IActionResult> Orders()
        {
            try
            {
                var userId = GetCurrentUserId();
                var orders = await _apiService.GetAsync<List<OrderDto>>($"orders/user/{userId}");

                var orderViewModels = orders?.Select(MapOrderToViewModel).ToList() ?? new List<UserOrderViewModel>();
                return View(orderViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user orders");
                TempData["Error"] = "Không thể tải danh sách đơn hàng.";
                return View(new List<UserOrderViewModel>());
            }
        }

        // GET: User/OrderDetails/5
        public async Task<IActionResult> OrderDetails(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var order = await _apiService.GetAsync<OrderDto>($"orders/{id}");

                if (order == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("Orders");
                }

                // Check if user owns this order
                if (order.UserId != userId)
                {
                    TempData["Error"] = "Bạn không có quyền xem đơn hàng này.";
                    return RedirectToAction("Orders");
                }

                var orderViewModel = MapOrderToViewModel(order);
                return View(orderViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details for order: {OrderId}", id);
                TempData["Error"] = "Không thể tải thông tin đơn hàng.";
                return RedirectToAction("Orders");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderOrder(int id)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này" });
                }

                _logger.LogInformation("Starting reorder process for order {OrderId}", id);

                // Call the correct API endpoint
                var result = await _apiService.PostAsync<ReorderResultDto>($"orders/{id}/reorder", new { });

                if (result != null)
                {
                    _logger.LogInformation("Reorder API call successful for order {OrderId}. Success: {Success}", id, result.Success);

                    // Return the result in the format expected by the frontend
                    return Json(new
                    {
                        success = result.Success,
                        message = result.Message,
                        originalOrderId = result.OriginalOrderId,
                        reorderItems = result.ReorderItems,
                        unavailableItems = result.UnavailableItems,
                        priceChangedItems = result.PriceChangedItems,
                        totalAmount = result.TotalAmount,
                        originalTotalAmount = result.OriginalTotalAmount
                    });
                }
                else
                {
                    _logger.LogWarning("Reorder API returned null result for order {OrderId}", id);
                    return Json(new { success = false, message = "Không nhận được phản hồi từ server" });
                }
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Unauthorized access attempt for reorder order {OrderId}", id);
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này" });
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error when reordering order {OrderId}: {Message}", id, httpEx.Message);

                if (httpEx.Message.Contains("401"))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại" });
                }
                else if (httpEx.Message.Contains("404"))
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }
                else if (httpEx.Message.Contains("400"))
                {
                    // Try to extract error message from HTTP exception
                    var errorMessage = "Không thể đặt lại đơn hàng";
                    if (httpEx.Message.Contains(":"))
                    {
                        var parts = httpEx.Message.Split(':', 2);
                        if (parts.Length > 1)
                        {
                            errorMessage = parts[1].Trim();
                        }
                    }
                    return Json(new { success = false, message = errorMessage });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi kết nối đến server" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when reordering order {OrderId}", id);
                return Json(new { success = false, message = "Có lỗi không mong muốn xảy ra. Vui lòng thử lại sau" });
            }
        }

        private BookViewModel MapBookToViewModel(BookDto book)
        {
            return new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description ?? "",
                Price = book.Price,
                ImageUrl = book.ImageUrl ?? "/images/no-image.jpg",
                AuthorName = book.AuthorName ?? "Unknown Author",
                CategoryName = book.CategoryName ?? "Unknown Category",
                ISBN = book.ISBN ?? "",
                Publisher = book.Publisher ?? "",
                PublicationYear = book.PublicationYear ?? 0,
                Quantity = book.Quantity
            };
        }

        private UserOrderViewModel MapOrderToViewModel(OrderDto order)
        {
            return new UserOrderViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                OrderDetails = order.OrderDetails.Select(d => new UserOrderDetailViewModel
                {
                    BookTitle = d.BookTitle,
                    BookImageUrl = d.BookImageUrl,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }


    }
}
