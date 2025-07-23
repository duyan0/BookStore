using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Core.DTOs;
using BookStore.Web.Attributes;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class CategoriesController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ApiService apiService, ILogger<CategoriesController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _apiService.GetAsync<List<CategoryDto>>("categories");
                var categoryViewModels = categories?.Select(MapToViewModel).ToList() ?? new List<CategoryViewModel>();
                return View(categoryViewModels);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                TempData["Error"] = "Không thể tải danh sách danh mục. Vui lòng thử lại sau.";
                return View(new List<CategoryViewModel>());
            }
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var category = await _apiService.GetAsync<CategoryDto>($"categories/{id}");
                if (category == null)
                {
                    TempData["Error"] = "Không tìm thấy danh mục.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = MapToViewModel(category);
                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category details for ID: {CategoryId}", id);
                TempData["Error"] = "Không thể tải thông tin danh mục. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        // POST: Admin/Categories/Create
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
                var createDto = MapToCreateDto(model);
                var createdCategory = await _apiService.PostAsync<CategoryDto>("categories", createDto);
                
                if (createdCategory != null)
                {
                    TempData["Success"] = "Tạo danh mục thành công!";
                    return RedirectToAction(nameof(Details), new { id = createdCategory.Id });
                }
                else
                {
                    TempData["Error"] = "Không thể tạo danh mục. Vui lòng thử lại.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                TempData["Error"] = "Có lỗi xảy ra khi tạo danh mục. Vui lòng thử lại sau.";
            }

            return View(model);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _apiService.GetAsync<CategoryDto>($"categories/{id}");
                if (category == null)
                {
                    TempData["Error"] = "Không tìm thấy danh mục.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = MapToViewModel(category);
                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for category ID: {CategoryId}", id);
                TempData["Error"] = "Không thể tải form chỉnh sửa. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (id != model.Id)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var updateDto = MapToUpdateDto(model);
                var success = await _apiService.PutAsync<bool>($"categories/{id}", updateDto);
                
                if (success)
                {
                    TempData["Success"] = "Cập nhật danh mục thành công!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["Error"] = "Không thể cập nhật danh mục. Vui lòng thử lại.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category ID: {CategoryId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật danh mục. Vui lòng thử lại sau.";
            }

            return View(model);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"categories/{id}");
                
                if (success)
                {
                    TempData["Success"] = "Xóa danh mục thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa danh mục. Vui lòng thử lại.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category ID: {CategoryId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa danh mục. Vui lòng thử lại sau.";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods

        private static CategoryViewModel MapToViewModel(CategoryDto dto)
        {
            return new CategoryViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                BookCount = 0, // Will be populated from API if needed
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        private static CreateCategoryDto MapToCreateDto(CategoryViewModel model)
        {
            return new CreateCategoryDto
            {
                Name = model.Name,
                Description = model.Description
            };
        }

        private static UpdateCategoryDto MapToUpdateDto(CategoryViewModel model)
        {
            return new UpdateCategoryDto
            {
                Name = model.Name,
                Description = model.Description
            };
        }

        #endregion
    }
}
