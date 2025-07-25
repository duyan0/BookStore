using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Core.DTOs;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BannersController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<BannersController> _logger;

        public BannersController(ApiService apiService, ILogger<BannersController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Admin/Banners
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                var banners = await _apiService.GetAsync<List<BannerDto>>("banners");
                return View(banners ?? new List<BannerDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banners");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách banner";
                return View(new List<BannerDto>());
            }
        }

        // GET: Admin/Banners/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                var banner = await _apiService.GetAsync<BannerDto>($"banners/{id}");
                if (banner == null)
                {
                    TempData["Error"] = "Không tìm thấy banner";
                    return RedirectToAction(nameof(Index));
                }
                return View(banner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banner {BannerId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin banner";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Banners/Create
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            return View(new CreateBannerDto());
        }

        // POST: Admin/Banners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBannerDto createBannerDto)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (!ModelState.IsValid)
            {
                return View(createBannerDto);
            }

            try
            {
                var banner = await _apiService.PostAsync<BannerDto>("banners", createBannerDto);
                TempData["Success"] = "Tạo banner thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating banner");
                TempData["Error"] = "Có lỗi xảy ra khi tạo banner";
                return View(createBannerDto);
            }
        }

        // GET: Admin/Banners/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                var banner = await _apiService.GetAsync<BannerDto>($"banners/{id}");
                if (banner == null)
                {
                    TempData["Error"] = "Không tìm thấy banner";
                    return RedirectToAction(nameof(Index));
                }

                var updateDto = new UpdateBannerDto
                {
                    Title = banner.Title,
                    Description = banner.Description,
                    ImageUrl = banner.ImageUrl,
                    LinkUrl = banner.LinkUrl,
                    DisplayOrder = banner.DisplayOrder,
                    IsActive = banner.IsActive,
                    Position = banner.Position,
                    Size = banner.Size,
                    ButtonText = banner.ButtonText,
                    ButtonStyle = banner.ButtonStyle,
                    StartDate = banner.StartDate,
                    EndDate = banner.EndDate
                };

                ViewBag.BannerId = id;
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banner {BannerId} for edit", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin banner";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Banners/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateBannerDto updateBannerDto)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (!ModelState.IsValid)
            {
                return View(updateBannerDto);
            }

            try
            {
                await _apiService.PutAsync<BannerDto>($"banners/{id}", updateBannerDto);
                TempData["Success"] = "Cập nhật banner thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner {BannerId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật banner";
                return View(updateBannerDto);
            }
        }

        // GET: Admin/Banners/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                var banner = await _apiService.GetAsync<BannerDto>($"banners/{id}");
                if (banner == null)
                {
                    TempData["Error"] = "Không tìm thấy banner";
                    return RedirectToAction(nameof(Index));
                }
                return View(banner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banner {BannerId} for delete", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin banner";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Banners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                await _apiService.DeleteAsync($"banners/{id}");
                TempData["Success"] = "Xóa banner thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting banner {BannerId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa banner";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Banners/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                await _apiService.PutAsync<object>($"banners/{id}/toggle-active", new { });
                return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling banner {BannerId} status", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái" });
            }
        }

        private bool IsAdmin()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            return !string.IsNullOrEmpty(isAdmin) && bool.Parse(isAdmin);
        }
    }
}
