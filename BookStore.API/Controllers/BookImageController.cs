using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class BookImageController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<BookImageController> _logger;
        
        // Allowed image file extensions
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        
        // Maximum file size: 5MB
        private readonly long _maxFileSize = 5 * 1024 * 1024;

        public BookImageController(IWebHostEnvironment environment, ILogger<BookImageController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

      
        [HttpPost("upload")]
        public async Task<IActionResult> UploadBookImage()
        {
            try
            {
                var formCollection = await Request.ReadFormAsync();
                var files = formCollection.Files;

                if (files.Count == 0)
                {
                    return BadRequest(new { success = false, message = "Không có file nào được gửi lên" });
                }

                if (files.Count > 1)
                {
                    return BadRequest(new { success = false, message = "Chỉ được upload một file ảnh" });
                }

                var file = files[0];

                // Validate file
                var validationResult = ValidateImageFile(file);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, message = validationResult.ErrorMessage });
                }

                // Create upload directory
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "books");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Generate unique filename
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName?.Trim('"');
                var uniqueFileName = GenerateUniqueFileName(fileName);
                var fullPath = Path.Combine(uploadPath, uniqueFileName);

                // Save file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Generate URL for accessing the image
                var imageUrl = $"/uploads/books/{uniqueFileName}";

                _logger.LogInformation($"Book image uploaded successfully: {uniqueFileName}, Size: {file.Length} bytes");

                return Ok(new
                {
                    success = true,
                    message = "Upload ảnh thành công",
                    data = new
                    {
                        fileName = uniqueFileName,
                        originalFileName = fileName,
                        size = file.Length,
                        contentType = file.ContentType,
                        imageUrl = imageUrl,
                        fullUrl = $"{Request.Scheme}://{Request.Host}{imageUrl}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading book image");
                return StatusCode(500, new { success = false, message = "Lỗi khi upload ảnh: " + ex.Message });
            }
        }

       
        [HttpDelete("{fileName}")]
        public IActionResult DeleteBookImage(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest(new { success = false, message = "Tên file không hợp lệ" });
                }

                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "books");
                var filePath = Path.Combine(uploadPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { success = false, message = "File không tồn tại" });
                }

                // Delete file
                System.IO.File.Delete(filePath);

                _logger.LogInformation($"Book image deleted successfully: {fileName}");

                return Ok(new { success = true, message = "Xóa ảnh thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book image: {FileName}", fileName);
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa ảnh: " + ex.Message });
            }
        }

        /// <summary>
        /// Get list of uploaded book images
        /// </summary>
        /// <returns>List of image files</returns>
        [HttpGet("list")]
        public IActionResult GetBookImages()
        {
            try
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "books");
                
                if (!Directory.Exists(uploadPath))
                {
                    return Ok(new { success = true, data = new List<object>() });
                }

                var files = Directory.GetFiles(uploadPath)
                    .Where(f => _allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .Select(f => new
                    {
                        fileName = Path.GetFileName(f),
                        size = new FileInfo(f).Length,
                        lastModified = new FileInfo(f).LastWriteTime,
                        imageUrl = $"/uploads/books/{Path.GetFileName(f)}",
                        fullUrl = $"{Request.Scheme}://{Request.Host}/uploads/books/{Path.GetFileName(f)}"
                    })
                    .OrderByDescending(f => f.lastModified)
                    .ToList();

                return Ok(new { success = true, data = files });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting book images list");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách ảnh: " + ex.Message });
            }
        }

        private (bool IsValid, string ErrorMessage) ValidateImageFile(IFormFile file)
        {
            // Check file size
            if (file.Length == 0)
            {
                return (false, "File rỗng");
            }

            if (file.Length > _maxFileSize)
            {
                return (false, $"File quá lớn. Kích thước tối đa: {_maxFileSize / (1024 * 1024)}MB");
            }

            // Check file extension
            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName?.Trim('"');
            if (string.IsNullOrEmpty(fileName))
            {
                return (false, "Tên file không hợp lệ");
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return (false, $"Định dạng file không được hỗ trợ. Chỉ chấp nhận: {string.Join(", ", _allowedExtensions)}");
            }

            // Check content type
            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return (false, "Loại file không được hỗ trợ. Chỉ chấp nhận file ảnh");
            }

            return (true, string.Empty);
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            if (string.IsNullOrEmpty(originalFileName))
            {
                originalFileName = "image.jpg";
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var extension = Path.GetExtension(originalFileName);
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var uniqueId = Guid.NewGuid().ToString("N")[..8]; // First 8 characters of GUID
            
            // Clean filename - remove special characters
            fileNameWithoutExtension = System.Text.RegularExpressions.Regex.Replace(fileNameWithoutExtension, @"[^a-zA-Z0-9_-]", "");
            
            return $"{fileNameWithoutExtension}_{timestamp}_{uniqueId}{extension}";
        }
    }
}
