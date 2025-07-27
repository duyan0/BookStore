using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Web.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class ReviewsController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(ApiService apiService, ILogger<ReviewsController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Admin/Reviews
        public async Task<IActionResult> Index()
        {
            try
            {
                var reviews = await _apiService.GetAsync<List<ReviewDto>>("reviews");
                var reviewViewModels = reviews?.Select(MapToViewModel).ToList() ?? new List<ReviewViewModel>();
                
                return View(reviewViewModels);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reviews");
                TempData["Error"] = "Không thể tải danh sách đánh giá. Vui lòng thử lại sau.";
                return View(new List<ReviewViewModel>());
            }
        }

        // GET: Admin/Reviews/Pending
        public async Task<IActionResult> Pending()
        {
            try
            {
                var reviews = await _apiService.GetAsync<List<ReviewDto>>("reviews/pending");
                var reviewViewModels = reviews?.Select(MapToViewModel).ToList() ?? new List<ReviewViewModel>();
                
                return View(reviewViewModels);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending reviews");
                TempData["Error"] = "Không thể tải danh sách đánh giá chờ duyệt.";
                return View(new List<ReviewViewModel>());
            }
        }

        // GET: Admin/Reviews/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var review = await _apiService.GetAsync<ReviewDto>($"reviews/{id}");
                if (review == null)
                {
                    return NotFound();
                }

                var reviewViewModel = MapToViewModel(review);
                reviewViewModel = await EnhanceReviewViewModel(reviewViewModel);
                return View(reviewViewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching review details for review {ReviewId}", id);
                TempData["Error"] = "Không thể tải chi tiết đánh giá.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Reviews/Moderate/5
        public async Task<IActionResult> Moderate(int id)
        {
            try
            {
                var review = await _apiService.GetAsync<ReviewDto>($"reviews/{id}");
                if (review == null)
                {
                    return NotFound();
                }

                var viewModel = new ModerateReviewViewModel
                {
                    ReviewId = review.Id,
                    BookTitle = review.BookTitle,
                    UserName = review.UserFullName,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt,
                    IsVerifiedPurchase = review.IsVerifiedPurchase,
                    CurrentStatus = review.Status,
                    CurrentAdminNote = review.AdminNote
                };

                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading moderate review page for review {ReviewId}", id);
                TempData["Error"] = "Không thể tải trang duyệt đánh giá.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Reviews/Moderate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Moderate(int id, ModerateReviewViewModel model)
        {
            try
            {
                if (id != model.ReviewId)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var moderateDto = new ModerateReviewDto
                {
                    Status = model.Status,
                    AdminNote = model.AdminNote
                };

                var moderatedReview = await _apiService.PostAsync<ReviewDto>($"reviews/{id}/moderate", moderateDto);
                if (moderatedReview != null)
                {
                    string successMessage = moderateDto.Status switch
                    {
                        ReviewStatus.Approved => "Đánh giá đã được duyệt và công khai thành công!",
                        ReviewStatus.Rejected => "Đánh giá đã được từ chối thành công!",
                        ReviewStatus.Hidden => "Đánh giá đã được ẩn thành công!",
                        _ => "Đánh giá đã được cập nhật thành công!"
                    };

                    TempData["Success"] = successMessage;
                    return RedirectToAction(nameof(Pending));
                }
                else
                {
                    TempData["Error"] = "Không thể xử lý đánh giá. Vui lòng kiểm tra kết nối và thử lại.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating review {ReviewId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi duyệt đánh giá.";
            }

            return View(model);
        }

        // POST: Admin/Reviews/QuickModerate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickModerate(int reviewId, string action, string adminNote = "")
        {
            try
            {
                ReviewStatus status;
                string successMessage;

                switch (action.ToLower())
                {
                    case "approve":
                        status = ReviewStatus.Approved;
                        successMessage = "Đánh giá đã được duyệt thành công!";
                        adminNote = string.IsNullOrEmpty(adminNote) ? "Đánh giá hợp lệ, được duyệt tự động." : adminNote;
                        break;
                    case "reject":
                        status = ReviewStatus.Rejected;
                        successMessage = "Đánh giá đã được từ chối!";
                        adminNote = string.IsNullOrEmpty(adminNote) ? "Đánh giá không phù hợp." : adminNote;
                        break;
                    default:
                        TempData["Error"] = "Hành động không hợp lệ.";
                        return RedirectToAction(nameof(Pending));
                }

                var moderateDto = new ModerateReviewDto
                {
                    Status = status,
                    AdminNote = adminNote
                };

                var moderatedReview = await _apiService.PostAsync<ReviewDto>($"reviews/{reviewId}/moderate", moderateDto);
                if (moderatedReview != null)
                {
                    TempData["Success"] = successMessage;
                }
                else
                {
                    TempData["Error"] = "Không thể xử lý đánh giá. Vui lòng thử lại.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in quick moderate for review {ReviewId}", reviewId);
                TempData["Error"] = "Có lỗi xảy ra khi xử lý đánh giá.";
            }

            return RedirectToAction(nameof(Pending));
        }

        // POST: Admin/Reviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
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
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa đánh giá.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Reviews/Statistics
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var statistics = await _apiService.GetAsync<ReviewStatisticsDto>("reviews/statistics");
                if (statistics == null)
                {
                    TempData["Error"] = "Không thể tải thống kê đánh giá.";
                    return RedirectToAction(nameof(Index));
                }

                return View(statistics);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading review statistics");
                TempData["Error"] = "Không thể tải thống kê đánh giá.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Reviews/ByStatus?status=approved
        public async Task<IActionResult> ByStatus(string status)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                {
                    return RedirectToAction(nameof(Index));
                }

                var reviews = await _apiService.GetAsync<List<ReviewDto>>($"reviews/status/{status}");
                var reviewViewModels = reviews?.Select(MapToViewModel).ToList() ?? new List<ReviewViewModel>();
                
                ViewBag.Status = status;
                ViewBag.StatusDisplayName = GetStatusDisplayName(status);
                
                return View("Index", reviewViewModels);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reviews by status {Status}", status);
                TempData["Error"] = $"Không thể tải danh sách đánh giá {GetStatusDisplayName(status)}.";
                return RedirectToAction(nameof(Index));
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

        private async Task<ReviewViewModel> EnhanceReviewViewModel(ReviewViewModel viewModel)
        {
            try
            {
                // Get book information
                var book = await _apiService.GetAsync<BookDto>($"books/{viewModel.BookId}");
                if (book != null)
                {
                    viewModel.BookImageUrl = book.ImageUrl;
                    viewModel.BookPrice = book.Price;
                    viewModel.BookAuthor = book.AuthorName;
                    viewModel.BookCategory = book.CategoryName;
                }

                // Note: UserAvatarUrl would need to be fetched from User API if available
                // For now, we'll leave it empty as there's no User API endpoint that returns avatar
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enhance review view model for review {ReviewId}", viewModel.Id);
                // Continue with basic view model if enhancement fails
            }

            return viewModel;
        }

        private static string GetStatusDisplayName(string status)
        {
            return status.ToLower() switch
            {
                "pending" => "chờ duyệt",
                "approved" => "đã duyệt",
                "rejected" => "bị từ chối",
                "hidden" => "đã ẩn",
                _ => status
            };
        }

        #endregion
    }
}
