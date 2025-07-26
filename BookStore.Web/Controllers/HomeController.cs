using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Models;
using BookStore.Web.Services;
using BookStore.Web.Helpers;
using BookStore.Core.DTOs;
using System.Diagnostics;

namespace BookStore.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApiService _apiService;

        public HomeController(ILogger<HomeController> logger, ApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new HomePageViewModel();

                // Get user info
                model.IsLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));
                model.UserFullName = HttpContext.Session.GetString("FullName");
                model.IsAdmin = HttpContext.Session.GetString("IsAdmin") == "True";

                // Load homepage data
                var slidersTask = _apiService.GetAsync<List<SliderDto>>("sliders/active");
                var bannersTask = _apiService.GetAsync<List<BannerDto>>("banners/position/home");
                var categoriesTask = _apiService.GetAsync<List<CategoryDto>>("categories");
                var booksTask = _apiService.GetAsync<List<BookDto>>("books");

                await Task.WhenAll(slidersTask, bannersTask, categoriesTask, booksTask);

                model.Sliders = await slidersTask ?? new List<SliderDto>();
                model.PromotionalBanners = (await bannersTask)?.Take(4).ToList() ?? new List<BannerDto>();
                model.Categories = await categoriesTask ?? new List<CategoryDto>();

                var allBooks = await booksTask ?? new List<BookDto>();

                // Get best sellers (books with highest sales - for now, we'll use random selection)
                model.BestSellerBooks = allBooks.Take(8).Select(MappingHelper.MapBookToViewModel).ToList();

                // Get featured books (newest books)
                model.FeaturedBooks = allBooks.OrderByDescending(b => b.CreatedAt).Take(4).Select(MappingHelper.MapBookToViewModel).ToList();

                // Get new books
                model.NewBooks = allBooks.OrderByDescending(b => b.CreatedAt).Take(6).Select(MappingHelper.MapBookToViewModel).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading homepage data");

                // Return empty model if API calls fail
                var model = new HomePageViewModel
                {
                    IsLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("Token")),
                    UserFullName = HttpContext.Session.GetString("FullName"),
                    IsAdmin = HttpContext.Session.GetString("IsAdmin") == "True"
                };

                return View(model);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
