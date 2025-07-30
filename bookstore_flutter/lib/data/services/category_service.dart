import '../../core/network/api_client.dart';
import '../../core/constants/api_constants.dart';
import '../../core/errors/api_exception.dart';
import '../models/category_model.dart';

class CategoryService {
  final ApiClient _apiClient;

  CategoryService(this._apiClient);

  /// Lấy danh sách tất cả thể loại
  Future<List<CategoryModel>> getAllCategories() async {
    try {
      final response = await _apiClient.get<List<dynamic>>(
        ApiConstants.categories,
      );

      return response.map((json) => CategoryModel.fromJson(json)).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch categories: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy chi tiết thể loại theo ID
  Future<CategoryModel> getCategoryById(int id) async {
    try {
      final response = await _apiClient.get<Map<String, dynamic>>(
        '${ApiConstants.categories}/$id',
      );

      return CategoryModel.fromJson(response);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch category details: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Tạo thể loại mới (Admin only)
  Future<CategoryModel> createCategory(CreateCategoryModel categoryData) async {
    try {
      final response = await _apiClient.post<Map<String, dynamic>>(
        ApiConstants.categories,
        data: categoryData.toJson(),
      );

      return CategoryModel.fromJson(response);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to create category: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Cập nhật thể loại (Admin only)
  Future<CategoryModel> updateCategory(int id, UpdateCategoryModel categoryData) async {
    try {
      final response = await _apiClient.put<Map<String, dynamic>>(
        '${ApiConstants.categories}/$id',
        data: categoryData.toJson(),
      );

      return CategoryModel.fromJson(response);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to update category: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Xóa thể loại (Admin only)
  Future<void> deleteCategory(int id) async {
    try {
      await _apiClient.delete<void>('${ApiConstants.categories}/$id');
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to delete category: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy thể loại có nhiều sách nhất
  Future<List<CategoryModel>> getTopCategories({int limit = 10}) async {
    try {
      final allCategories = await getAllCategories();
      // Sort by book count in descending order
      allCategories.sort((a, b) => b.bookCount.compareTo(a.bookCount));
      return allCategories.take(limit).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch top categories: ${e.toString()}',
        statusCode: 0,
      );
    }
  }
}
