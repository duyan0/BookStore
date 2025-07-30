import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:cached_network_image/cached_network_image.dart';
import 'package:go_router/go_router.dart';
import '../../../core/constants/app_colors.dart';
import '../../../core/constants/app_strings.dart';
import '../../../core/utils/image_utils.dart';
import '../../providers/book_provider.dart';
import '../../providers/category_provider.dart';
import '../../widgets/loading_widget.dart';
import '../../widgets/error_widget.dart';
import '../../widgets/price_widget.dart';
import '../../../data/models/book_model.dart';

class AdminBooksPage extends StatefulWidget {
  const AdminBooksPage({super.key});

  @override
  State<AdminBooksPage> createState() => _AdminBooksPageState();
}

class _AdminBooksPageState extends State<AdminBooksPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadData();
    });
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
    await context.read<BookProvider>().refreshBooks();
  }

  Future<void> _deleteBook(BookModel book) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder:
          (context) => AlertDialog(
            title: const Text('Xác nhận xóa'),
            content: Text('Bạn có chắc chắn muốn xóa sách "${book.title}"?'),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(false),
                child: const Text(AppStrings.cancel),
              ),
              ElevatedButton(
                onPressed: () => Navigator.of(context).pop(true),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.error,
                  foregroundColor: AppColors.white,
                ),
                child: const Text('Xóa'),
              ),
            ],
          ),
    );

    if (confirmed == true && mounted) {
      // TODO: Implement delete book API call
      SuccessSnackBar.show(
        context,
        'Tính năng xóa sách sẽ được triển khai sau',
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.scaffoldBackground,
      appBar: AppBar(
        title: const Text(AppStrings.bookManagement),
        elevation: 1,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.go('/admin'),
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.add),
            onPressed: () {
              // TODO: Navigate to create book page
              InfoSnackBar.show(
                context,
                'Tính năng thêm sách sẽ được triển khai sau',
              );
            },
          ),
        ],
      ),
      body: Consumer<BookProvider>(
        builder: (context, bookProvider, child) {
          if (bookProvider.isLoading && bookProvider.books.isEmpty) {
            return const LoadingWidget(message: 'Đang tải danh sách sách...');
          }

          if (bookProvider.hasError) {
            return ErrorDisplayWidget(
              message: bookProvider.errorMessage ?? AppStrings.unknownError,
              onRetry: () => bookProvider.loadBooks(),
            );
          }

          if (bookProvider.isEmpty) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(
                    Icons.book_outlined,
                    size: 64,
                    color: AppColors.secondaryText,
                  ),
                  const SizedBox(height: 16),
                  const Text(
                    'Chưa có sách nào',
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.w500,
                      color: AppColors.secondaryText,
                    ),
                  ),
                  const SizedBox(height: 24),
                  ElevatedButton.icon(
                    onPressed: () {
                      // TODO: Navigate to create book page
                      InfoSnackBar.show(
                        context,
                        'Tính năng thêm sách sẽ được triển khai sau',
                      );
                    },
                    icon: const Icon(Icons.add),
                    label: const Text('Thêm sách mới'),
                  ),
                ],
              ),
            );
          }

          return RefreshIndicator(
            onRefresh: _onRefresh,
            child: ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: bookProvider.books.length,
              itemBuilder: (context, index) {
                final book = bookProvider.books[index];
                return _AdminBookCard(
                  book: book,
                  onEdit: () {
                    // TODO: Navigate to edit book page
                    InfoSnackBar.show(
                      context,
                      'Tính năng sửa sách sẽ được triển khai sau',
                    );
                  },
                  onDelete: () => _deleteBook(book),
                );
              },
            ),
          );
        },
      ),
    );
  }
}

class _AdminBookCard extends StatelessWidget {
  final BookModel book;
  final VoidCallback? onEdit;
  final VoidCallback? onDelete;

  const _AdminBookCard({required this.book, this.onEdit, this.onDelete});

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          children: [
            // Book Image
            ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: CachedNetworkImage(
                imageUrl: ImageUtils.getFullImageUrl(book.imageUrl),
                width: 60,
                height: 80,
                fit: BoxFit.cover,
                placeholder:
                    (context, url) => Container(
                      width: 60,
                      height: 80,
                      color: AppColors.lightGray,
                      child: const Icon(
                        Icons.book,
                        color: AppColors.secondaryText,
                      ),
                    ),
                errorWidget:
                    (context, url, error) => Container(
                      width: 60,
                      height: 80,
                      color: AppColors.lightGray,
                      child: const Icon(
                        Icons.book,
                        color: AppColors.secondaryText,
                      ),
                    ),
              ),
            ),
            const SizedBox(width: 16),

            // Book Info
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    book.title,
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Tác giả: ${book.authorName}',
                    style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: AppColors.secondaryText,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Thể loại: ${book.categoryName}',
                    style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: AppColors.secondaryText,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      SimplePriceWidget(
                        price: book.effectivePrice,
                        style: Theme.of(context).textTheme.titleSmall?.copyWith(
                          fontWeight: FontWeight.bold,
                          color:
                              book.hasDiscount
                                  ? AppColors.discountRed
                                  : AppColors.primaryText,
                        ),
                      ),
                      const SizedBox(width: 8),
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 6,
                          vertical: 2,
                        ),
                        decoration: BoxDecoration(
                          color:
                              book.isInStock
                                  ? AppColors.success.withValues(alpha: 0.1)
                                  : AppColors.error.withValues(alpha: 0.1),
                          borderRadius: BorderRadius.circular(4),
                        ),
                        child: Text(
                          book.isInStock
                              ? 'Còn hàng (${book.quantity})'
                              : 'Hết hàng',
                          style: Theme.of(
                            context,
                          ).textTheme.bodySmall?.copyWith(
                            color:
                                book.isInStock
                                    ? AppColors.success
                                    : AppColors.error,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),

            // Actions
            Column(
              children: [
                IconButton(
                  onPressed: onEdit,
                  icon: const Icon(Icons.edit),
                  color: AppColors.primary,
                  tooltip: 'Sửa',
                ),
                IconButton(
                  onPressed: onDelete,
                  icon: const Icon(Icons.delete),
                  color: AppColors.error,
                  tooltip: 'Xóa',
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
