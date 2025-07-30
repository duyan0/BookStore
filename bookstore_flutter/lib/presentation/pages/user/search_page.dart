import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import '../../../core/constants/app_colors.dart';
import '../../../core/constants/app_strings.dart';
import '../../../data/models/book_model.dart';
import '../../providers/book_provider.dart';
import '../../providers/cart_provider.dart';
import '../../widgets/loading_widget.dart';
import '../../widgets/error_widget.dart';
import '../../widgets/book_card_widget.dart';
import '../../routes/app_routes.dart';
import '../../../data/services/storage_service.dart';

class SearchPage extends StatefulWidget {
  const SearchPage({super.key});

  @override
  State<SearchPage> createState() => _SearchPageState();
}

class _SearchPageState extends State<SearchPage> {
  final TextEditingController _searchController = TextEditingController();
  final FocusNode _searchFocusNode = FocusNode();
  List<String> _searchHistory = [];
  bool _showHistory = true;

  @override
  void initState() {
    super.initState();
    _loadSearchHistory();
    _searchFocusNode.requestFocus();
  }

  @override
  void dispose() {
    _searchController.dispose();
    _searchFocusNode.dispose();
    super.dispose();
  }

  Future<void> _loadSearchHistory() async {
    final history = await StorageService.getSearchHistory();
    setState(() {
      _searchHistory = history;
    });
  }

  Future<void> _saveSearchTerm(String term) async {
    await StorageService.saveSearchTerm(term);
    await _loadSearchHistory();
  }

  Future<void> _clearSearchHistory() async {
    await StorageService.clearSearchHistory();
    setState(() {
      _searchHistory = [];
    });
  }

  void _performSearch(String query) {
    if (query.trim().isEmpty) return;

    setState(() {
      _showHistory = false;
    });

    context.read<BookProvider>().searchBooks(query);
    _saveSearchTerm(query);
  }

  void _clearSearch() {
    _searchController.clear();
    setState(() {
      _showHistory = true;
    });
    context.read<BookProvider>().clearFilters();
  }

  Future<void> _addToCart(BookModel book) async {
    try {
      final cartProvider = context.read<CartProvider>();
      await cartProvider.addItem(book);

      if (mounted) {
        SuccessSnackBar.show(context, AppStrings.addToCartSuccess);
      }
    } catch (e) {
      if (mounted) {
        ErrorSnackBar.show(context, e.toString());
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.scaffoldBackground,
      appBar: AppBar(
        title: TextField(
          controller: _searchController,
          focusNode: _searchFocusNode,
          decoration: InputDecoration(
            hintText: 'Tìm kiếm sách, tác giả...',
            border: InputBorder.none,
            suffixIcon:
                _searchController.text.isNotEmpty
                    ? IconButton(
                      icon: const Icon(Icons.clear),
                      onPressed: _clearSearch,
                    )
                    : null,
          ),
          textInputAction: TextInputAction.search,
          onSubmitted: _performSearch,
          onChanged: (value) {
            setState(() {
              _showHistory = value.isEmpty;
            });
          },
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.search),
            onPressed: () => _performSearch(_searchController.text),
          ),
        ],
      ),
      body: _showHistory ? _buildSearchHistory() : _buildSearchResults(),
    );
  }

  Widget _buildSearchHistory() {
    if (_searchHistory.isEmpty) {
      return const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.search, size: 64, color: AppColors.secondaryText),
            SizedBox(height: 16),
            Text(
              'Nhập từ khóa để tìm kiếm sách',
              style: TextStyle(fontSize: 16, color: AppColors.secondaryText),
            ),
          ],
        ),
      );
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Tìm kiếm gần đây',
                style: Theme.of(
                  context,
                ).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
              ),
              TextButton(
                onPressed: _clearSearchHistory,
                child: const Text('Xóa tất cả'),
              ),
            ],
          ),
        ),
        Expanded(
          child: ListView.builder(
            itemCount: _searchHistory.length,
            itemBuilder: (context, index) {
              final term = _searchHistory[index];
              return ListTile(
                leading: const Icon(
                  Icons.history,
                  color: AppColors.secondaryText,
                ),
                title: Text(term),
                trailing: IconButton(
                  icon: const Icon(
                    Icons.north_west,
                    color: AppColors.secondaryText,
                  ),
                  onPressed: () {
                    _searchController.text = term;
                    _performSearch(term);
                  },
                ),
                onTap: () {
                  _searchController.text = term;
                  _performSearch(term);
                },
              );
            },
          ),
        ),
      ],
    );
  }

  Widget _buildSearchResults() {
    return Consumer<BookProvider>(
      builder: (context, bookProvider, child) {
        if (bookProvider.isLoading) {
          return const LoadingWidget(message: 'Đang tìm kiếm...');
        }

        if (bookProvider.hasError) {
          return ErrorDisplayWidget(
            message: bookProvider.errorMessage ?? AppStrings.unknownError,
            onRetry: () => _performSearch(_searchController.text),
          );
        }

        final results = bookProvider.filteredBooks;
        if (results.isEmpty) {
          return Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                const Icon(
                  Icons.search_off,
                  size: 64,
                  color: AppColors.secondaryText,
                ),
                const SizedBox(height: 16),
                Text(
                  'Không tìm thấy kết quả cho "${_searchController.text}"',
                  style: const TextStyle(
                    fontSize: 16,
                    color: AppColors.secondaryText,
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 8),
                const Text(
                  'Hãy thử với từ khóa khác',
                  style: TextStyle(color: AppColors.secondaryText),
                ),
              ],
            ),
          );
        }

        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Padding(
              padding: const EdgeInsets.all(16),
              child: Text(
                'Tìm thấy ${results.length} kết quả cho "${_searchController.text}"',
                style: Theme.of(
                  context,
                ).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
              ),
            ),
            Expanded(
              child: GridView.builder(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 2,
                  childAspectRatio: 0.65,
                  crossAxisSpacing: 12,
                  mainAxisSpacing: 12,
                ),
                itemCount: results.length,
                itemBuilder: (context, index) {
                  final book = results[index];
                  return BookCardWidget(
                    book: book,
                    showAddToCart: true,
                    onTap: () => context.go(AppRoutes.bookDetailPath(book.id)),
                    onAddToCart: () => _addToCart(book),
                  );
                },
              ),
            ),
          ],
        );
      },
    );
  }
}
