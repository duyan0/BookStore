using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Web.Attributes;
using BookStore.Core.DTOs;

namespace BookStore.Web.Controllers
{
    [AdminOnly]
    public class BooksController : BaseController
    {
        private readonly ApiService _apiService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(ApiService apiService, ILogger<BooksController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Books
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
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching books");
                TempData["Error"] = "Không thể tải danh sách sách. Vui lòng thử lại sau.";
                return View(new List<BookViewModel>());
            }
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var book = await _apiService.GetAsync<BookDto>($"books/{id}");
                if (book == null)
                {
                    return NotFound();
                }

                var bookViewModel = MapToViewModel(book);
                return View(bookViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching book details for ID: {BookId}", id);
                TempData["Error"] = "Không thể tải thông tin sách.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Books/Create
        public async Task<IActionResult> Create()
        {
            await LoadSelectLists();
            return View(new BookViewModel());
        }

        // POST: Books/Create
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
                var createDto = new CreateBookDto
                {
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    Quantity = model.StockQuantity,
                    ISBN = model.ISBN ?? string.Empty,
                    Publisher = string.Empty, // Will be added to ViewModel later
                    PublicationYear = model.PublishedDate?.Year,
                    ImageUrl = model.ImageUrl ?? string.Empty,
                    CategoryId = model.CategoryId,
                    AuthorId = model.AuthorId
                };

                var createdBook = await _apiService.PostAsync<BookDto>("books", createDto);
                if (createdBook != null)
                {
                    TempData["Success"] = "Tạo sách thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Không thể tạo sách. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book");
                TempData["Error"] = "Có lỗi xảy ra khi tạo sách.";
            }

            await LoadSelectLists();
            return View(model);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var book = await _apiService.GetAsync<BookDto>($"books/{id}");
                if (book == null)
                {
                    return NotFound();
                }

                await LoadSelectLists();
                var bookViewModel = MapToViewModel(book);
                return View(bookViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching book for edit. ID: {BookId}", id);
                TempData["Error"] = "Không thể tải thông tin sách để chỉnh sửa.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(model);
            }

            try
            {
                var updateDto = new UpdateBookDto
                {
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    Quantity = model.StockQuantity,
                    ISBN = model.ISBN ?? string.Empty,
                    Publisher = string.Empty, // Will be added to ViewModel later
                    PublicationYear = model.PublishedDate?.Year,
                    ImageUrl = model.ImageUrl ?? string.Empty,
                    CategoryId = model.CategoryId,
                    AuthorId = model.AuthorId
                };

                var updatedBook = await _apiService.PutAsync<BookDto>($"books/{id}", updateDto);
                if (updatedBook != null)
                {
                    TempData["Success"] = "Cập nhật sách thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Không thể cập nhật sách. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book. ID: {BookId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật sách.";
            }

            await LoadSelectLists();
            return View(model);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var book = await _apiService.GetAsync<BookDto>($"books/{id}");
                if (book == null)
                {
                    return NotFound();
                }

                var bookViewModel = MapToViewModel(book);
                return View(bookViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching book for delete. ID: {BookId}", id);
                TempData["Error"] = "Không thể tải thông tin sách.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book. ID: {BookId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa sách.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Books/Search
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var books = await _apiService.GetAsync<List<BookDto>>($"books/search?term={Uri.EscapeDataString(searchTerm)}");
                var bookViewModels = books?.Select(MapToViewModel).ToList() ?? new List<BookViewModel>();
                
                ViewBag.SearchTerm = searchTerm;
                ViewBag.ResultCount = bookViewModels.Count;
                
                return View("Index", bookViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching books with term: {SearchTerm}", searchTerm);
                TempData["Error"] = "Không thể tìm kiếm sách. Vui lòng thử lại sau.";
                return View("Index", new List<BookViewModel>());
            }
        }

        private async Task LoadSelectLists()
        {
            try
            {
                var categories = await _apiService.GetAsync<List<CategoryDto>>("categories");
                var authors = await _apiService.GetAsync<List<AuthorDto>>("authors");

                ViewBag.Categories = categories?.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList() ?? new List<SelectListItem>();

                ViewBag.Authors = authors?.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.FullName
                }).ToList() ?? new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading select lists");
                ViewBag.Categories = new List<SelectListItem>();
                ViewBag.Authors = new List<SelectListItem>();
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
                StockQuantity = dto.Quantity,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName,
                AuthorId = dto.AuthorId,
                AuthorName = dto.AuthorName,
                ISBN = dto.ISBN,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
}
