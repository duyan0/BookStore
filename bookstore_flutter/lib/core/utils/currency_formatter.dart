import 'package:intl/intl.dart';

class CurrencyFormatter {
  static final NumberFormat _vndFormatter = NumberFormat.currency(
    locale: 'vi_VN',
    symbol: '',
    decimalDigits: 0,
  );
  
  /// Format số tiền theo định dạng VND với dấu phẩy
  /// Ví dụ: 100000 -> "100,000"
  static String formatVND(num amount) {
    if (amount == 0) return '0';
    return _vndFormatter.format(amount);
  }
  
  /// Format số tiền với đơn vị VND
  /// Ví dụ: 100000 -> "100,000 VND"
  static String formatVNDWithUnit(num amount) {
    return '${formatVND(amount)} VND';
  }
  
  /// Format số tiền với ký hiệu đồng
  /// Ví dụ: 100000 -> "100,000₫"
  static String formatVNDWithSymbol(num amount) {
    return '${formatVND(amount)}₫';
  }
  
  /// Parse chuỗi tiền tệ về số
  /// Ví dụ: "100,000" -> 100000
  static double parseVND(String amount) {
    try {
      // Remove all non-digit characters except decimal point
      String cleanAmount = amount.replaceAll(RegExp(r'[^\d.]'), '');
      return double.tryParse(cleanAmount) ?? 0.0;
    } catch (e) {
      return 0.0;
    }
  }
  
  /// Kiểm tra xem có phải là giá khuyến mãi không
  static bool isDiscounted(num originalPrice, num currentPrice) {
    return currentPrice < originalPrice && originalPrice > 0;
  }
  
  /// Tính phần trăm giảm giá
  static double calculateDiscountPercentage(num originalPrice, num discountedPrice) {
    if (originalPrice <= 0) return 0.0;
    return ((originalPrice - discountedPrice) / originalPrice * 100);
  }
  
  /// Format phần trăm giảm giá
  /// Ví dụ: 0.25 -> "25%"
  static String formatDiscountPercentage(double percentage) {
    return '${percentage.round()}%';
  }
  
  /// Tính số tiền tiết kiệm được
  static num calculateSavings(num originalPrice, num discountedPrice) {
    return originalPrice - discountedPrice;
  }
}
