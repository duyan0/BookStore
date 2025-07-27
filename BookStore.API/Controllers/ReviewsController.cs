using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookStore.Core.DTOs;
using BookStore.Core.Interfaces;
using BookStore.Core.Entities;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        // GET: api/Reviews
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews()
        {
            try
            {
                var reviews = await _reviewService.GetAllReviewsAsync();
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews");
                return StatusCode(500, new { message = "Error retrieving reviews" });
            }
        }

        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetReview(int id)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }

                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review {ReviewId}", id);
                return StatusCode(500, new { message = "Error retrieving review" });
            }
        }

        // GET: api/Reviews/book/5
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByBook(int bookId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByBookIdAsync(bookId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for book {BookId}", bookId);
                return StatusCode(500, new { message = "Error retrieving reviews" });
            }
        }

        // GET: api/Reviews/user/5
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByUser(int userId)
        {
            try
            {
                // Users can only view their own reviews, admins can view all
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");
                
                if (!isAdmin && userId.ToString() != currentUserId)
                {
                    return Forbid();
                }

                var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving reviews" });
            }
        }

        // GET: api/Reviews/pending
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetPendingReviews()
        {
            try
            {
                var reviews = await _reviewService.GetPendingReviewsAsync();
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending reviews");
                return StatusCode(500, new { message = "Error retrieving pending reviews" });
            }
        }

        // GET: api/Reviews/status/approved
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByStatus(string status)
        {
            try
            {
                if (!Enum.TryParse<ReviewStatus>(status, true, out var reviewStatus))
                {
                    return BadRequest(new { message = "Invalid status" });
                }

                var reviews = await _reviewService.GetReviewsByStatusAsync(reviewStatus);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews by status {Status}", status);
                return StatusCode(500, new { message = "Error retrieving reviews" });
            }
        }

        // GET: api/Reviews/statistics
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ReviewStatisticsDto>> GetReviewStatistics()
        {
            try
            {
                var statistics = await _reviewService.GetReviewStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review statistics");
                return StatusCode(500, new { message = "Error retrieving statistics" });
            }
        }

        // GET: api/Reviews/book/5/statistics
        [HttpGet("book/{bookId}/statistics")]
        public async Task<ActionResult<ReviewStatisticsDto>> GetBookReviewStatistics(int bookId)
        {
            try
            {
                var statistics = await _reviewService.GetBookReviewStatisticsAsync(bookId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review statistics for book {BookId}", bookId);
                return StatusCode(500, new { message = "Error retrieving statistics" });
            }
        }

        // POST: api/Reviews
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewDto>> CreateReview(CreateReviewDto createReviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get current user ID from token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                createReviewDto.UserId = userId;

                var createdReview = await _reviewService.CreateReviewAsync(createReviewDto);
                return CreatedAtAction(nameof(GetReview), new { id = createdReview.Id }, createdReview);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, new { message = "Error creating review" });
            }
        }

        // PUT: api/Reviews/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ReviewDto>> UpdateReview(int id, UpdateReviewDto updateReviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if user owns this review or is admin
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }

                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");
                
                if (!isAdmin && review.UserId.ToString() != currentUserId)
                {
                    return Forbid();
                }

                var updatedReview = await _reviewService.UpdateReviewAsync(id, updateReviewDto);
                if (updatedReview == null)
                {
                    return NotFound();
                }

                return Ok(updatedReview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", id);
                return StatusCode(500, new { message = "Error updating review" });
            }
        }

        // DELETE: api/Reviews/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                // Check if user owns this review or is admin
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }

                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");
                
                if (!isAdmin && review.UserId.ToString() != currentUserId)
                {
                    return Forbid();
                }

                var result = await _reviewService.DeleteReviewAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", id);
                return StatusCode(500, new { message = "Error deleting review" });
            }
        }

        // POST: api/Reviews/5/moderate
        [HttpPost("{id}/moderate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ReviewDto>> ModerateReview(int id, ModerateReviewDto moderateReviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var moderatedReview = await _reviewService.ModerateReviewAsync(id, moderateReviewDto);
                if (moderatedReview == null)
                {
                    return NotFound();
                }

                return Ok(moderatedReview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating review {ReviewId}", id);
                return StatusCode(500, new { message = "Error moderating review" });
            }
        }

        // POST: api/Reviews/5/helpful
        [HttpPost("{id}/helpful")]
        [Authorize]
        public async Task<IActionResult> MarkReviewHelpful(int id, [FromBody] bool isHelpful)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _reviewService.MarkReviewHelpfulAsync(id, userId, isHelpful);
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { message = "Vote recorded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking review {ReviewId} as helpful", id);
                return StatusCode(500, new { message = "Error recording vote" });
            }
        }

        // DELETE: api/Reviews/5/helpful
        [HttpDelete("{id}/helpful")]
        [Authorize]
        public async Task<IActionResult> RemoveHelpfulnessVote(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _reviewService.RemoveHelpfulnessVoteAsync(id, userId);
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { message = "Vote removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing helpfulness vote for review {ReviewId}", id);
                return StatusCode(500, new { message = "Error removing vote" });
            }
        }
    }
}
