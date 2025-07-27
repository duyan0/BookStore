using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookStore.Core.DTOs;
using BookStore.Core.Interfaces;
using BookStore.Core.Entities;

namespace BookStore.API.Controllers
{
    [Route("api/help")]
    [ApiController]
    public class HelpArticlesController : ControllerBase
    {
        private readonly IHelpArticleService _helpArticleService;
        private readonly ILogger<HelpArticlesController> _logger;

        public HelpArticlesController(IHelpArticleService helpArticleService, ILogger<HelpArticlesController> logger)
        {
            _helpArticleService = helpArticleService;
            _logger = logger;
        }

        // GET: api/help
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HelpArticleListDto>>> GetArticles()
        {
            try
            {
                var articles = await _helpArticleService.GetAllArticlesAsync();
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving help articles");
                return StatusCode(500, new { message = "Error retrieving articles" });
            }
        }

        // GET: api/help/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HelpArticleDto>> GetArticle(int id)
        {
            try
            {
                var article = await _helpArticleService.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                return Ok(article);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving help article {ArticleId}", id);
                return StatusCode(500, new { message = "Error retrieving article" });
            }
        }

        // GET: api/help/article/slug-name
        [HttpGet("article/{slug}")]
        public async Task<ActionResult<HelpArticleDto>> GetArticleBySlug(string slug)
        {
            try
            {
                var article = await _helpArticleService.GetArticleBySlugAsync(slug);
                if (article == null)
                {
                    return NotFound();
                }

                return Ok(article);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving help article by slug {Slug}", slug);
                return StatusCode(500, new { message = "Error retrieving article" });
            }
        }

        // GET: api/help/category/general
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<HelpArticleListDto>>> GetArticlesByCategory(string category)
        {
            try
            {
                if (!Enum.TryParse<HelpArticleCategory>(category, true, out var categoryEnum))
                {
                    return BadRequest(new { message = "Invalid category" });
                }

                var articles = await _helpArticleService.GetArticlesByCategoryAsync(categoryEnum);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving articles by category {Category}", category);
                return StatusCode(500, new { message = "Error retrieving articles" });
            }
        }

        // GET: api/help/type/faq
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<HelpArticleListDto>>> GetArticlesByType(string type)
        {
            try
            {
                if (!Enum.TryParse<HelpArticleType>(type, true, out var typeEnum))
                {
                    return BadRequest(new { message = "Invalid type" });
                }

                var articles = await _helpArticleService.GetArticlesByTypeAsync(typeEnum);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving articles by type {Type}", type);
                return StatusCode(500, new { message = "Error retrieving articles" });
            }
        }

        // GET: api/help/featured
        [HttpGet("featured")]
        public async Task<ActionResult<IEnumerable<HelpArticleListDto>>> GetFeaturedArticles()
        {
            try
            {
                var articles = await _helpArticleService.GetFeaturedArticlesAsync();
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving featured articles");
                return StatusCode(500, new { message = "Error retrieving articles" });
            }
        }

        // GET: api/help/recent?count=10
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<HelpArticleListDto>>> GetRecentArticles([FromQuery] int count = 10)
        {
            try
            {
                var articles = await _helpArticleService.GetRecentArticlesAsync(count);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent articles");
                return StatusCode(500, new { message = "Error retrieving articles" });
            }
        }

        // GET: api/help/home
        [HttpGet("home")]
        public async Task<ActionResult<HelpCenterHomeDto>> GetHelpCenterHome()
        {
            try
            {
                var homeData = await _helpArticleService.GetHelpCenterHomeDataAsync();
                return Ok(homeData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving help center home data");
                return StatusCode(500, new { message = "Error retrieving home data" });
            }
        }

        // POST: api/help/search
        [HttpPost("search")]
        public async Task<ActionResult<HelpSearchResultDto>> SearchArticles(HelpSearchDto searchDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var results = await _helpArticleService.SearchArticlesAsync(searchDto);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching help articles");
                return StatusCode(500, new { message = "Error searching articles" });
            }
        }

        // POST: api/help
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HelpArticleDto>> CreateArticle(CreateHelpArticleDto createArticleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Set author ID from current user
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    // Note: This would need to be set in the service or here
                    // createArticleDto.AuthorId = userId;
                }

                var createdArticle = await _helpArticleService.CreateArticleAsync(createArticleDto);
                return CreatedAtAction(nameof(GetArticle), new { id = createdArticle.Id }, createdArticle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating help article");
                return StatusCode(500, new { message = "Error creating article" });
            }
        }

        // PUT: api/help/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HelpArticleDto>> UpdateArticle(int id, UpdateHelpArticleDto updateArticleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedArticle = await _helpArticleService.UpdateArticleAsync(id, updateArticleDto);
                if (updatedArticle == null)
                {
                    return NotFound();
                }

                return Ok(updatedArticle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating help article {ArticleId}", id);
                return StatusCode(500, new { message = "Error updating article" });
            }
        }

        // DELETE: api/help/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            try
            {
                var result = await _helpArticleService.DeleteArticleAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting help article {ArticleId}", id);
                return StatusCode(500, new { message = "Error deleting article" });
            }
        }

        // POST: api/help/5/toggle-publish
        [HttpPost("{id}/toggle-publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TogglePublishStatus(int id)
        {
            try
            {
                var result = await _helpArticleService.TogglePublishStatusAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { message = "Publish status toggled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling publish status for article {ArticleId}", id);
                return StatusCode(500, new { message = "Error updating article" });
            }
        }

        // POST: api/help/5/toggle-featured
        [HttpPost("{id}/toggle-featured")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleFeaturedStatus(int id)
        {
            try
            {
                var result = await _helpArticleService.ToggleFeaturedStatusAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { message = "Featured status toggled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling featured status for article {ArticleId}", id);
                return StatusCode(500, new { message = "Error updating article" });
            }
        }

        // PUT: api/help/5/display-order
        [HttpPut("{id}/display-order")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDisplayOrder(int id, [FromBody] int newOrder)
        {
            try
            {
                var result = await _helpArticleService.UpdateDisplayOrderAsync(id, newOrder);
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { message = "Display order updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating display order for article {ArticleId}", id);
                return StatusCode(500, new { message = "Error updating article" });
            }
        }

        // POST: api/help/5/view
        [HttpPost("{id}/view")]
        public async Task<IActionResult> IncrementViewCount(int id)
        {
            try
            {
                var result = await _helpArticleService.IncrementViewCountAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { message = "View count incremented" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing view count for article {ArticleId}", id);
                return StatusCode(500, new { message = "Error updating view count" });
            }
        }
    }
}
