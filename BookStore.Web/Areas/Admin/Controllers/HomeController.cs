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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user statistics for dashboard");
                ViewBag.UserStats = new Core.DTOs.UserStatisticsDto();
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