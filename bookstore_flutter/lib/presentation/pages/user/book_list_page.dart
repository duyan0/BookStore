import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import '../../../core/constants/app_colors.dart';
import '../../../core/constants/app_strings.dart';
import '../../../data/models/book_model.dart';
import '../../providers/book_provider.dart';
import '../../providers/category_provider.dart';
import '../../providers/cart_provider.dart';
import '../../widgets/loading_widget.dart';
import '../../widgets/error_widget.dart';
import '../../widgets/book_card_widget.dart';
import '../../widgets/category_chip_widget.dart';
import '../../routes/app_routes.dart';

class BookListPage extends StatefulWidget {
  const BookListPage({super.key});

  @override
  State<BookListPage> createState() => _BookListPageState();
}

class _BookListPageState extends State<BookListPage> {
  final ScrollController _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadData();
    });
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  Future<void> _loadData() async {
    final bookProvider = context.read<BookProvider>();
    final categoryProvider = context.read<CategoryProvider>();

    await Future.wait([
      if (bookProvider.books.isEmpty) bookProvider.loadBooks(),
      if (categoryProvider.categories.isEmpty)
        categoryProvider.loadCategories(),
    ]);
  }

  Future<void> _onRefresh() async {
    final bookProvider = context.read<BookProvider>();
    await bookProvider.refreshBooks();
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
        title: const Text(AppStrings.books),
        actions: [
          IconButton(
            icon: const Icon(Icons.search),
            onPressed: () => context.go(AppRoutes.search),
          ),
        ],
      ),
      body: Column(
        children: [
          // Category Filter
          Consumer<CategoryProvider>(
            builder: (context, categoryProvider, child) {
              if (categoryProvider.categories.isEmpty) {
                return const SizedBox.shrink();
              }

              return Consumer<BookProvider>(
                builder: (context, bookProvider, child) {
                  return Container(
                    color: AppColors.white,
                    padding: const EdgeInsets.symmetric(vertical: 12),
                    child: CategoryListWidget(
                      categories: categoryProvider.categories,
                      selectedCategoryId: bookProvider.selectedCategoryId,
                      onCategorySelected: (category) {
                        if (category == null) {
                          bookProvider.clearFilters();
                        } else {
                          bookProvider.filterByCategory(category.id);
                        }
                      },
                    ),
                  );
                },
              );
            },
          ),

          // Books Grid
          Expanded(
            child: Consumer<BookProvider>(
              builder: (context, bookProvider, child) {
                if (bookProvider.isLoading &&
                    bookProvider.filteredBooks.isEmpty) {
                  return const LoadingWidget(message: 'Đang tải sách...');
                }

                if (bookProvider.hasError) {
                  return ErrorDisplayWidget(
                    message:
                        bookProvider.errorMessage ?? AppStrings.unknownError,
                    onRetry: () => bookProvider.loadBooks(),
                  );
                }

                if (bookProvider.isEmpty) {
                  return const Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          Icons.book_outlined,
                          size: 64,
                          color: AppColors.secondaryText,
                        ),
                        SizedBox(height: 16),
                        Text(
                          'Không có sách nào',
                          style: TextStyle(
                            fontSize: 16,
                            color: AppColors.secondaryText,
                          ),
                        ),
                      ],
                    ),
                  );
                }

                return RefreshIndicator(
                  onRefresh: _onRefresh,
                  child: GridView.builder(
                    controller: _scrollController,
                    padding: const EdgeInsets.all(16),
                    gridDelegate:
                        const SliverGridDelegateWithFixedCrossAxisCount(
                          crossAxisCount: 2,
                          childAspectRatio: 0.7,
                          crossAxisSpacing: 8,
                          mainAxisSpacing: 12,
                        ),
                    itemCount: bookProvider.filteredBooks.length,
                    itemBuilder: (context, index) {
                      final book = bookProvider.filteredBooks[index];
                      return BookCardWidget(
                        book: book,
                        showAddToCart: true,
                        onTap:
                            () => context.go(AppRoutes.bookDetailPath(book.id)),
                        onAddToCart: () => _addToCart(book),
                      );
                    },
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}
