using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Web.Attributes;
using BookStore.Core.DTOs;

namespace BookStore.Web.Controllers
{
    [AdminOnly]
    public class AuthorsController : BaseController
    {
        private readonly ApiService _apiService;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(ApiService apiService, ILogger<AuthorsController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Authors
        public async Task<IActionResult> Index()
        {
            try
            {
                var authors = await _apiService.GetAsync<List<AuthorDto>>("authors");
                var authorViewModels = authors?.Select(MapToViewModel).ToList() ?? new List<AuthorViewModel>();
                return View(authorViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching authors");
                TempData["Error"] = "Không thể tải danh sách tác giả. Vui lòng thử lại sau.";
                return View(new List<AuthorViewModel>());
            }
        }

        // GET: Authors/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var author = await _apiService.GetAsync<AuthorDto>($"authors/{id}");
                if (author == null)
                {
                    return NotFound();
                }

                var authorViewModel = MapToViewModel(author);
                return View(authorViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching author details for ID: {AuthorId}", id);
                TempData["Error"] = "Không thể tải thông tin tác giả.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Authors/Create
        public IActionResult Create()
        {
            return View(new AuthorViewModel());
        }

        // POST: Authors/Create
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
                var createDto = new CreateAuthorDto
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Biography = model.Biography ?? string.Empty
                };

                var createdAuthor = await _apiService.PostAsync<AuthorDto>("authors", createDto);
                if (createdAuthor != null)
                {
                    TempData["Success"] = "Tạo tác giả thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Không thể tạo tác giả. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating author");
                TempData["Error"] = "Có lỗi xảy ra khi tạo tác giả.";
            }

            return View(model);
        }

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var author = await _apiService.GetAsync<AuthorDto>($"authors/{id}");
                if (author == null)
                {
                    return NotFound();
                }

                var authorViewModel = MapToViewModel(author);
                return View(authorViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching author for edit. ID: {AuthorId}", id);
                TempData["Error"] = "Không thể tải thông tin tác giả để chỉnh sửa.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AuthorViewModel model)
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
                var updateDto = new UpdateAuthorDto
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Biography = model.Biography ?? string.Empty
                };

                var updatedAuthor = await _apiService.PutAsync<AuthorDto>($"authors/{id}", updateDto);
                if (updatedAuthor != null)
                {
                    TempData["Success"] = "Cập nhật tác giả thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Không thể cập nhật tác giả. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating author. ID: {AuthorId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật tác giả.";
            }

            return View(model);
        }

        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var author = await _apiService.GetAsync<AuthorDto>($"authors/{id}");
                if (author == null)
                {
                    return NotFound();
                }

                var authorViewModel = MapToViewModel(author);
                return View(authorViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching author for delete. ID: {AuthorId}", id);
                TempData["Error"] = "Không thể tải thông tin tác giả.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting author. ID: {AuthorId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa tác giả.";
            }

            return RedirectToAction(nameof(Index));
        }

        private static AuthorViewModel MapToViewModel(AuthorDto dto)
        {
            return new AuthorViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Biography = dto.Biography,
                BookCount = dto.BookCount,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
}
