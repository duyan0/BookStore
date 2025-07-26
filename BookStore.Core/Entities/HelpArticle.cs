using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.Entities
{
    public class HelpArticle : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Slug { get; set; } = string.Empty; // URL-friendly identifier

        [Required]
        public string Content { get; set; } = string.Empty; // Rich text content

        [StringLength(500)]
        public string Summary { get; set; } = string.Empty; // Short description

        [Required]
        public HelpArticleType Type { get; set; }

        [Required]
        public HelpArticleCategory Category { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsPublished { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        // SEO fields
        [StringLength(200)]
        public string? MetaTitle { get; set; }

        [StringLength(500)]
        public string? MetaDescription { get; set; }

        [StringLength(200)]
        public string? MetaKeywords { get; set; }

        // Analytics
        public int ViewCount { get; set; } = 0;
        public DateTime? LastViewedAt { get; set; }

        // Author tracking
        public int AuthorId { get; set; }
        public User? Author { get; set; }

        public int? LastModifiedById { get; set; }
        public User? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        // Related articles
        public string? RelatedArticleIds { get; set; } // JSON array of IDs

        // Computed properties
        public string TypeName => Type.ToString();
        public string CategoryName => Category.ToString();
        public bool IsRecentlyUpdated => LastModifiedAt.HasValue && 
                                        LastModifiedAt.Value > DateTime.UtcNow.AddDays(-7);
    }

    public enum HelpArticleType
    {
        FAQ = 1,
        Policy = 2,
        Guide = 3,
        Tutorial = 4,
        Announcement = 5
    }

    public enum HelpArticleCategory
    {
        General = 1,
        Account = 2,
        Orders = 3,
        Payment = 4,
        Shipping = 5,
        Returns = 6,
        Technical = 7,
        Privacy = 8,
        Terms = 9,
        Contact = 10
    }
}
