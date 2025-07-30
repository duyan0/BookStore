class AppRoutes {
  // Auth routes
  static const String login = '/login';
  static const String register = '/register';
  
  // User routes
  static const String userHome = '/user';
  static const String bookList = '/user/books';
  static const String bookDetail = '/user/books/:id';
  static const String search = '/user/search';
  static const String cart = '/user/cart';
  static const String checkout = '/user/checkout';
  static const String orderHistory = '/user/orders';
  static const String orderDetail = '/user/orders/:id';
  static const String profile = '/user/profile';
  
  // Admin routes
  static const String adminHome = '/admin';
  static const String adminBooks = '/admin/books';
  static const String adminBookCreate = '/admin/books/create';
  static const String adminBookEdit = '/admin/books/:id/edit';
  static const String adminAuthors = '/admin/authors';
  static const String adminAuthorCreate = '/admin/authors/create';
  static const String adminAuthorEdit = '/admin/authors/:id/edit';
  static const String adminCategories = '/admin/categories';
  static const String adminCategoryCreate = '/admin/categories/create';
  static const String adminCategoryEdit = '/admin/categories/:id/edit';
  static const String adminOrders = '/admin/orders';
  static const String adminOrderDetail = '/admin/orders/:id';
  static const String adminStatistics = '/admin/statistics';
  
  // Helper methods
  static String bookDetailPath(int id) => '/user/books/$id';
  static String orderDetailPath(int id) => '/user/orders/$id';
  static String adminBookEditPath(int id) => '/admin/books/$id/edit';
  static String adminAuthorEditPath(int id) => '/admin/authors/$id/edit';
  static String adminCategoryEditPath(int id) => '/admin/categories/$id/edit';
  static String adminOrderDetailPath(int id) => '/admin/orders/$id';
}
