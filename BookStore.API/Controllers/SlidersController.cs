using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookStore.Core.DTOs;
using BookStore.Core.Interfaces;

namespace BookStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlidersController : ControllerBase
    {
        private readonly ISliderService _sliderService;
        private readonly ILogger<SlidersController> _logger;

        public SlidersController(ISliderService sliderService, ILogger<SlidersController> logger)
        {
            _sliderService = sliderService;
            _logger = logger;
        }

        // GET: api/Sliders
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<SliderDto>>> GetSliders()
        {
            try
            {
                var sliders = await _sliderService.GetAllSlidersAsync();
                return Ok(sliders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sliders");
                return StatusCode(500, new { message = "Error retrieving sliders" });
            }
        }

        // GET: api/Sliders/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<SliderDto>>> GetActiveSliders()
        {
            try
            {
                var sliders = await _sliderService.GetActiveSlidersAsync();
                return Ok(sliders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sliders");
                return StatusCode(500, new { message = "Error retrieving active sliders" });
            }
        }

        // GET: api/Sliders/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SliderDto>> GetSlider(int id)
        {
            try
            {
                var slider = await _sliderService.GetSliderByIdAsync(id);
                if (slider == null)
                {
                    return NotFound(new { message = "Slider not found" });
                }
                return Ok(slider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving slider {SliderId}", id);
                return StatusCode(500, new { message = "Error retrieving slider" });
            }
        }

        // POST: api/Sliders
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SliderDto>> CreateSlider(CreateSliderDto createSliderDto)
        {
            try
            {
                var slider = await _sliderService.CreateSliderAsync(createSliderDto);
                return CreatedAtAction(nameof(GetSlider), new { id = slider.Id }, slider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating slider");
                return StatusCode(500, new { message = "Error creating slider" });
            }
        }

        // PUT: api/Sliders/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSlider(int id, UpdateSliderDto updateSliderDto)
        {
            try
            {
                var slider = await _sliderService.UpdateSliderAsync(id, updateSliderDto);
                if (slider == null)
                {
                    return NotFound(new { message = "Slider not found" });
                }
                return Ok(slider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating slider {SliderId}", id);
                return StatusCode(500, new { message = "Error updating slider" });
            }
        }

        // DELETE: api/Sliders/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSlider(int id)
        {
            try
            {
                var result = await _sliderService.DeleteSliderAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Slider not found" });
                }
                return Ok(new { message = "Slider deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting slider {SliderId}", id);
                return StatusCode(500, new { message = "Error deleting slider" });
            }
        }

        // PUT: api/Sliders/5/toggle-active
        [HttpPut("{id}/toggle-active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            try
            {
                var result = await _sliderService.ToggleActiveStatusAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Slider not found" });
                }
                return Ok(new { message = "Slider status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling slider status {SliderId}", id);
                return StatusCode(500, new { message = "Error updating slider status" });
            }
        }

        // PUT: api/Sliders/5/display-order
        [HttpPut("{id}/display-order")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDisplayOrder(int id, [FromBody] int newOrder)
        {
            try
            {
                var result = await _sliderService.UpdateDisplayOrderAsync(id, newOrder);
                if (!result)
                {
                    return NotFound(new { message = "Slider not found" });
                }
                return Ok(new { message = "Display order updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating slider display order {SliderId}", id);
                return StatusCode(500, new { message = "Error updating display order" });
            }
        }
    }
}
