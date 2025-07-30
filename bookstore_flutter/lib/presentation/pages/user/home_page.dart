import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import '../../../core/constants/app_colors.dart';
import '../../../core/constants/app_strings.dart';
import '../../providers/auth_provider.dart';
import '../../providers/book_provider.dart';
import '../../providers/category_provider.dart';
import '../../widgets/book_card_widget.dart';
import '../../widgets/category_chip_widget.dart';
import '../../../data/models/book_model.dart';
import '../../routes/app_routes.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
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
      bookProvider.loadBooks(),
      categoryProvider.loadCategories(),
    ]);
  }

  Future<void> _onRefresh() async {
    await _loadData();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.scaffoldBackground,
      body: SafeArea(
        child: RefreshIndicator(
          onRefresh: _onRefresh,
          child: CustomScrollView(
            slivers: [
              // App Bar
              SliverAppBar(
                floating: true,
                backgroundColor: AppColors.white,
                elevation: 1,
                title: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      AppStrings.appName,
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: AppColors.primaryText,
                      ),
                    ),
                    Consumer<AuthProvider>(
                      builder: (context, authProvider, child) {
                        return Text(
                          'Xin chào, ${authProvider.user?.firstName ?? 'Khách hàng'}!',
                          style: Theme.of(context).textTheme.bodySmall
                              ?.copyWith(color: AppColors.secondaryText),
                        );
                      },
                    ),
                  ],
                ),
                actions: [
                  IconButton(
                    icon: const Icon(Icons.notifications_outlined),
                    onPressed: () {
                      // TODO: Implement notifications
                    },
                  ),
                ],
              ),

              // Search Bar
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: GestureDetector(
                    onTap: () => context.go(AppRoutes.search),
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 12,
                      ),
                      decoration: BoxDecoration(
                        color: AppColors.lightGray,
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(color: AppColors.borderGray),
                      ),
                      child: Row(
                        children: [
                          const Icon(
                            Icons.search,
                            color: AppColors.secondaryText,
                          ),
                          const SizedBox(width: 12),
                          Text(
                            'Tìm kiếm sách...',
                            style: Theme.of(context).textTheme.bodyMedium
                                ?.copyWith(color: AppColors.hintText),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),

              // Categories
              SliverToBoxAdapter(
                child: Consumer<CategoryProvider>(
                  builder: (context, categoryProvider, child) {
                    if (categoryProvider.isLoading) {
                      return const SizedBox(
                        height: 60,
                        child: Center(child: CircularProgressIndicator()),
                      );
                    }

                    if (categoryProvider.hasError) {
                      return const SizedBox.shrink();
                    }

                    final topCategories = categoryProvider.getTopCategories();
                    if (topCategories.isEmpty) {
                      return const SizedBox.shrink();
                    }

                    return Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 16.0),
                          child: Text(
                            'Thể loại',
                            style: Theme.of(context).textTheme.titleLarge
                                ?.copyWith(fontWeight: FontWeight.bold),
                          ),
                        ),
                        const SizedBox(height: 12),
                        SizedBox(
                          height: 40,
                          child: ListView.builder(
                            scrollDirection: Axis.horizontal,
                            padding: const EdgeInsets.symmetric(horizontal: 16),
                            itemCount: topCategories.length,
                            itemBuilder: (context, index) {
                              final category = topCategories[index];
                              return Padding(
                                padding: const EdgeInsets.only(right: 8),
                                child: CategoryChipWidget(
                                  category: category,
                                  onTap: () {
                                    context.go(AppRoutes.bookList);
                                    // TODO: Filter by category
                                  },
                                ),
                              );
                            },
                          ),
                        ),
                        const SizedBox(height: 24),
                      ],
                    );
                  },
                ),
              ),

              // Featured Books
              SliverToBoxAdapter(
                child: Consumer<BookProvider>(
                  builder: (context, bookProvider, child) {
                    return _buildBookSection(
                      context,
                      title: 'Sách nổi bật',
                      books: bookProvider.featuredBooks,
                      isLoading: bookProvider.isLoading,
                      hasError: bookProvider.hasError,
                      onSeeAll: () => context.go(AppRoutes.bookList),
                    );
                  },
                ),
              ),

              // Discounted Books
              SliverToBoxAdapter(
                child: Consumer<BookProvider>(
                  builder: (context, bookProvider, child) {
                    if (bookProvider.discountedBooks.isEmpty) {
                      return const SizedBox.shrink();
                    }

                    return _buildBookSection(
                      context,
                      title: 'Sách khuyến mãi',
                      books: bookProvider.discountedBooks,
                      isLoading: bookProvider.isLoading,
                      hasError: bookProvider.hasError,
                      onSeeAll: () => context.go(AppRoutes.bookList),
                    );
                  },
                ),
              ),

              // Bottom padding
              const SliverToBoxAdapter(child: SizedBox(height: 24)),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildBookSection(
    BuildContext context, {
    required String title,
    required List<BookModel> books,
    required bool isLoading,
    required bool hasError,
    VoidCallback? onSeeAll,
  }) {
    if (isLoading) {
      return const SizedBox(
        height: 200,
        child: Center(child: CircularProgressIndicator()),
      );
    }

    if (hasError || books.isEmpty) {
      return const SizedBox.shrink();
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16.0),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                title,
                style: Theme.of(
                  context,
                ).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
              ),
              if (onSeeAll != null)
                TextButton(
                  onPressed: onSeeAll,
                  child: const Text('Xem tất cả'),
                ),
            ],
          ),
        ),
        const SizedBox(height: 12),
        SizedBox(
          height: 280,
          child: ListView.builder(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 16),
            itemCount: books.length,
            itemBuilder: (context, index) {
              final book = books[index];
              return Padding(
                padding: const EdgeInsets.only(right: 12),
                child: SizedBox(
                  width: 160,
                  child: BookCardWidget(
                    book: book,
                    onTap: () => context.go(AppRoutes.bookDetailPath(book.id)),
                  ),
                ),
              );
            },
          ),
        ),
        const SizedBox(height: 24),
      ],
    );
  }
}
