using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookStore.Core.DTOs;
using BookStore.Core.Interfaces;

namespace BookStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannersController : ControllerBase
    {
        private readonly IBannerService _bannerService;
        private readonly ILogger<BannersController> _logger;

        public BannersController(IBannerService bannerService, ILogger<BannersController> logger)
        {
            _bannerService = bannerService;
            _logger = logger;
        }

        // GET: api/Banners
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BannerDto>>> GetBanners()
        {
            try
            {
                var banners = await _bannerService.GetAllBannersAsync();
                return Ok(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving banners");
                return StatusCode(500, new { message = "Error retrieving banners" });
            }
        }

        // GET: api/Banners/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BannerDto>>> GetActiveBanners()
        {
            try
            {
                var banners = await _bannerService.GetActiveBannersAsync();
                return Ok(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active banners");
                return StatusCode(500, new { message = "Error retrieving active banners" });
            }
        }

        // GET: api/Banners/position/{position}
        [HttpGet("position/{position}")]
        public async Task<ActionResult<IEnumerable<BannerDto>>> GetBannersByPosition(string position)
        {
            try
            {
                var banners = await _bannerService.GetActiveBannersByPositionAsync(position);
                return Ok(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving banners for position {Position}", position);
                return StatusCode(500, new { message = "Error retrieving banners" });
            }
        }

        // GET: api/Banners/debug/all
        [HttpGet("debug/all")]
        public async Task<ActionResult<IEnumerable<BannerDto>>> GetAllBannersDebug()
        {
            try
            {
                var banners = await _bannerService.GetAllBannersAsync();
                return Ok(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all banners for debug");
                return StatusCode(500, new { message = "Error retrieving banners" });
            }
        }

        // GET: api/Banners/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BannerDto>> GetBanner(int id)
        {
            try
            {
                var banner = await _bannerService.GetBannerByIdAsync(id);
                if (banner == null)
                {
                    return NotFound(new { message = "Banner not found" });
                }
                return Ok(banner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving banner {BannerId}", id);
                return StatusCode(500, new { message = "Error retrieving banner" });
            }
        }

        // POST: api/Banners
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BannerDto>> CreateBanner(CreateBannerDto createBannerDto)
        {
            try
            {
                var banner = await _bannerService.CreateBannerAsync(createBannerDto);
                return CreatedAtAction(nameof(GetBanner), new { id = banner.Id }, banner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating banner");
                return StatusCode(500, new { message = "Error creating banner" });
            }
        }

        // PUT: api/Banners/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBanner(int id, UpdateBannerDto updateBannerDto)
        {
            try
            {
                var banner = await _bannerService.UpdateBannerAsync(id, updateBannerDto);
                if (banner == null)
                {
                    return NotFound(new { message = "Banner not found" });
                }
                return Ok(banner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner {BannerId}", id);
                return StatusCode(500, new { message = "Error updating banner" });
            }
        }

        // DELETE: api/Banners/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            try
            {
                var result = await _bannerService.DeleteBannerAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Banner not found" });
                }
                return Ok(new { message = "Banner deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting banner {BannerId}", id);
                return StatusCode(500, new { message = "Error deleting banner" });
            }
        }

        // PUT: api/Banners/5/toggle-active
        [HttpPut("{id}/toggle-active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            try
            {
                var result = await _bannerService.ToggleActiveStatusAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Banner not found" });
                }
                return Ok(new { message = "Banner status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling banner status {BannerId}", id);
                return StatusCode(500, new { message = "Error updating banner status" });
            }
        }

        // PUT: api/Banners/5/display-order
        [HttpPut("{id}/display-order")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDisplayOrder(int id, [FromBody] int newOrder)
        {
            try
            {
                var result = await _bannerService.UpdateDisplayOrderAsync(id, newOrder);
                if (!result)
                {
                    return NotFound(new { message = "Banner not found" });
                }
                return Ok(new { message = "Display order updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner display order {BannerId}", id);
                return StatusCode(500, new { message = "Error updating display order" });
            }
        }
    }
}
