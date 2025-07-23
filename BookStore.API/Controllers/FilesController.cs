using BookStore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Upload một file
        /// </summary>
        /// <param name="file">File cần upload</param>
        /// <param name="folderName">Thư mục con để lưu file (không bắt buộc)</param>
        /// <returns>Đường dẫn tương đối của file đã upload</returns>
        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string folderName = "")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Không có file nào được chọn");
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Định dạng file không được hỗ trợ");
                }

                // Kiểm tra kích thước file (tối đa 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest("Kích thước file không được vượt quá 10MB");
                }

                var filePath = await _fileService.UploadFileAsync(file, folderName);
                return Ok(new { filePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi upload file: {ex.Message}");
            }
        }

        /// <summary>
        /// Upload nhiều file
        /// </summary>
        /// <param name="files">Danh sách file cần upload</param>
        /// <param name="folderName">Thư mục con để lưu file (không bắt buộc)</param>
        /// <returns>Danh sách đường dẫn tương đối của các file đã upload</returns>
        [HttpPost("upload-multiple")]
        [Authorize]
        public async Task<IActionResult> UploadMultipleFiles([FromForm] List<IFormFile> files, [FromQuery] string folderName = "")
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return BadRequest("Không có file nào được chọn");
                }

                // Kiểm tra định dạng và kích thước của tất cả các file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
                
                foreach (var file in files)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest($"Định dạng file {file.FileName} không được hỗ trợ");
                    }

                    if (file.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest($"Kích thước file {file.FileName} không được vượt quá 10MB");
                    }
                }

                var filePaths = await _fileService.UploadFilesAsync(files, folderName);
                return Ok(new { filePaths });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi upload file: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa một file
        /// </summary>
        /// <param name="filePath">Đường dẫn tương đối của file cần xóa</param>
        /// <returns>Kết quả xóa file</returns>
        [HttpDelete("delete")]
        [Authorize]
        public async Task<IActionResult> DeleteFile([FromQuery] string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return BadRequest("Đường dẫn file không được để trống");
                }

                var result = await _fileService.DeleteFileAsync(filePath);
                
                if (result)
                {
                    return Ok(new { success = true, message = "Xóa file thành công" });
                }
                else
                {
                    return NotFound(new { success = false, message = "Không tìm thấy file hoặc không thể xóa" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi xóa file: {ex.Message}");
            }
        }
    }
} 