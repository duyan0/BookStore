using BookStore.Core.DTOs;
using BookStore.Web.Models;

namespace BookStore.Web.Helpers
{
    public static class MappingHelper
    {
        public static BookViewModel MapBookToViewModel(BookDto book)
        {
            return new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description ?? "",
                Price = book.Price,
                
                // Discount fields
                DiscountPercentage = book.DiscountPercentage ?? 0,
                DiscountAmount = book.DiscountAmount,
                IsOnSale = book.IsOnSale,
                SaleStartDate = book.SaleStartDate,
                SaleEndDate = book.SaleEndDate,
                
                ImageUrl = book.ImageUrl ?? "/images/no-image.jpg",
                AuthorName = book.AuthorName ?? "Unknown Author",
                CategoryName = book.CategoryName ?? "Unknown Category",
                CategoryId = book.CategoryId,
                AuthorId = book.AuthorId,
                ISBN = book.ISBN ?? "",
                Publisher = book.Publisher ?? "",
                PublicationYear = book.PublicationYear ?? 0,
                Quantity = book.Quantity,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt
            };
        }

        public static CategoryViewModel MapCategoryToViewModel(CategoryDto category)
        {
            return new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description ?? ""
            };
        }
    }
}
