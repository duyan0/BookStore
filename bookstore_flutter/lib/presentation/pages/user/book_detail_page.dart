import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:cached_network_image/cached_network_image.dart';
import 'package:go_router/go_router.dart';
import '../../../core/constants/app_colors.dart';
import '../../../core/constants/app_strings.dart';
import '../../../core/utils/image_utils.dart';
import '../../providers/book_provider.dart';
import '../../providers/cart_provider.dart';
import '../../widgets/loading_widget.dart';
import '../../widgets/error_widget.dart';
import '../../widgets/price_widget.dart';
import '../../../data/models/book_model.dart';

class BookDetailPage extends StatefulWidget {
  final int bookId;

  const BookDetailPage({super.key, required this.bookId});

  @override
  State<BookDetailPage> createState() => _BookDetailPageState();
}

class _BookDetailPageState extends State<BookDetailPage> {
  int _quantity = 1;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<BookProvider>().loadBookById(widget.bookId);
    });
  }

  Future<void> _addToCart(BookModel book) async {
    try {
      final cartProvider = context.read<CartProvider>();
      await cartProvider.addItem(book, quantity: _quantity);

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
      body: Consumer<BookProvider>(
        builder: (context, bookProvider, child) {
          if (bookProvider.isLoading) {
            return const Scaffold(
              body: LoadingWidget(message: 'Đang tải thông tin sách...'),
            );
          }

          if (bookProvider.hasError) {
            return Scaffold(
              appBar: AppBar(
                leading: IconButton(
                  icon: const Icon(Icons.arrow_back),
                  onPressed: () => context.pop(),
                ),
              ),
              body: ErrorDisplayWidget(
                message: bookProvider.errorMessage ?? AppStrings.unknownError,
                onRetry: () => bookProvider.loadBookById(widget.bookId),
              ),
            );
          }

          final book = bookProvider.selectedBook;
          if (book == null) {
            return Scaffold(
              appBar: AppBar(
                leading: IconButton(
                  icon: const Icon(Icons.arrow_back),
                  onPressed: () => context.pop(),
                ),
              ),
              body: const ErrorDisplayWidget(
                message: 'Không tìm thấy thông tin sách',
              ),
            );
          }

          return CustomScrollView(
            slivers: [
              // App Bar with Book Image
              SliverAppBar(
                expandedHeight: 300,
                pinned: true,
                leading: IconButton(
                  icon: const Icon(Icons.arrow_back, color: AppColors.white),
                  onPressed: () {
                    // Always go back to user home to avoid navigation stack issues
                    context.go('/user');
                  },
                ),
                backgroundColor: AppColors.primary,
                flexibleSpace: FlexibleSpaceBar(
                  background: CachedNetworkImage(
                    imageUrl: ImageUtils.getFullImageUrl(book.imageUrl),
                    fit: BoxFit.cover,
                    placeholder:
                        (context, url) => Container(
                          color: AppColors.lightGray,
                          child: const Center(
                            child: CircularProgressIndicator(),
                          ),
                        ),
                    errorWidget:
                        (context, url, error) => Container(
                          color: AppColors.lightGray,
                          child: const Center(
                            child: Icon(
                              Icons.book,
                              size: 64,
                              color: AppColors.secondaryText,
                            ),
                          ),
                        ),
                  ),
                ),
              ),

              // Book Details
              SliverToBoxAdapter(
                child: Container(
                  color: AppColors.white,
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Title and Author
                      Text(
                        book.title,
                        style: Theme.of(context).textTheme.headlineSmall
                            ?.copyWith(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        'Tác giả: ${book.authorName}',
                        style: Theme.of(context).textTheme.titleMedium
                            ?.copyWith(color: AppColors.secondaryText),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Thể loại: ${book.categoryName}',
                        style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                          color: AppColors.secondaryText,
                        ),
                      ),
                      const SizedBox(height: 16),

                      // Price
                      PriceWidget(
                        price: book.effectivePrice,
                        originalPrice:
                            book.hasDiscount ? book.originalPrice : null,
                        priceStyle: Theme.of(
                          context,
                        ).textTheme.headlineSmall?.copyWith(
                          fontWeight: FontWeight.bold,
                          color:
                              book.hasDiscount
                                  ? AppColors.discountRed
                                  : AppColors.primaryText,
                        ),
                      ),
                      const SizedBox(height: 16),

                      // Stock Status
                      Row(
                        children: [
                          Icon(
                            book.isInStock ? Icons.check_circle : Icons.cancel,
                            color:
                                book.isInStock
                                    ? AppColors.success
                                    : AppColors.error,
                            size: 20,
                          ),
                          const SizedBox(width: 8),
                          Text(
                            book.isInStock
                                ? 'Còn hàng (${book.quantity} cuốn)'
                                : AppStrings.outOfStock,
                            style: Theme.of(
                              context,
                            ).textTheme.bodyMedium?.copyWith(
                              color:
                                  book.isInStock
                                      ? AppColors.success
                                      : AppColors.error,
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 24),

                      // Book Info
                      _buildInfoSection(context, book),
                      const SizedBox(height: 24),

                      // Description
                      if (book.description.isNotEmpty) ...[
                        Text(
                          'Mô tả',
                          style: Theme.of(context).textTheme.titleLarge
                              ?.copyWith(fontWeight: FontWeight.bold),
                        ),
                        const SizedBox(height: 12),
                        Text(
                          book.description,
                          style: Theme.of(context).textTheme.bodyMedium,
                        ),
                        const SizedBox(height: 24),
                      ],
                    ],
                  ),
                ),
              ),
            ],
          );
        },
      ),
      bottomNavigationBar: Consumer<BookProvider>(
        builder: (context, bookProvider, child) {
          final book = bookProvider.selectedBook;
          if (book == null || !book.isInStock) {
            return const SizedBox.shrink();
          }

          return Container(
            padding: const EdgeInsets.all(16),
            decoration: const BoxDecoration(
              color: AppColors.white,
              boxShadow: [
                BoxShadow(
                  color: Colors.black12,
                  blurRadius: 4,
                  offset: Offset(0, -2),
                ),
              ],
            ),
            child: SafeArea(
              child: Row(
                children: [
                  // Quantity Selector
                  Container(
                    decoration: BoxDecoration(
                      border: Border.all(color: AppColors.borderGray),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        IconButton(
                          onPressed:
                              _quantity > 1
                                  ? () {
                                    setState(() {
                                      _quantity--;
                                    });
                                  }
                                  : null,
                          icon: const Icon(Icons.remove),
                          iconSize: 20,
                        ),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 16),
                          child: Text(
                            '$_quantity',
                            style: Theme.of(context).textTheme.titleMedium
                                ?.copyWith(fontWeight: FontWeight.bold),
                          ),
                        ),
                        IconButton(
                          onPressed:
                              _quantity < book.quantity
                                  ? () {
                                    setState(() {
                                      _quantity++;
                                    });
                                  }
                                  : null,
                          icon: const Icon(Icons.add),
                          iconSize: 20,
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(width: 16),

                  // Add to Cart Button
                  Expanded(
                    child: ElevatedButton.icon(
                      onPressed: () => _addToCart(book),
                      icon: const Icon(Icons.shopping_cart),
                      label: const Text(AppStrings.addToCart),
                      style: ElevatedButton.styleFrom(
                        padding: const EdgeInsets.symmetric(vertical: 16),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildInfoSection(BuildContext context, BookModel book) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.lightGray,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Column(
        children: [
          _buildInfoRow(context, 'ISBN', book.isbn),
          const Divider(height: 16),
          _buildInfoRow(context, 'Nhà xuất bản', book.publisher),
          if (book.publicationYear != null) ...[
            const Divider(height: 16),
            _buildInfoRow(context, 'Năm xuất bản', '${book.publicationYear}'),
          ],
        ],
      ),
    );
  }

  Widget _buildInfoRow(BuildContext context, String label, String value) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 120,
          child: Text(
            label,
            style: Theme.of(
              context,
            ).textTheme.bodyMedium?.copyWith(fontWeight: FontWeight.w500),
          ),
        ),
        Expanded(
          child: Text(value, style: Theme.of(context).textTheme.bodyMedium),
        ),
      ],
    );
  }
}
