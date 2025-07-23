using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Attributes;
using BookStore.Core.DTOs;
using BookStore.Web.Models;
using BookStore.Web.Helpers;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class OrdersController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ApiService apiService, ILogger<OrdersController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _apiService.GetAsync<List<OrderDto>>("orders");
                var orderViewModels = orders?.Select(MapToViewModel).ToList() ?? new List<OrderViewModel>();
                return View(orderViewModels);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                TempData["Error"] = "Không thể tải danh sách đơn hàng. Vui lòng thử lại sau.";
                return View(new List<OrderViewModel>());
            }
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _apiService.GetAsync<OrderDto>($"orders/{id}");
                if (order == null)
                {
                    return NotFound();
                }

                var orderViewModel = MapToViewModel(order);
                return View(orderViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order details for ID: {OrderId}", id);
                TempData["Error"] = "Không thể tải thông tin đơn hàng.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Orders/Statistics
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var statistics = await _apiService.GetAsync<OrderStatisticsDto>("orders/statistics");
                return View(statistics ?? new OrderStatisticsDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order statistics");
                TempData["Error"] = "Không thể tải thống kê đơn hàng.";
                return View(new OrderStatisticsDto());
            }
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var response = await _apiService.PutAsync<object>($"orders/{id}/status", status);
                
                if (response != null)
                {
                    TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công.";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái đơn hàng.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for ID: {OrderId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái đơn hàng.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync($"orders/{id}");
                
                if (response)
                {
                    TempData["Success"] = "Xóa đơn hàng thành công.";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xóa đơn hàng.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order ID: {OrderId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa đơn hàng.";
            }

            return RedirectToAction(nameof(Index));
        }

        private OrderViewModel MapToViewModel(OrderDto dto)
        {
            return new OrderViewModel
            {
                Id = dto.Id,
                UserId = dto.UserId,
                UserName = dto.UserName,
                UserFullName = dto.UserFullName,
                OrderDate = dto.OrderDate,
                TotalAmount = dto.TotalAmount,
                Status = dto.Status,
                ShippingAddress = dto.ShippingAddress,
                PaymentMethod = dto.PaymentMethod,
                OrderDetails = dto.OrderDetails.Select(d => new OrderDetailViewModel
                {
                    Id = d.Id,
                    BookId = d.BookId,
                    BookTitle = d.BookTitle,
                    BookImageUrl = d.BookImageUrl,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList(),
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }

    public class OrderViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public List<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Computed properties
        public string OrderDateFormatted => OrderDate.ToString("dd/MM/yyyy HH:mm");
        public string TotalAmountFormatted => CurrencyHelper.FormatVND(TotalAmount);
        public string StatusBadgeClass => Status switch
        {
            "Pending" => "bg-warning",
            "Processing" => "bg-info",
            "Completed" => "bg-success",
            "Cancelled" => "bg-danger",
            _ => "bg-secondary"
        };
        public string StatusText => Status switch
        {
            "Pending" => "Chờ xử lý",
            "Processing" => "Đang xử lý",
            "Completed" => "Hoàn thành",
            "Cancelled" => "Đã hủy",
            _ => Status
        };
    }

    public class OrderDetailViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;

        public string UnitPriceFormatted => CurrencyHelper.FormatVND(UnitPrice);
        public string TotalPriceFormatted => CurrencyHelper.FormatVND(TotalPrice);
    }
}
