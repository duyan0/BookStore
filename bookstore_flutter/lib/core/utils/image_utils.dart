class ImageUtils {
  // Base URL for localhost API
  static const String _baseUrl = 'http://10.0.2.2:5274';
  
  /// Converts relative image URL to full URL
  /// If URL already starts with http, returns as is
  /// Otherwise prepends base URL
  static String getFullImageUrl(String imageUrl) {
    if (imageUrl.isEmpty) {
      return '';
    }
    
    if (imageUrl.startsWith('http')) {
      return imageUrl;
    }
    
    // Ensure imageUrl starts with /
    if (!imageUrl.startsWith('/')) {
      imageUrl = '/$imageUrl';
    }
    
    return '$_baseUrl$imageUrl';
  }
  
  /// Gets placeholder image URL for fallback
  static String getPlaceholderImageUrl() {
    return 'https://via.placeholder.com/300x400/f0f0f0/666666?text=No+Image';
  }
}
