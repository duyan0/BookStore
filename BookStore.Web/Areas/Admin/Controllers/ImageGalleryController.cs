using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Attributes;
using BookStore.Core.DTOs;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class ImageGalleryController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<ImageGalleryController> _logger;

        public ImageGalleryController(ApiService apiService, ILogger<ImageGalleryController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Admin/ImageGallery
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiService.GetAsync<ImageListResponseDto>("BookImage/list");
                var images = response?.Data ?? new List<ImageFileDto>();
                
                return View(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading image gallery");
                TempData["Error"] = "Không thể tải danh sách ảnh.";
                return View(new List<ImageFileDto>());
            }
        }

        // POST: Admin/ImageGallery/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return Json(new { success = false, message = "Tên file không hợp lệ" });
                }

                var response = await _apiService.DeleteAsync($"BookImage/{fileName}");
                
                if (response)
                {
                    return Json(new { success = true, message = "Xóa ảnh thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xóa ảnh" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {FileName}", fileName);
                return Json(new { success = false, message = "Lỗi khi xóa ảnh: " + ex.Message });
            }
        }

        // GET: Admin/ImageGallery/Upload
        public IActionResult Upload()
        {
            return View();
        }
    }
}
