using BookStore.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.DTOs
{
    public class HelpArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public HelpArticleType Type { get; set; }
        public HelpArticleCategory Category { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public int ViewCount { get; set; }
        public DateTime? LastViewedAt { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int? LastModifiedById { get; set; }
        public string? LastModifiedByName { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? RelatedArticleIds { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Computed properties
        public string TypeName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool IsRecentlyUpdated { get; set; }
        public string ContentPreview => Content.Length > 200 ? Content.Substring(0, 200) + "..." : Content;
    }

    public class CreateHelpArticleDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug là bắt buộc")]
        [StringLength(250, ErrorMessage = "Slug không được vượt quá 250 ký tự")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug chỉ được chứa chữ thường, số và dấu gạch ngang")]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Tóm tắt không được vượt quá 500 ký tự")]
        public string Summary { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại bài viết là bắt buộc")]
        public HelpArticleType Type { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public HelpArticleCategory Category { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải >= 0")]
        public int DisplayOrder { get; set; } = 0;

        public bool IsPublished { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        [StringLength(200, ErrorMessage = "Meta title không được vượt quá 200 ký tự")]
        public string? MetaTitle { get; set; }

        [StringLength(500, ErrorMessage = "Meta description không được vượt quá 500 ký tự")]
        public string? MetaDescription { get; set; }

        [StringLength(200, ErrorMessage = "Meta keywords không được vượt quá 200 ký tự")]
        public string? MetaKeywords { get; set; }

        public string? RelatedArticleIds { get; set; }
    }

    public class UpdateHelpArticleDto : CreateHelpArticleDto
    {
        // Inherits all properties from CreateHelpArticleDto
    }

    public class HelpArticleListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public HelpArticleType Type { get; set; }
        public HelpArticleCategory Category { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        // Computed properties
        public string TypeName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }

    public class HelpCenterHomeDto
    {
        public List<HelpArticleListDto> FeaturedArticles { get; set; } = new List<HelpArticleListDto>();
        public List<HelpArticleListDto> RecentArticles { get; set; } = new List<HelpArticleListDto>();
        public List<HelpCategoryStatsDto> Categories { get; set; } = new List<HelpCategoryStatsDto>();
        public List<HelpArticleListDto> PopularFAQs { get; set; } = new List<HelpArticleListDto>();
        public HelpStatisticsDto Statistics { get; set; } = new HelpStatisticsDto();
    }

    public class HelpCategoryStatsDto
    {
        public HelpArticleCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryDescription { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
        public string IconClass { get; set; } = string.Empty; // CSS icon class
    }

    public class HelpStatisticsDto
    {
        public int TotalArticles { get; set; }
        public int PublishedArticles { get; set; }
        public int DraftArticles { get; set; }
        public int FeaturedArticles { get; set; }
        public int TotalViews { get; set; }
        public int ViewsThisMonth { get; set; }
        public int ViewsThisWeek { get; set; }
        public DateTime LastUpdated { get; set; }

        // Article breakdown by type
        public int FAQCount { get; set; }
        public int PolicyCount { get; set; }
        public int GuideCount { get; set; }
        public int TutorialCount { get; set; }
        public int AnnouncementCount { get; set; }

        // Most viewed articles
        public List<HelpArticleListDto> MostViewedArticles { get; set; } = new List<HelpArticleListDto>();
    }

    public class HelpSearchDto
    {
        [Required(ErrorMessage = "Từ khóa tìm kiếm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Từ khóa không được vượt quá 100 ký tự")]
        public string Query { get; set; } = string.Empty;

        public HelpArticleType? Type { get; set; }
        public HelpArticleCategory? Category { get; set; }
        public bool? IsPublished { get; set; } = true;
    }

    public class HelpSearchResultDto
    {
        public List<HelpArticleListDto> Articles { get; set; } = new List<HelpArticleListDto>();
        public int TotalCount { get; set; }
        public string Query { get; set; } = string.Empty;
        public HelpArticleType? FilterType { get; set; }
        public HelpArticleCategory? FilterCategory { get; set; }
        public bool HasResults => Articles.Any();
    }
}
