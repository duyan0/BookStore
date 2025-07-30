import 'package:flutter/foundation.dart';
import '../../data/models/book_model.dart';
import '../../data/services/book_service.dart';
import '../../core/errors/api_exception.dart';

enum BookLoadingState {
  initial,
  loading,
  loaded,
  error,
}

class BookProvider extends ChangeNotifier {
  final BookService _bookService;

  BookProvider(this._bookService);

  // State
  BookLoadingState _state = BookLoadingState.initial;
  List<BookModel> _books = [];
  List<BookModel> _filteredBooks = [];
  List<BookModel> _featuredBooks = [];
  List<BookModel> _discountedBooks = [];
  BookModel? _selectedBook;
  String? _errorMessage;
  String _searchQuery = '';
  int? _selectedCategoryId;
  int? _selectedAuthorId;

  // Getters
  BookLoadingState get state => _state;
  List<BookModel> get books => _books;
  List<BookModel> get filteredBooks => _filteredBooks;
  List<BookModel> get featuredBooks => _featuredBooks;
  List<BookModel> get discountedBooks => _discountedBooks;
  BookModel? get selectedBook => _selectedBook;
  String? get errorMessage => _errorMessage;
  String get searchQuery => _searchQuery;
  int? get selectedCategoryId => _selectedCategoryId;
  int? get selectedAuthorId => _selectedAuthorId;
  bool get isLoading => _state == BookLoadingState.loading;
  bool get hasError => _state == BookLoadingState.error;
  bool get isEmpty => _books.isEmpty && _state == BookLoadingState.loaded;

  /// Load all books
  Future<void> loadBooks() async {
    try {
      _setState(BookLoadingState.loading);
      _clearError();

      _books = await _bookService.getAllBooks();
      _filteredBooks = List.from(_books);
      
      // Load featured and discounted books
      await _loadFeaturedBooks();
      await _loadDiscountedBooks();

      _setState(BookLoadingState.loaded);
    } on ApiException catch (e) {
      _setError(e.message);
    } catch (e) {
      _setError('Failed to load books: ${e.toString()}');
    }
  }

  /// Load featured books (latest books)
  Future<void> _loadFeaturedBooks() async {
    try {
      _featuredBooks = await _bookService.getLatestBooks(limit: 10);
    } catch (e) {
      // Handle error silently for featured books
    }
  }

  /// Load discounted books
  Future<void> _loadDiscountedBooks() async {
    try {
      _discountedBooks = await _bookService.getDiscountedBooks();
    } catch (e) {
      // Handle error silently for discounted books
    }
  }

  /// Load book by ID
  Future<void> loadBookById(int id) async {
    try {
      _setState(BookLoadingState.loading);
      _clearError();

      _selectedBook = await _bookService.getBookById(id);
      _setState(BookLoadingState.loaded);
    } on ApiException catch (e) {
      _setError(e.message);
    } catch (e) {
      _setError('Failed to load book details: ${e.toString()}');
    }
  }

  /// Search books
  Future<void> searchBooks(String query) async {
    try {
      _setState(BookLoadingState.loading);
      _clearError();
      _searchQuery = query;

      if (query.trim().isEmpty) {
        _filteredBooks = List.from(_books);
      } else {
        final searchResults = await _bookService.searchBooks(query);
        _filteredBooks = searchResults;
      }

      _setState(BookLoadingState.loaded);
    } on ApiException catch (e) {
      _setError(e.message);
    } catch (e) {
      _setError('Failed to search books: ${e.toString()}');
    }
  }

  /// Filter books by category
  Future<void> filterByCategory(int? categoryId) async {
    try {
      _setState(BookLoadingState.loading);
      _clearError();
      _selectedCategoryId = categoryId;

      if (categoryId == null) {
        _filteredBooks = List.from(_books);
      } else {
        final categoryBooks = await _bookService.getBooksByCategory(categoryId);
        _filteredBooks = categoryBooks;
      }

      _setState(BookLoadingState.loaded);
    } on ApiException catch (e) {
      _setError(e.message);
    } catch (e) {
      _setError('Failed to filter books by category: ${e.toString()}');
    }
  }

  /// Filter books by author
  Future<void> filterByAuthor(int? authorId) async {
    try {
      _setState(BookLoadingState.loading);
      _clearError();
      _selectedAuthorId = authorId;

      if (authorId == null) {
        _filteredBooks = List.from(_books);
      } else {
        final authorBooks = await _bookService.getBooksByAuthor(authorId);
        _filteredBooks = authorBooks;
      }

      _setState(BookLoadingState.loaded);
    } on ApiException catch (e) {
      _setError(e.message);
    } catch (e) {
      _setError('Failed to filter books by author: ${e.toString()}');
    }
  }

  /// Clear all filters
  void clearFilters() {
    _searchQuery = '';
    _selectedCategoryId = null;
    _selectedAuthorId = null;
    _filteredBooks = List.from(_books);
    notifyListeners();
  }

  /// Refresh books
  Future<void> refreshBooks() async {
    await loadBooks();
  }

  /// Clear selected book
  void clearSelectedBook() {
    _selectedBook = null;
    notifyListeners();
  }

  /// Clear error
  void clearError() {
    _clearError();
  }

  // Private methods
  void _setState(BookLoadingState newState) {
    _state = newState;
    notifyListeners();
  }

  void _setError(String error) {
    _errorMessage = error;
    _state = BookLoadingState.error;
    notifyListeners();
  }

  void _clearError() {
    _errorMessage = null;
    if (_state == BookLoadingState.error) {
      _state = _books.isEmpty ? BookLoadingState.initial : BookLoadingState.loaded;
    }
    notifyListeners();
  }
}
