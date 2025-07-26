using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;

namespace BookStore.Web.Controllers
{
    [Route("uploads")]
    public class ImageController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(ApiService apiService, ILogger<ImageController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        /// <summary>
        /// Proxy images from the API server to the Web application
        /// This allows the Web app to serve images that are stored on the API server
        /// </summary>
        /// <param name="category">Image category (e.g., "books")</param>
        /// <param name="filename">Image filename</param>
        /// <returns>Image file</returns>
        [HttpGet("books/{filename}")]
        public async Task<IActionResult> GetBookImage(string filename)
        {
            try
            {
                if (string.IsNullOrEmpty(filename))
                {
                    return NotFound();
                }

                // Validate filename to prevent directory traversal attacks
                if (filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
                {
                    return BadRequest("Invalid filename");
                }

                // Get the image from the API server
                var apiUrl = $"http://localhost:5274/uploads/books/{filename}";
                
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Image not found on API server: {filename}");
                    return NotFound();
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error serving image: {filename}");
                return StatusCode(500, "Error loading image");
            }
        }

        /// <summary>
        /// Generic image proxy for other categories if needed in the future
        /// </summary>
        /// <param name="category">Image category</param>
        /// <param name="filename">Image filename</param>
        /// <returns>Image file</returns>
        [HttpGet("{category}/{filename}")]
        public async Task<IActionResult> GetImage(string category, string filename)
        {
            try
            {
                if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(filename))
                {
                    return NotFound();
                }

                // Validate inputs to prevent directory traversal attacks
                if (category.Contains("..") || category.Contains("/") || category.Contains("\\") ||
                    filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
                {
                    return BadRequest("Invalid parameters");
                }

                // Only allow specific categories for security
                var allowedCategories = new[] { "books", "authors", "categories", "sliders", "banners" };
                if (!allowedCategories.Contains(category.ToLower()))
                {
                    return BadRequest("Invalid category");
                }

                // Get the image from the API server
                var apiUrl = $"http://localhost:5274/uploads/{category}/{filename}";
                
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Image not found on API server: {category}/{filename}");
                    return NotFound();
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error serving image: {category}/{filename}");
                return StatusCode(500, "Error loading image");
            }
        }

        /// <summary>
        /// Serve slider images publicly
        /// </summary>
        /// <param name="filename">Image filename</param>
        /// <returns>Image file</returns>
        [HttpGet("sliders/{filename}")]
        public async Task<IActionResult> GetSliderImage(string filename)
        {
            try
            {
                if (string.IsNullOrEmpty(filename))
                {
                    return NotFound();
                }

                // Validate filename to prevent directory traversal attacks
                if (filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
                {
                    return BadRequest("Invalid filename");
                }

                // Get the image from the API server
                var apiUrl = $"http://localhost:5274/uploads/sliders/{filename}";

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Slider image not found on API server: {filename}");
                    return NotFound();
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error serving slider image: {filename}");
                return StatusCode(500, "Error loading image");
            }
        }

        /// <summary>
        /// Serve banner images publicly
        /// </summary>
        /// <param name="filename">Image filename</param>
        /// <returns>Image file</returns>
        [HttpGet("banners/{filename}")]
        public async Task<IActionResult> GetBannerImage(string filename)
        {
            try
            {
                if (string.IsNullOrEmpty(filename))
                {
                    return NotFound();
                }

                // Validate filename to prevent directory traversal attacks
                if (filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
                {
                    return BadRequest("Invalid filename");
                }

                // Get the image from the API server
                var apiUrl = $"http://localhost:5274/uploads/banners/{filename}";

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Banner image not found on API server: {filename}");
                    return NotFound();
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error serving banner image: {filename}");
                return StatusCode(500, "Error loading image");
            }
        }
    }
}
