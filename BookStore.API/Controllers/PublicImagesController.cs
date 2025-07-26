using Microsoft.AspNetCore.Mvc;

namespace BookStore.API.Controllers
{
    [Route("uploads")]
    [ApiController]
    public class PublicImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PublicImagesController> _logger;
        
        // Allowed image file extensions
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public PublicImagesController(IWebHostEnvironment environment, ILogger<PublicImagesController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Serve book images publicly (no authentication required)
        /// </summary>
        /// <param name="filename">Image filename</param>
        /// <returns>Image file</returns>
        [HttpGet("books/{filename}")]
        public IActionResult GetBookImage(string filename)
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

                // Check file extension
                var extension = Path.GetExtension(filename).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    return BadRequest("Invalid file type");
                }

                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "books");
                var filePath = Path.Combine(uploadPath, filename);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"Book image not found: {filename}");
                    return NotFound();
                }

                var imageBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(extension);

                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error serving book image: {filename}");
                return StatusCode(500, "Error loading image");
            }
        }

        /// <summary>
        /// Serve slider images publicly (no authentication required)
        /// </summary>
        /// <param name="filename">Image filename</param>
        /// <returns>Image file</returns>
        [HttpGet("sliders/{filename}")]
        public IActionResult GetSliderImage(string filename)
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

                // Check file extension
                var extension = Path.GetExtension(filename).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    return BadRequest("Invalid file type");
                }

                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "sliders");
                var filePath = Path.Combine(uploadPath, filename);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"Slider image not found: {filename}");
                    return NotFound();
                }

                var imageBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(extension);

                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error serving slider image: {filename}");
                return StatusCode(500, "Error loading image");
            }
        }

        /// <summary>
        /// Serve banner images publicly (no authentication required)
        /// </summary>
        /// <param name="filename">Image filename</param>
        /// <returns>Image file</returns>
        [HttpGet("banners/{filename}")]
        public IActionResult GetBannerImage(string filename)
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

                // Check file extension
                var extension = Path.GetExtension(filename).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    return BadRequest("Invalid file type");
                }

                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "banners");
                var filePath = Path.Combine(uploadPath, filename);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"Banner image not found: {filename}");
                    return NotFound();
                }

                var imageBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(extension);

                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error serving banner image: {filename}");
                return StatusCode(500, "Error loading image");
            }
        }

        /// <summary>
        /// Generic public image serving for other categories
        /// </summary>
        /// <param name="category">Image category (books, sliders, banners, authors, etc.)</param>
        /// <param name="filename">Image filename</param>
        /// <returns>Image file</returns>
        [HttpGet("{category}/{filename}")]
        public IActionResult GetImage(string category, string filename)
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
                var allowedCategories = new[] { "books", "sliders", "banners", "authors", "categories" };
                if (!allowedCategories.Contains(category.ToLower()))
                {
                    return BadRequest("Invalid category");
                }

                // Check file extension
                var extension = Path.GetExtension(filename).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    return BadRequest("Invalid file type");
                }

                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", category.ToLower());
                var filePath = Path.Combine(uploadPath, filename);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"Image not found: {category}/{filename}");
                    return NotFound();
                }

                var imageBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(extension);

                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error serving image: {category}/{filename}");
                return StatusCode(500, "Error loading image");
            }
        }

        private string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
        }
    }
}
