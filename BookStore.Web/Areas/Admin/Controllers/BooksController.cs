using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Core.DTOs;
using BookStore.Web.Attributes;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class BooksController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(ApiService apiService, ILogger<BooksController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Admin/Books
        public async Task<IActionResult> Index()
        {
            try
            {
                var books = await _apiService.GetAsync<List<BookDto>>("books");
                var bookViewModels = books?.Select(MapToViewModel).ToList() ?? new List<BookViewModel>();
                return View(bookViewModels);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching books");
                TempData["Error"] = "Không thể tải danh sách sách. Vui lòng thử lại sau.";
                return View(new List<BookViewModel>());
            }
        }

        // GET: Admin/Books/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var book = await _apiService.GetAsync<BookDto>($"books/{id}");
                if (book == null)
                {
                    TempData["Error"] = "Không tìm thấy sách.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = MapToViewModel(book);
                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching book details for ID: {BookId}", id);
                TempData["Error"] = "Không thể tải thông tin sách. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Books/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                await LoadSelectLists();
                return View(new BookViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create book form");
                TempData["Error"] = "Không thể tải form tạo sách. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(model);
            }

            try
            {
                // Ensure ImageUrl is never null before sending to API
                if (model.ImageUrl == null)
                {
                    model.ImageUrl = string.Empty;
                }

                var createDto = MapToCreateDto(model);
                var createdBook = await _apiService.PostAsync<BookDto>("books", createDto);
                
                if (createdBook != null)
                {
                    TempData["Success"] = "Tạo sách thành công!";
                    return RedirectToAction(nameof(Details), new { id = createdBook.Id });
                }
                else
                {
                    TempData["Error"] = "Không thể tạo sách. Vui lòng thử lại.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book");
                TempData["Error"] = "Có lỗi xảy ra khi tạo sách. Vui lòng thử lại sau.";
            }

            await LoadSelectLists();
            return View(model);
        }

        // GET: Admin/Books/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var book = await _apiService.GetAsync<BookDto>($"books/{id}");
                if (book == null)
                {
                    TempData["Error"] = "Không tìm thấy sách.";
                    return RedirectToAction(nameof(Index));
                }

                await LoadSelectLists();
                var viewModel = MapToViewModel(book);
                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for book ID: {BookId}", id);
                TempData["Error"] = "Không thể tải form chỉnh sửa. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookViewModel model)
        {
            if (id != model.Id)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(model);
            }

            try
            {
                // Ensure ImageUrl is never null before sending to API
                if (model.ImageUrl == null)
                {
                    model.ImageUrl = string.Empty;
                }

                var updateDto = MapToUpdateDto(model);
                var updatedBook = await _apiService.PutAsync<BookDto>($"books/{id}", updateDto);

                if (updatedBook != null)
                {
                    TempData["Success"] = "Cập nhật sách thành công!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["Error"] = "Không thể cập nhật sách. Vui lòng thử lại.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book ID: {BookId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật sách. Vui lòng thử lại sau.";
            }

            await LoadSelectLists();
            return View(model);
        }

        // POST: Admin/Books/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"books/{id}");
                
                if (success)
                {
                    TempData["Success"] = "Xóa sách thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa sách. Vui lòng thử lại.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book ID: {BookId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa sách. Vui lòng thử lại sau.";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods

        private async Task LoadSelectLists()
        {
            try
            {
                var authors = await _apiService.GetAsync<List<AuthorDto>>("authors");
                var categories = await _apiService.GetAsync<List<CategoryDto>>("categories");

                ViewBag.AuthorId = new SelectList(authors ?? new List<AuthorDto>(), "Id", "FullName");
                ViewBag.CategoryId = new SelectList(categories ?? new List<CategoryDto>(), "Id", "Name");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading select lists");
                ViewBag.AuthorId = new SelectList(new List<AuthorDto>(), "Id", "FullName");
                ViewBag.CategoryId = new SelectList(new List<CategoryDto>(), "Id", "Name");
            }
        }

        private static BookViewModel MapToViewModel(BookDto dto)
        {
            return new BookViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Quantity = dto.Quantity,
                ISBN = dto.ISBN,
                Publisher = dto.Publisher,
                PublicationYear = dto.PublicationYear ?? 0,
                ImageUrl = dto.ImageUrl,
                AuthorId = dto.AuthorId,
                CategoryId = dto.CategoryId,
                AuthorName = dto.AuthorName,
                CategoryName = dto.CategoryName,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        private static CreateBookDto MapToCreateDto(BookViewModel model)
        {
            return new CreateBookDto
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                Quantity = model.Quantity,
                ISBN = model.ISBN ?? string.Empty,
                Publisher = model.Publisher ?? string.Empty,
                PublicationYear = model.PublicationYear,
                ImageUrl = model.ImageUrl ?? string.Empty,
                AuthorId = model.AuthorId,
                CategoryId = model.CategoryId
            };
        }

        private static UpdateBookDto MapToUpdateDto(BookViewModel model)
        {
            return new UpdateBookDto
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                Quantity = model.Quantity,
                ISBN = model.ISBN ?? string.Empty,
                Publisher = model.Publisher ?? string.Empty,
                PublicationYear = model.PublicationYear,
                ImageUrl = model.ImageUrl ?? string.Empty,
                AuthorId = model.AuthorId,
                CategoryId = model.CategoryId
            };
        }

        #endregion
    }
}
