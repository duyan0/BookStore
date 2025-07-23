using BookStore.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly string _uploadsFolder;

        public FileService(IHostingEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            // Đảm bảo thư mục uploads tồn tại
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName = "")
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Không có file được chọn");
            }

            // Tạo tên file duy nhất để tránh trùng lặp
            string uniqueFileName = GetUniqueFileName(file.FileName);

            // Tạo đường dẫn đầy đủ đến thư mục lưu trữ
            string targetFolder = _uploadsFolder;
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                targetFolder = Path.Combine(targetFolder, folderName);
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }
            }

            // Đường dẫn đầy đủ đến file
            string filePath = Path.Combine(targetFolder, uniqueFileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn tương đối
            string relativePath = Path.Combine("uploads", folderName, uniqueFileName).Replace("\\", "/");
            return relativePath;
        }

        public async Task<IEnumerable<string>> UploadFilesAsync(IEnumerable<IFormFile> files, string folderName = "")
        {
            if (files == null || !files.Any())
            {
                throw new ArgumentException("Không có file nào được chọn");
            }

            var uploadedFiles = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string relativePath = await UploadFileAsync(file, folderName);
                    uploadedFiles.Add(relativePath);
                }
            }

            return uploadedFiles;
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return Task.FromResult(false);
            }

            try
            {
                // Xây dựng đường dẫn đầy đủ
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));

                // Kiểm tra file có tồn tại không
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
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
    }
} 