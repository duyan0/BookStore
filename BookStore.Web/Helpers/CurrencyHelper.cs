using System.Globalization;

namespace BookStore.Web.Helpers
{
    public static class CurrencyHelper
    {
        /// <summary>
        /// Format currency to Vietnamese Dong
        /// </summary>
        /// <param name="amount">Amount to format</param>
        /// <returns>Formatted currency string</returns>
        public static string FormatVND(decimal amount)
        {
            return amount.ToString("N0", CultureInfo.InvariantCulture) + " VNĐ";
        }

        /// <summary>
        /// Format currency to Vietnamese Dong with custom format
        /// </summary>
        /// <param name="amount">Amount to format</param>
        /// <param name="showSymbol">Whether to show VNĐ symbol</param>
        /// <returns>Formatted currency string</returns>
        public static string FormatVND(decimal amount, bool showSymbol = true)
        {
            var formatted = amount.ToString("N0", CultureInfo.InvariantCulture);
            return showSymbol ? formatted + " VNĐ" : formatted;
        }

        /// <summary>
        /// Format currency for display in tables
        /// </summary>
        /// <param name="amount">Amount to format</param>
        /// <returns>Formatted currency string</returns>
        public static string FormatVNDCompact(decimal amount)
        {
            if (amount >= 1_000_000_000)
            {
                return (amount / 1_000_000_000).ToString("0.#") + " tỷ VNĐ";
            }
            else if (amount >= 1_000_000)
            {
                return (amount / 1_000_000).ToString("0.#") + " triệu VNĐ";
            }
            else if (amount >= 1_000)
            {
                return (amount / 1_000).ToString("0.#") + " nghìn VNĐ";
            }
            else
            {
                return amount.ToString("N0", CultureInfo.InvariantCulture) + " VNĐ";
            }
        }

        /// <summary>
        /// Parse VND string back to decimal
        /// </summary>
        /// <param name="vndString">VND formatted string</param>
        /// <returns>Decimal value</returns>
        public static decimal ParseVND(string vndString)
        {
            if (string.IsNullOrEmpty(vndString))
                return 0;

            // Remove VNĐ and spaces
            var cleanString = vndString.Replace("VNĐ", "").Replace(" ", "").Replace(",", "");
            
            if (decimal.TryParse(cleanString, out decimal result))
                return result;
            
            return 0;
        }
    }
}
