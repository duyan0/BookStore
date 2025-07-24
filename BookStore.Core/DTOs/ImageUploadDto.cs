namespace BookStore.Core.DTOs
{
    public class ImageUploadResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ImageUploadDataDto? Data { get; set; }
    }

    public class ImageUploadDataDto
    {
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string FullUrl { get; set; } = string.Empty;
    }

    public class ImageListResponseDto
    {
        public bool Success { get; set; }
        public List<ImageFileDto> Data { get; set; } = new List<ImageFileDto>();
    }

    public class ImageFileDto
    {
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string FullUrl { get; set; } = string.Empty;
        public string SizeFormatted => FormatFileSize(Size);

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
