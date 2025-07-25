using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Core.DTOs;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SlidersController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<SlidersController> _logger;

        public SlidersController(ApiService apiService, ILogger<SlidersController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Admin/Sliders
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                var sliders = await _apiService.GetAsync<List<SliderDto>>("sliders");
                return View(sliders ?? new List<SliderDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sliders");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách slider";
                return View(new List<SliderDto>());
            }
        }

        // GET: Admin/Sliders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                var slider = await _apiService.GetAsync<SliderDto>($"sliders/{id}");
                if (slider == null)
                {
                    TempData["Error"] = "Không tìm thấy slider";
                    return RedirectToAction(nameof(Index));
                }
                return View(slider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading slider {SliderId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin slider";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Sliders/Create
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            return View(new CreateSliderDto());
        }

        // POST: Admin/Sliders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSliderDto createSliderDto)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (!ModelState.IsValid)
            {
                return View(createSliderDto);
            }

            try
            {
                var slider = await _apiService.PostAsync<SliderDto>("sliders", createSliderDto);
                TempData["Success"] = "Tạo slider thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating slider");
                TempData["Error"] = "Có lỗi xảy ra khi tạo slider";
                return View(createSliderDto);
            }
        }

        // GET: Admin/Sliders/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                var slider = await _apiService.GetAsync<SliderDto>($"sliders/{id}");
                if (slider == null)
                {
                    TempData["Error"] = "Không tìm thấy slider";
                    return RedirectToAction(nameof(Index));
                }

                var updateDto = new UpdateSliderDto
                {
                    Title = slider.Title,
                    Description = slider.Description,
                    ImageUrl = slider.ImageUrl,
                    LinkUrl = slider.LinkUrl,
                    DisplayOrder = slider.DisplayOrder,
                    IsActive = slider.IsActive,
                    ButtonText = slider.ButtonText,
                    ButtonStyle = slider.ButtonStyle
                };

                ViewBag.SliderId = id;
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading slider {SliderId} for edit", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin slider";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Sliders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateSliderDto updateSliderDto)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (!ModelState.IsValid)
            {
                return View(updateSliderDto);
            }

            try
            {
                await _apiService.PutAsync<SliderDto>($"sliders/{id}", updateSliderDto);
                TempData["Success"] = "Cập nhật slider thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating slider {SliderId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật slider";
                return View(updateSliderDto);
            }
        }

        // GET: Admin/Sliders/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                var slider = await _apiService.GetAsync<SliderDto>($"sliders/{id}");
                if (slider == null)
                {
                    TempData["Error"] = "Không tìm thấy slider";
                    return RedirectToAction(nameof(Index));
                }
                return View(slider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading slider {SliderId} for delete", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin slider";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Sliders/Delete/5
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
                await _apiService.DeleteAsync($"sliders/{id}");
                TempData["Success"] = "Xóa slider thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting slider {SliderId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa slider";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Sliders/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                await _apiService.PutAsync<object>($"sliders/{id}/toggle-active", new { });
                return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling slider {SliderId} status", id);
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
