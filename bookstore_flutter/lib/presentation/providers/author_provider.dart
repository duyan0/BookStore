import 'package:flutter/foundation.dart';
import '../../data/models/author_model.dart';
import '../../data/services/author_service.dart';

enum AuthorLoadingState { initial, loading, loaded, error }

class AuthorProvider extends ChangeNotifier {
  final AuthorService _authorService;

  AuthorProvider(this._authorService);

  // State
  AuthorLoadingState _state = AuthorLoadingState.initial;
  List<AuthorModel> _authors = [];
  AuthorModel? _selectedAuthor;
  String? _errorMessage;

  // Getters
  AuthorLoadingState get state => _state;
  List<AuthorModel> get authors => _authors;
  AuthorModel? get selectedAuthor => _selectedAuthor;
  String? get errorMessage => _errorMessage;
  bool get isLoading => _state == AuthorLoadingState.loading;
  bool get hasError => _state == AuthorLoadingState.error;
  bool get isEmpty => _authors.isEmpty && _state == AuthorLoadingState.loaded;

  /// Load all authors
  Future<void> loadAuthors() async {
    try {
      _setState(AuthorLoadingState.loading);
      _clearError();

      final authors = await _authorService.getAllAuthors();
      _authors = authors;
      _setState(AuthorLoadingState.loaded);
    } catch (e) {
      _setError('Failed to load authors: ${e.toString()}');
    }
  }

  /// Get author by ID
  Future<void> getAuthorById(int id) async {
    try {
      _setState(AuthorLoadingState.loading);
      _clearError();

      final author = await _authorService.getAuthorById(id);
      _selectedAuthor = author;
      _setState(AuthorLoadingState.loaded);
    } catch (e) {
      _setError('Failed to get author: ${e.toString()}');
    }
  }

  /// Search authors
  Future<void> searchAuthors(String query) async {
    try {
      _setState(AuthorLoadingState.loading);
      _clearError();

      final authors = await _authorService.searchAuthors(query);
      _authors = authors;
      _setState(AuthorLoadingState.loaded);
    } catch (e) {
      _setError('Failed to search authors: ${e.toString()}');
    }
  }

  /// Refresh authors
  Future<void> refreshAuthors() async {
    await loadAuthors();
  }

  /// Clear error message
  void clearError() {
    _clearError();
  }

  /// Create new author (Admin only)
  Future<bool> createAuthor(CreateAuthorModel authorData) async {
    try {
      _setState(AuthorLoadingState.loading);
      _clearError();

      final newAuthor = await _authorService.createAuthor(authorData);
      _authors.add(newAuthor);
      _setState(AuthorLoadingState.loaded);
      return true;
    } catch (e) {
      _setError('Failed to create author: ${e.toString()}');
      return false;
    }
  }

  /// Update author (Admin only)
  Future<bool> updateAuthor(int id, UpdateAuthorModel authorData) async {
    try {
      _setState(AuthorLoadingState.loading);
      _clearError();

      final updatedAuthor = await _authorService.updateAuthor(id, authorData);
      final index = _authors.indexWhere((author) => author.id == id);
      if (index != -1) {
        _authors[index] = updatedAuthor;
      }
      _setState(AuthorLoadingState.loaded);
      return true;
    } catch (e) {
      _setError('Failed to update author: ${e.toString()}');
      return false;
    }
  }

  /// Delete author (Admin only)
  Future<bool> deleteAuthor(int id) async {
    try {
      _setState(AuthorLoadingState.loading);
      _clearError();

      await _authorService.deleteAuthor(id);
      _authors.removeWhere((author) => author.id == id);
      _setState(AuthorLoadingState.loaded);
      return true;
    } catch (e) {
      _setError('Failed to delete author: ${e.toString()}');
      return false;
    }
  }

  // Private methods
  void _setState(AuthorLoadingState newState) {
    _state = newState;
    notifyListeners();
  }

  void _setError(String message) {
    _errorMessage = message;
    _state = AuthorLoadingState.error;
    notifyListeners();
  }

  void _clearError() {
    _errorMessage = null;
    notifyListeners();
  }
}
