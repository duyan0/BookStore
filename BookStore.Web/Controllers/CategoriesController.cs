using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Web.Attributes;
using BookStore.Core.DTOs;

namespace BookStore.Web.Controllers
{
    [AdminOnly]
    public class CategoriesController : BaseController
    {
        private readonly ApiService _apiService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ApiService apiService, ILogger<CategoriesController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _apiService.GetAsync<List<CategoryDto>>("categories");
                var categoryViewModels = categories?.Select(MapToViewModel).ToList() ?? new List<CategoryViewModel>();
                return View(categoryViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                TempData["Error"] = "Không thể tải danh sách thể loại. Vui lòng thử lại sau.";
                return View(new List<CategoryViewModel>());
            }
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var category = await _apiService.GetAsync<CategoryDto>($"categories/{id}");
                if (category == null)
                {
                    return NotFound();
                }

                var categoryViewModel = MapToViewModel(category);
                return View(categoryViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category details for ID: {CategoryId}", id);
                TempData["Error"] = "Không thể tải thông tin thể loại.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var createDto = new CreateCategoryDto
                {
                    Name = model.Name,
                    Description = model.Description ?? string.Empty
                };

                var createdCategory = await _apiService.PostAsync<CategoryDto>("categories", createDto);
                if (createdCategory != null)
                {
                    TempData["Success"] = "Tạo thể loại thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Không thể tạo thể loại. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                TempData["Error"] = "Có lỗi xảy ra khi tạo thể loại.";
            }

            return View(model);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _apiService.GetAsync<CategoryDto>($"categories/{id}");
                if (category == null)
                {
                    return NotFound();
                }

                var categoryViewModel = MapToViewModel(category);
                return View(categoryViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category for edit. ID: {CategoryId}", id);
                TempData["Error"] = "Không thể tải thông tin thể loại để chỉnh sửa.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var updateDto = new UpdateCategoryDto
                {
                    Name = model.Name,
                    Description = model.Description ?? string.Empty
                };

                var updatedCategory = await _apiService.PutAsync<CategoryDto>($"categories/{id}", updateDto);
                if (updatedCategory != null)
                {
                    TempData["Success"] = "Cập nhật thể loại thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Không thể cập nhật thể loại. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category. ID: {CategoryId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật thể loại.";
            }

            return View(model);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _apiService.GetAsync<CategoryDto>($"categories/{id}");
                if (category == null)
                {
                    return NotFound();
                }

                var categoryViewModel = MapToViewModel(category);
                return View(categoryViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category for delete. ID: {CategoryId}", id);
                TempData["Error"] = "Không thể tải thông tin thể loại.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"categories/{id}");
                if (success)
                {
                    TempData["Success"] = "Xóa thể loại thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa thể loại. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category. ID: {CategoryId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa thể loại.";
            }

            return RedirectToAction(nameof(Index));
        }

        private static CategoryViewModel MapToViewModel(CategoryDto dto)
        {
            return new CategoryViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                BookCount = dto.BookCount,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
}
