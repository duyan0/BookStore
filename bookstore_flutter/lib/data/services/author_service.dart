import '../../core/network/api_client.dart';
import '../../core/constants/api_constants.dart';
import '../../core/errors/api_exception.dart';
import '../models/author_model.dart';

class AuthorService {
  final ApiClient _apiClient;

  AuthorService(this._apiClient);

  /// Lấy danh sách tất cả tác giả
  Future<List<AuthorModel>> getAllAuthors() async {
    try {
      final response = await _apiClient.get<List<dynamic>>(
        ApiConstants.authors,
      );

      return response.map((json) => AuthorModel.fromJson(json)).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch authors: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy chi tiết tác giả theo ID
  Future<AuthorModel> getAuthorById(int id) async {
    try {
      final response = await _apiClient.get<Map<String, dynamic>>(
        '${ApiConstants.authors}/$id',
      );

      return AuthorModel.fromJson(response);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch author details: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Tìm kiếm tác giả
  Future<List<AuthorModel>> searchAuthors(String searchTerm) async {
    try {
      final response = await _apiClient.get<List<dynamic>>(
        ApiConstants.authorsSearch,
        queryParameters: {'term': searchTerm},
      );

      return response.map((json) => AuthorModel.fromJson(json)).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to search authors: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Tạo tác giả mới (Admin only)
  Future<AuthorModel> createAuthor(CreateAuthorModel authorData) async {
    try {
      final response = await _apiClient.post<Map<String, dynamic>>(
        ApiConstants.authors,
        data: authorData.toJson(),
      );

      return AuthorModel.fromJson(response);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to create author: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Cập nhật tác giả (Admin only)
  Future<AuthorModel> updateAuthor(int id, UpdateAuthorModel authorData) async {
    try {
      final response = await _apiClient.put<Map<String, dynamic>>(
        '${ApiConstants.authors}/$id',
        data: authorData.toJson(),
      );

      return AuthorModel.fromJson(response);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to update author: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Xóa tác giả (Admin only)
  Future<void> deleteAuthor(int id) async {
    try {
      await _apiClient.delete<void>('${ApiConstants.authors}/$id');
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to delete author: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy tác giả có nhiều sách nhất
  Future<List<AuthorModel>> getTopAuthors({int limit = 10}) async {
    try {
      final allAuthors = await getAllAuthors();
      // Sort by book count in descending order
      allAuthors.sort((a, b) => b.bookCount.compareTo(a.bookCount));
      return allAuthors.take(limit).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch top authors: ${e.toString()}',
        statusCode: 0,
      );
    }
  }
}
