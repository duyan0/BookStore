using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApiService apiService, ILogger<HomeController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra xem người dùng đã đăng nhập và có quyền admin hay không
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                // Get user statistics for dashboard
                var userStats = await _apiService.GetAsync<Core.DTOs.UserStatisticsDto>("auth/users/statistics");
                ViewBag.UserStats = userStats ?? new Core.DTOs.UserStatisticsDto();

                // Get book statistics for dashboard
                var bookStats = await _apiService.GetAsync<Core.DTOs.BookStatisticsDto>("books/statistics");
                ViewBag.BookStats = bookStats ?? new Core.DTOs.BookStatisticsDto();

                // Get order statistics for dashboard
                var orderStats = await _apiService.GetAsync<Core.DTOs.OrderStatisticsDto>("orders/statistics");
                ViewBag.OrderStats = orderStats ?? new Core.DTOs.OrderStatisticsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading statistics for dashboard");
                ViewBag.UserStats = new Core.DTOs.UserStatisticsDto();
                ViewBag.BookStats = new Core.DTOs.BookStatisticsDto();
                ViewBag.OrderStats = new Core.DTOs.OrderStatisticsDto();
            }

            return View();
        }

        private bool IsAdmin()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            return !string.IsNullOrEmpty(isAdmin) && bool.Parse(isAdmin);
        }
    }
} 