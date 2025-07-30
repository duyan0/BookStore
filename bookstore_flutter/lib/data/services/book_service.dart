import '../../core/network/api_client.dart';
import '../../core/constants/api_constants.dart';
import '../../core/errors/api_exception.dart';
import '../models/book_model.dart';

class BookService {
  final ApiClient _apiClient;

  BookService(this._apiClient);

  /// Lấy danh sách tất cả sách
  Future<List<BookModel>> getAllBooks() async {
    try {
      final response = await _apiClient.get<List<dynamic>>(ApiConstants.books);

      return response.map((json) => BookModel.fromJson(json)).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch books: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy chi tiết sách theo ID
  Future<BookModel> getBookById(int id) async {
    try {
      final response = await _apiClient.get<Map<String, dynamic>>(
        '${ApiConstants.books}/$id',
      );

      return BookModel.fromJson(response);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch book details: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Tìm kiếm sách
  Future<List<BookModel>> searchBooks(String searchTerm) async {
    try {
      final response = await _apiClient.get<List<dynamic>>(
        ApiConstants.booksSearch,
        queryParameters: {'term': searchTerm},
      );

      return response.map((json) => BookModel.fromJson(json)).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to search books: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy sách theo thể loại
  Future<List<BookModel>> getBooksByCategory(int categoryId) async {
    try {
      final response = await _apiClient.get<List<dynamic>>(
        ApiConstants.books,
        queryParameters: {'categoryId': categoryId},
      );

      return response.map((json) => BookModel.fromJson(json)).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch books by category: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy sách theo tác giả
  Future<List<BookModel>> getBooksByAuthor(int authorId) async {
    try {
      final response = await _apiClient.get<List<dynamic>>(
        ApiConstants.books,
        queryParameters: {'authorId': authorId},
      );

      return response.map((json) => BookModel.fromJson(json)).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch books by author: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Tạo sách mới (Admin only)
  Future<BookModel> createBook(CreateBookModel bookData) async {
    try {
      final response = await _apiClient.post<Map<String, dynamic>>(
        ApiConstants.books,
        data: bookData.toJson(),
      );

      return BookModel.fromJson(response);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to create book: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Cập nhật sách (Admin only)
  Future<BookModel> updateBook(int id, CreateBookModel bookData) async {
    try {
      final response = await _apiClient.put<Map<String, dynamic>>(
        '${ApiConstants.books}/$id',
        data: bookData.toJson(),
      );

      return BookModel.fromJson(response);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to update book: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Xóa sách (Admin only)
  Future<void> deleteBook(int id) async {
    try {
      await _apiClient.delete<void>('${ApiConstants.books}/$id');
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to delete book: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy sách khuyến mãi
  Future<List<BookModel>> getDiscountedBooks() async {
    try {
      final allBooks = await getAllBooks();
      return allBooks.where((book) => book.hasDiscount).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch discounted books: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy sách mới nhất
  Future<List<BookModel>> getLatestBooks({int limit = 10}) async {
    try {
      final allBooks = await getAllBooks();
      // Sort by creation date and take the latest ones
      allBooks.sort((a, b) => b.createdAt.compareTo(a.createdAt));
      return allBooks.take(limit).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch latest books: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy sách bán chạy (giả lập - cần API thực tế)
  Future<List<BookModel>> getBestSellingBooks({int limit = 10}) async {
    try {
      final allBooks = await getAllBooks();
      // For now, just return random books
      // In real implementation, this should be based on sales data
      return allBooks.take(limit).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch best selling books: ${e.toString()}',
        statusCode: 0,
      );
    }
  }
}
