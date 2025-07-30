class ApiConstants {
  // Base URL - có thể thay đổi theo môi trường
  // Sử dụng 10.0.2.2 cho Android emulator, localhost cho iOS simulator
  // Hoặc IP thực của máy cho device thật (ví dụ: 192.168.1.100)
  static const String baseUrl = 'http://10.0.2.2:5274/api';

  // Auth endpoints
  static const String login = '/auth/login';
  static const String register = '/auth/register';

  // Books endpoints
  static const String books = '/books';
  static const String booksSearch = '/books/search';

  // Authors endpoints
  static const String authors = '/authors';
  static const String authorsSearch = '/authors/search';

  // Categories endpoints
  static const String categories = '/categories';

  // Orders endpoints
  static const String orders = '/orders';
  static const String ordersStatistics = '/orders/statistics';

  // Users endpoints
  static const String users = '/users';

  // Reviews endpoints
  static const String reviews = '/reviews';

  // Headers
  static const String contentType = 'application/json';
  static const String authorization = 'Authorization';
  static const String bearer = 'Bearer';

  // Storage keys
  static const String tokenKey = 'auth_token';
  static const String userKey = 'user_data';
  static const String isAdminKey = 'is_admin';

  // Timeouts
  static const int connectionTimeout = 30000; // 30 seconds
  static const int receiveTimeout = 30000; // 30 seconds
}
