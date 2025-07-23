using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Core.DTOs;
using BookStore.Web.Attributes;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class AuthorsController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(ApiService apiService, ILogger<AuthorsController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Admin/Authors
        public async Task<IActionResult> Index()
        {
            try
            {
                var authors = await _apiService.GetAsync<List<AuthorDto>>("authors");
                var authorViewModels = authors?.Select(MapToViewModel).ToList() ?? new List<AuthorViewModel>();
                return View(authorViewModels);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching authors");
                TempData["Error"] = "Không thể tải danh sách tác giả. Vui lòng thử lại sau.";
                return View(new List<AuthorViewModel>());
            }
        }

        // GET: Admin/Authors/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var author = await _apiService.GetAsync<AuthorDto>($"authors/{id}");
                if (author == null)
                {
                    TempData["Error"] = "Không tìm thấy tác giả.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = MapToViewModel(author);
                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching author details for ID: {AuthorId}", id);
                TempData["Error"] = "Không thể tải thông tin tác giả. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Authors/Create
        public IActionResult Create()
        {
            return View(new AuthorViewModel());
        }

        // POST: Admin/Authors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuthorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var createDto = MapToCreateDto(model);
                var createdAuthor = await _apiService.PostAsync<AuthorDto>("authors", createDto);
                
                if (createdAuthor != null)
                {
                    TempData["Success"] = "Tạo tác giả thành công!";
                    return RedirectToAction(nameof(Details), new { id = createdAuthor.Id });
                }
                else
                {
                    TempData["Error"] = "Không thể tạo tác giả. Vui lòng thử lại.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating author");
                TempData["Error"] = "Có lỗi xảy ra khi tạo tác giả. Vui lòng thử lại sau.";
            }

            return View(model);
        }

        // GET: Admin/Authors/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var author = await _apiService.GetAsync<AuthorDto>($"authors/{id}");
                if (author == null)
                {
                    TempData["Error"] = "Không tìm thấy tác giả.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = MapToViewModel(author);
                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for author ID: {AuthorId}", id);
                TempData["Error"] = "Không thể tải form chỉnh sửa. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AuthorViewModel model)
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
                var success = await _apiService.PutAsync<bool>($"authors/{id}", updateDto);
                
                if (success)
                {
                    TempData["Success"] = "Cập nhật tác giả thành công!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["Error"] = "Không thể cập nhật tác giả. Vui lòng thử lại.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating author ID: {AuthorId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật tác giả. Vui lòng thử lại sau.";
            }

            return View(model);
        }

        // POST: Admin/Authors/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"authors/{id}");
                
                if (success)
                {
                    TempData["Success"] = "Xóa tác giả thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa tác giả. Vui lòng thử lại.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting author ID: {AuthorId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa tác giả. Vui lòng thử lại sau.";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods

        private static AuthorViewModel MapToViewModel(AuthorDto dto)
        {
            return new AuthorViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Biography = dto.Biography,
                BookCount = 0, // Will be populated from API if needed
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        private static CreateAuthorDto MapToCreateDto(AuthorViewModel model)
        {
            return new CreateAuthorDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Biography = model.Biography
            };
        }

        private static UpdateAuthorDto MapToUpdateDto(AuthorViewModel model)
        {
            return new UpdateAuthorDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Biography = model.Biography
            };
        }

        #endregion
    }
}
