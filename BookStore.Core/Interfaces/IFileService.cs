using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Core.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Upload một file và trả về đường dẫn tương đối của file
        /// </summary>
        /// <param name="file">File cần upload</param>
        /// <param name="folderName">Thư mục con trong uploads để lưu file (ví dụ: books, authors)</param>
        /// <returns>Đường dẫn tương đối của file đã upload</returns>
        Task<string> UploadFileAsync(IFormFile file, string folderName = "");

        /// <summary>
        /// Upload nhiều file và trả về danh sách đường dẫn tương đối
        /// </summary>
        /// <param name="files">Danh sách file cần upload</param>
        /// <param name="folderName">Thư mục con trong uploads để lưu file</param>
        /// <returns>Danh sách đường dẫn tương đối của các file đã upload</returns>
        Task<IEnumerable<string>> UploadFilesAsync(IEnumerable<IFormFile> files, string folderName = "");

        /// <summary>
        /// Xóa một file dựa trên đường dẫn tương đối
        /// </summary>
        /// <param name="filePath">Đường dẫn tương đối của file cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không</returns>
        Task<bool> DeleteFileAsync(string filePath);
    }
} 