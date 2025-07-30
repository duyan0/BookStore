import 'package:flutter/foundation.dart';
import '../../data/models/category_model.dart';
import '../../data/services/category_service.dart';
import '../../core/errors/api_exception.dart';

enum CategoryLoadingState {
  initial,
  loading,
  loaded,
  error,
}

class CategoryProvider extends ChangeNotifier {
  final CategoryService _categoryService;

  CategoryProvider(this._categoryService);

  // State
  CategoryLoadingState _state = CategoryLoadingState.initial;
  List<CategoryModel> _categories = [];
  CategoryModel? _selectedCategory;
  String? _errorMessage;

  // Getters
  CategoryLoadingState get state => _state;
  List<CategoryModel> get categories => _categories;
  CategoryModel? get selectedCategory => _selectedCategory;
  String? get errorMessage => _errorMessage;
  bool get isLoading => _state == CategoryLoadingState.loading;
  bool get hasError => _state == CategoryLoadingState.error;
  bool get isEmpty => _categories.isEmpty && _state == CategoryLoadingState.loaded;

  /// Load all categories
  Future<void> loadCategories() async {
    try {
      _setState(CategoryLoadingState.loading);
      _clearError();

      _categories = await _categoryService.getAllCategories();
      _setState(CategoryLoadingState.loaded);
    } on ApiException catch (e) {
      _setError(e.message);
    } catch (e) {
      _setError('Failed to load categories: ${e.toString()}');
    }
  }

  /// Load category by ID
  Future<void> loadCategoryById(int id) async {
    try {
      _setState(CategoryLoadingState.loading);
      _clearError();

      _selectedCategory = await _categoryService.getCategoryById(id);
      _setState(CategoryLoadingState.loaded);
    } on ApiException catch (e) {
      _setError(e.message);
    } catch (e) {
      _setError('Failed to load category details: ${e.toString()}');
    }
  }

  /// Create new category (Admin only)
  Future<bool> createCategory(CreateCategoryModel categoryData) async {
    try {
      _setState(CategoryLoadingState.loading);
      _clearError();

      final newCategory = await _categoryService.createCategory(categoryData);
      _categories.add(newCategory);
      _setState(CategoryLoadingState.loaded);
      return true;
    } on ApiException catch (e) {
      _setError(e.message);
      return false;
    } catch (e) {
      _setError('Failed to create category: ${e.toString()}');
      return false;
    }
  }

  /// Update category (Admin only)
  Future<bool> updateCategory(int id, UpdateCategoryModel categoryData) async {
    try {
      _setState(CategoryLoadingState.loading);
      _clearError();

      final updatedCategory = await _categoryService.updateCategory(id, categoryData);
      final index = _categories.indexWhere((category) => category.id == id);
      if (index != -1) {
        _categories[index] = updatedCategory;
      }
      _setState(CategoryLoadingState.loaded);
      return true;
    } on ApiException catch (e) {
      _setError(e.message);
      return false;
    } catch (e) {
      _setError('Failed to update category: ${e.toString()}');
      return false;
    }
  }

  /// Delete category (Admin only)
  Future<bool> deleteCategory(int id) async {
    try {
      _setState(CategoryLoadingState.loading);
      _clearError();

      await _categoryService.deleteCategory(id);
      _categories.removeWhere((category) => category.id == id);
      _setState(CategoryLoadingState.loaded);
      return true;
    } on ApiException catch (e) {
      _setError(e.message);
      return false;
    } catch (e) {
      _setError('Failed to delete category: ${e.toString()}');
      return false;
    }
  }

  /// Get category by ID
  CategoryModel? getCategoryById(int id) {
    try {
      return _categories.firstWhere((category) => category.id == id);
    } catch (e) {
      return null;
    }
  }

  /// Get top categories
  List<CategoryModel> getTopCategories({int limit = 5}) {
    final sortedCategories = List<CategoryModel>.from(_categories);
    sortedCategories.sort((a, b) => b.bookCount.compareTo(a.bookCount));
    return sortedCategories.take(limit).toList();
  }

  /// Refresh categories
  Future<void> refreshCategories() async {
    await loadCategories();
  }

  /// Clear selected category
  void clearSelectedCategory() {
    _selectedCategory = null;
    notifyListeners();
  }

  /// Clear error
  void clearError() {
    _clearError();
  }

  // Private methods
  void _setState(CategoryLoadingState newState) {
    _state = newState;
    notifyListeners();
  }

  void _setError(String error) {
    _errorMessage = error;
    _state = CategoryLoadingState.error;
    notifyListeners();
  }

  void _clearError() {
    _errorMessage = null;
    if (_state == CategoryLoadingState.error) {
      _state = _categories.isEmpty ? CategoryLoadingState.initial : CategoryLoadingState.loaded;
    }
    notifyListeners();
  }
}
