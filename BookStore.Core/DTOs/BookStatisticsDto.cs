namespace BookStore.Core.DTOs
{
    public class BookStatisticsDto
    {
        public int TotalBooks { get; set; }
        public int BooksInStock { get; set; }
        public int BooksOutOfStock { get; set; }
        public int LowStockBooks { get; set; } // Books with quantity < 10
        public int NewBooksThisMonth { get; set; }
        public int NewBooksThisWeek { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public decimal AverageBookPrice { get; set; }
        public int TotalCategories { get; set; }
        public int TotalAuthors { get; set; }
        public DateTime LastUpdated { get; set; }
        
        // Top selling categories (if needed later)
        public List<CategoryStatDto> TopCategories { get; set; } = new List<CategoryStatDto>();
        
        // Recent books
        public List<BookDto> RecentBooks { get; set; } = new List<BookDto>();
    }

    public class CategoryStatDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int BookCount { get; set; }
        public decimal TotalValue { get; set; }
    }
}
