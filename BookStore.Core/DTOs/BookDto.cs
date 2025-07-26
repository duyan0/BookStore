namespace BookStore.Core.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; } // Original price

        // Discount fields
        public decimal? DiscountPercentage { get; set; } // 0-100%, null means no discount
        public decimal DiscountAmount { get; set; } = 0; // Fixed discount amount
        public bool IsOnSale { get; set; } = false;
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }

        // Computed properties
        public decimal DiscountedPrice { get; set; } // Final price after discount
        public bool IsDiscountActive { get; set; } // Whether discount is currently active
        public decimal TotalDiscountAmount { get; set; } // Total savings amount

        public int Quantity { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int? PublicationYear { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 