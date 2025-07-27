using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Web.Helpers;

namespace BookStore.Web.Controllers
{
    public class ReviewsController : BaseController
    {
        private readonly ApiService _apiService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(ApiService apiService, ILogger<ReviewsController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Reviews/MyReviews
        public async Task<IActionResult> MyReviews()
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }

                var reviews = await _apiService.GetAsync<List<ReviewDto>>($"reviews/user/{userId.Value}");
                var reviewViewModels = new List<ReviewViewModel>();

                if (reviews != null && reviews.Any())
                {
                    // Get book information for each review
                    foreach (var review in reviews)
                    {
                        var reviewViewModel = MapToViewModel(review);

                        // Get book details
                        try
                        {
                            var book = await _apiService.GetAsync<BookDto>($"books/{review.BookId}");
                            if (book != null)
                            {
                                reviewViewModel.BookImageUrl = book.ImageUrl ?? "/images/no-image.jpg";
                                reviewViewModel.BookPrice = book.Price;
                                reviewViewModel.BookAuthor = book.AuthorName;
                                reviewViewModel.BookCategory = book.CategoryName;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not load book details for BookId: {BookId}", review.BookId);
                            // Set default values if book loading fails
                            reviewViewModel.BookImageUrl = "/images/no-image.jpg";
                            reviewViewModel.BookAuthor = "Unknown Author";
                            reviewViewModel.BookCategory = "Unknown Category";
                        }

                        reviewViewModels.Add(reviewViewModel);
                    }
                }

                return View(reviewViewModels);
            }
            catch (UnauthorizedAccessException)
            {
                return HandleUnauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user reviews");
                TempData["Error"] = "Không thể tải danh sách đánh giá của bạn.";
                return View(new List<ReviewViewModel>());
            }
        }

        // GET: Reviews/Book/5
        [AllowAnonymous]
        public async Task<IActionResult> Book(int id)
        {
            try
            {
                var reviews = await _apiService.GetAsync<List<ReviewDto>>($"reviews/book/{id}");
                var book = await _apiService.GetAsync<BookDto>($"books/{id}");

                if (book == null)
                {
                    return NotFound();
                }

                var viewModel = new BookReviewsViewModel
                {
                    Book = MapBookToViewModel(book),
                    Reviews = reviews?.Where(r => r.Status == ReviewStatus.Approved)
                                    .Select(MapToViewModel).ToList() ?? new List<ReviewViewModel>(),
                    CanUserReview = false // Will be set based on user login and purchase history
                };

                // Check if user can review (logged in and hasn't reviewed yet)
                if (IsUserLoggedIn())
                {
                    var userId = HttpContext.Session.GetInt32("UserId");
                    if (userId.HasValue)
                    {
                        var userReviews = await _apiService.GetAsync<List<ReviewDto>>($"reviews/user/{userId.Value}");
                        viewModel.CanUserReview = userReviews?.Any(r => r.BookId == id) != true;
                        
                        // Check if user has existing review for this book
                        viewModel.UserReview = userReviews?.FirstOrDefault(r => r.BookId == id);
                        if (viewModel.UserReview != null)
                        {
                            viewModel.UserReviewViewModel = MapToViewModel(viewModel.UserReview);
                        }
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book reviews for book {BookId}", id);
                TempData["Error"] = "Không thể tải danh sách đánh giá.";
                return RedirectToAction("Details", "Shop", new { id });
            }
        }

        // GET: Reviews/Create?bookId=5
        public async Task<IActionResult> Create(int bookId)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                var book = await _apiService.GetAsync<BookDto>($"books/{bookId}");
                if (book == null)
                {
                    return NotFound();
                }

                // Check if user has already reviewed this book
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    var userReviews = await _apiService.GetAsync<List<ReviewDto>>($"reviews/user/{userId.Value}");
                    if (userReviews?.Any(r => r.BookId == bookId) == true)
                    {
                        TempData["Warning"] = "Bạn đã đánh giá sách này rồi.";
                        return RedirectToAction("Book", new { id = bookId });
                    }
                }

                var viewModel = new CreateReviewViewModel
                {
                    BookId = bookId,
                    BookTitle = book.Title,
                    BookImageUrl = book.ImageUrl
                };

                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return HandleUnauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create review page for book {BookId}", bookId);
                TempData["Error"] = "Không thể tải trang đánh giá.";
                return RedirectToAction("Details", "Shop", new { id = bookId });
            }
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    TempData["Warning"] = "Vui lòng đăng nhập để gửi đánh giá.";
                    return RedirectToAction("Login", "Account");
                }

                if (!ModelState.IsValid)
                {
                    // Reload book info for display
                    var book = await _apiService.GetAsync<BookDto>($"books/{model.BookId}");
                    if (book != null)
                    {
                        model.BookTitle = book.Title;
                        model.BookImageUrl = book.ImageUrl;
                    }
                    return View(model);
                }

                var createDto = new CreateReviewDto
                {
                    BookId = model.BookId,
                    Rating = model.Rating,
                    Comment = model.Comment
                };

                _logger.LogInformation("Creating review for book {BookId} by user {UserId}", model.BookId, HttpContext.Session.GetInt32("UserId"));

                var createdReview = await _apiService.PostAsync<ReviewDto>("reviews", createDto);
                if (createdReview != null)
                {
                    TempData["Success"] = "Đánh giá của bạn đã được gửi và đang chờ duyệt!";
                    return RedirectToAction("Details", "Shop", new { id = model.BookId });
                }
                else
                {
                    TempData["Error"] = "Không thể gửi đánh giá. Vui lòng kiểm tra kết nối và thử lại.";
                    return View(model);
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Warning"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when creating review for book {BookId}", model.BookId);

                if (ex.Message.Contains("401"))
                {
                    TempData["Warning"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Login", "Account");
                }
                else if (ex.Message.Contains("400"))
                {
                    if (ex.Message.Contains("Bạn đã đánh giá sách này rồi"))
                    {
                        TempData["Warning"] = "Bạn đã đánh giá sách này rồi.";
                        return RedirectToAction("Details", "Shop", new { id = model.BookId });
                    }
                    else
                    {
                        TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin.";
                    }
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi gửi đánh giá. Vui lòng thử lại sau.";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when creating review for book {BookId}", model.BookId);
                TempData["Error"] = "Có lỗi không mong muốn xảy ra. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        // GET: Reviews/DebugSession - For debugging authentication issues
        [HttpGet]
        public IActionResult DebugSession()
        {
            var token = HttpContext.Session.GetString("Token");
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("Username");

            return Json(new
            {
                HasToken = !string.IsNullOrEmpty(token),
                TokenLength = token?.Length ?? 0,
                UserId = userId,
                Username = username,
                IsLoggedIn = IsUserLoggedIn(),
                SessionId = HttpContext.Session.Id,
                TokenPreview = !string.IsNullOrEmpty(token) ? token.Substring(0, Math.Min(20, token.Length)) + "..." : null
            });
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                var review = await _apiService.GetAsync<ReviewDto>($"reviews/{id}");
                if (review == null)
                {
                    return NotFound();
                }

                // Check if user owns this review
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue || review.UserId != userId.Value)
                {
                    return Forbid();
                }

                var book = await _apiService.GetAsync<BookDto>($"books/{review.BookId}");
                
                var viewModel = new EditReviewViewModel
                {
                    Id = review.Id,
                    BookId = review.BookId,
                    BookTitle = book?.Title ?? review.BookTitle,
                    BookImageUrl = book?.ImageUrl ?? "",
                    Rating = review.Rating,
                    Comment = review.Comment
                };

                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return HandleUnauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit review page for review {ReviewId}", id);
                TempData["Error"] = "Không thể tải trang chỉnh sửa đánh giá.";
                return RedirectToAction("MyReviews");
            }
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditReviewViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var updateDto = new UpdateReviewDto
                {
                    Rating = model.Rating,
                    Comment = model.Comment
                };

                var updatedReview = await _apiService.PutAsync<ReviewDto>($"reviews/{id}", updateDto);
                if (updatedReview != null)
                {
                    TempData["Success"] = "Đánh giá đã được cập nhật và đang chờ duyệt lại!";
                    return RedirectToAction("MyReviews");
                }
                else
                {
                    TempData["Error"] = "Không thể cập nhật đánh giá. Vui lòng thử lại.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return HandleUnauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật đánh giá.";
            }

            return View(model);
        }

        // POST: Reviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                var result = await _apiService.DeleteAsync($"reviews/{id}");
                if (result)
                {
                    TempData["Success"] = "Đánh giá đã được xóa!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa đánh giá.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return HandleUnauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa đánh giá.";
            }

            return RedirectToAction("MyReviews");
        }

        // POST: Reviews/MarkHelpful/5
        [HttpPost]
        public async Task<IActionResult> MarkHelpful(int id, bool isHelpful)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá." });
                }

                var result = await _apiService.PostAsync<object>($"reviews/{id}/helpful", isHelpful);
                if (result != null)
                {
                    return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể ghi nhận đánh giá." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking review {ReviewId} as helpful", id);
                return Json(new { success = false, message = "Có lỗi xảy ra." });
            }
        }

        #region Helper Methods

        private static ReviewViewModel MapToViewModel(ReviewDto dto)
        {
            return new ReviewViewModel
            {
                Id = dto.Id,
                BookId = dto.BookId,
                BookTitle = dto.BookTitle,
                BookImageUrl = "", // Will be populated separately if needed
                BookPrice = 0, // Will be populated separately if needed
                BookAuthor = "", // Will be populated separately if needed
                BookCategory = "", // Will be populated separately if needed
                UserId = dto.UserId,
                UserName = dto.UserName,
                UserFullName = dto.UserFullName,
                UserAvatarUrl = "", // Will be populated separately if needed
                Rating = dto.Rating,
                Comment = dto.Comment,
                Status = dto.Status,
                AdminNote = dto.AdminNote,
                ReviewedByAdminId = dto.ReviewedByAdminId,
                ReviewedByAdminName = dto.ReviewedByAdminName,
                ReviewedAt = dto.ReviewedAt,
                IsVerifiedPurchase = dto.IsVerifiedPurchase,
                OrderId = dto.OrderId,
                HelpfulCount = dto.HelpfulCount,
                NotHelpfulCount = dto.NotHelpfulCount,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        private BookViewModel MapBookToViewModel(BookDto book)
        {
            return new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description ?? "",
                Price = book.Price,

                // Discount fields
                DiscountPercentage = book.DiscountPercentage ?? 0,
                DiscountAmount = book.DiscountAmount,
                IsOnSale = book.IsOnSale,
                SaleStartDate = book.SaleStartDate,
                SaleEndDate = book.SaleEndDate,

                ImageUrl = book.ImageUrl ?? "/images/no-image.jpg",
                AuthorName = book.AuthorName ?? "Unknown Author",
                CategoryName = book.CategoryName ?? "Unknown Category",
                CategoryId = book.CategoryId,
                AuthorId = book.AuthorId,
                ISBN = book.ISBN ?? "",
                Publisher = book.Publisher ?? "",
                PublicationYear = book.PublicationYear ?? 0,
                Quantity = book.Quantity,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt
            };
        }

        #endregion
    }
}
