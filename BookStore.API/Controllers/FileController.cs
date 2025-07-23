using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileController> _logger;

        public FileController(IWebHostEnvironment environment, ILogger<FileController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload()
        {
            try
            {
                var formCollection = await Request.ReadFormAsync();
                var files = formCollection.Files;

                if (files.Count == 0)
                {
                    return BadRequest("Không có file nào được gửi lên");
                }

                var uploadResults = new List<object>();
                var uploadPath = Path.Combine(_environment.ContentRootPath, "Uploads");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Tạo tên file an toàn
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var uniqueFileName = GetUniqueFileName(fileName);
                        var fullPath = Path.Combine(uploadPath, uniqueFileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        uploadResults.Add(new
                        {
                            fileName = uniqueFileName,
                            originalFileName = fileName,
                            size = file.Length,
                            contentType = file.ContentType,
                            url = $"/api/file/download/{uniqueFileName}"
                        });

                        _logger.LogInformation($"File uploaded: {uniqueFileName}, Size: {file.Length} bytes");
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Upload thành công",
                    files = uploadResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Upload error: {ex.Message}");
                return StatusCode(500, $"Lỗi khi upload file: {ex.Message}");
            }
        }

        [HttpPost("upload/anonymous")]
        public async Task<IActionResult> UploadAnonymous()
        {
            try
            {
                var formCollection = await Request.ReadFormAsync();
                var files = formCollection.Files;

                if (files.Count == 0)
                {
                    return BadRequest("Không có file nào được gửi lên");
                }

                var uploadResults = new List<object>();
                var uploadPath = Path.Combine(_environment.ContentRootPath, "Uploads", "Anonymous");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Tạo tên file an toàn
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var uniqueFileName = GetUniqueFileName(fileName);
                        var fullPath = Path.Combine(uploadPath, uniqueFileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        uploadResults.Add(new
                        {
                            fileName = uniqueFileName,
                            originalFileName = fileName,
                            size = file.Length,
                            contentType = file.ContentType,
                            url = $"/api/file/download/anonymous/{uniqueFileName}"
                        });

                        _logger.LogInformation($"Anonymous file uploaded: {uniqueFileName}, Size: {file.Length} bytes");
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Upload thành công",
                    files = uploadResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Upload error: {ex.Message}");
                return StatusCode(500, $"Lỗi khi upload file: {ex.Message}");
            }
        }

        [HttpGet("download/{fileName}")]
        [Authorize]
        public IActionResult Download(string fileName)
        {
            try
            {
                var uploadPath = Path.Combine(_environment.ContentRootPath, "Uploads");
                var filePath = Path.Combine(uploadPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File không tồn tại");
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;

                return File(memory, GetContentType(filePath), fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Download error: {ex.Message}");
                return StatusCode(500, $"Lỗi khi tải file: {ex.Message}");
            }
        }

        [HttpGet("download/anonymous/{fileName}")]
        public IActionResult DownloadAnonymous(string fileName)
        {
            try
            {
                var uploadPath = Path.Combine(_environment.ContentRootPath, "Uploads", "Anonymous");
                var filePath = Path.Combine(uploadPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File không tồn tại");
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;

                return File(memory, GetContentType(filePath), fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Download error: {ex.Message}");
                return StatusCode(500, $"Lỗi khi tải file: {ex.Message}");
            }
        }

        private string GetUniqueFileName(string fileName)
        {
            // Tạo tên file duy nhất bằng cách thêm timestamp
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            return $"{fileNameWithoutExtension}_{timestamp}{extension}";
        }

        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
            {
                { ".txt", "text/plain" },
                { ".pdf", "application/pdf" },
                { ".doc", "application/vnd.ms-word" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".csv", "text/csv" }
            };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }
    }
} 