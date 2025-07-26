using System;
using System.Globalization;

namespace BookStore.Web.Helpers
{
    public static class PriceHelper
    {
        /// <summary>
        /// Format price to Vietnamese Dong with comma separators
        /// </summary>
        /// <param name="price">Price to format</param>
        /// <returns>Formatted price string (e.g., "100,000 VND")</returns>
        public static string FormatVND(decimal price)
        {
            return price.ToString("#,##0", CultureInfo.InvariantCulture) + " VND";
        }

        /// <summary>
        /// Format price to Vietnamese Dong without VND suffix
        /// </summary>
        /// <param name="price">Price to format</param>
        /// <returns>Formatted price string (e.g., "100,000")</returns>
        public static string FormatVNDWithoutSuffix(decimal price)
        {
            return price.ToString("#,##0", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Calculate discount percentage between original and final price
        /// </summary>
        /// <param name="originalPrice">Original price</param>
        /// <param name="finalPrice">Final price after discount</param>
        /// <returns>Discount percentage</returns>
        public static decimal CalculateDiscountPercentage(decimal originalPrice, decimal finalPrice)
        {
            if (originalPrice <= 0) return 0;
            return Math.Round(((originalPrice - finalPrice) / originalPrice) * 100, 0);
        }

        /// <summary>
        /// Calculate savings amount
        /// </summary>
        /// <param name="originalPrice">Original price</param>
        /// <param name="finalPrice">Final price after discount</param>
        /// <returns>Savings amount</returns>
        public static decimal CalculateSavings(decimal originalPrice, decimal finalPrice)
        {
            return Math.Max(0, originalPrice - finalPrice);
        }

        /// <summary>
        /// Check if a book has active discount
        /// </summary>
        /// <param name="discountPercentage">Discount percentage</param>
        /// <param name="discountAmount">Fixed discount amount</param>
        /// <param name="isOnSale">Is on sale flag</param>
        /// <param name="saleStartDate">Sale start date</param>
        /// <param name="saleEndDate">Sale end date</param>
        /// <returns>True if discount is active</returns>
        public static bool HasActiveDiscount(decimal? discountPercentage, decimal discountAmount, 
            bool isOnSale, DateTime? saleStartDate, DateTime? saleEndDate)
        {
            var now = DateTime.UtcNow;
            var hasValidDiscountPercentage = discountPercentage.HasValue && discountPercentage.Value > 0;
            var hasValidDiscountAmount = discountAmount > 0;
            var isInSalePeriod = !isOnSale || 
                ((saleStartDate == null || saleStartDate <= now) &&
                 (saleEndDate == null || saleEndDate >= now));

            return (hasValidDiscountPercentage || hasValidDiscountAmount) && isInSalePeriod;
        }

        /// <summary>
        /// Calculate final price after discount
        /// </summary>
        /// <param name="originalPrice">Original price</param>
        /// <param name="discountPercentage">Discount percentage</param>
        /// <param name="discountAmount">Fixed discount amount</param>
        /// <param name="isOnSale">Is on sale flag</param>
        /// <param name="saleStartDate">Sale start date</param>
        /// <param name="saleEndDate">Sale end date</param>
        /// <returns>Final price after discount</returns>
        public static decimal CalculateFinalPrice(decimal originalPrice, decimal? discountPercentage, 
            decimal discountAmount, bool isOnSale, DateTime? saleStartDate, DateTime? saleEndDate)
        {
            if (!HasActiveDiscount(discountPercentage, discountAmount, isOnSale, saleStartDate, saleEndDate))
                return originalPrice;

            // Calculate discount based on percentage
            var discountFromPercentage = discountPercentage.HasValue 
                ? originalPrice * (discountPercentage.Value / 100) 
                : 0;
            
            // Add fixed discount amount if any
            var totalDiscount = discountFromPercentage + discountAmount;
            var finalPrice = originalPrice - totalDiscount;

            // Ensure price doesn't go below 0
            return Math.Max(0, finalPrice);
        }
    }
}
