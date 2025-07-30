import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'core/theme/app_theme.dart';
import 'core/constants/app_strings.dart';
import 'core/network/api_client.dart';
import 'data/services/storage_service.dart';
import 'data/services/auth_service.dart';
import 'data/services/book_service.dart';
import 'data/services/author_service.dart';
import 'data/services/category_service.dart';
import 'data/services/order_service.dart';
import 'presentation/providers/auth_provider.dart';
import 'presentation/providers/cart_provider.dart';
import 'presentation/providers/book_provider.dart';
import 'presentation/providers/category_provider.dart';
import 'presentation/providers/author_provider.dart';
import 'presentation/providers/order_provider.dart';

import 'presentation/routes/app_router.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Initialize storage service
  await StorageService.init();

  runApp(const BookStoreApp());
}

class BookStoreApp extends StatelessWidget {
  const BookStoreApp({super.key});

  @override
  Widget build(BuildContext context) {
    // Initialize API client and services
    final apiClient = ApiClient();
    final authService = AuthService(apiClient);
    final bookService = BookService(apiClient);
    final authorService = AuthorService(apiClient);
    final categoryService = CategoryService(apiClient);
    final orderService = OrderService(apiClient);

    return MultiProvider(
      providers: [
        // Services
        Provider<ApiClient>.value(value: apiClient),
        Provider<AuthService>.value(value: authService),
        Provider<BookService>.value(value: bookService),
        Provider<AuthorService>.value(value: authorService),
        Provider<CategoryService>.value(value: categoryService),
        Provider<OrderService>.value(value: orderService),

        // State providers
        ChangeNotifierProvider<AuthProvider>(
          create: (context) => AuthProvider(authService),
        ),
        ChangeNotifierProvider<CartProvider>(
          create: (context) => CartProvider(),
        ),
        ChangeNotifierProvider<BookProvider>(
          create: (context) => BookProvider(bookService),
        ),
        ChangeNotifierProvider<CategoryProvider>(
          create: (context) => CategoryProvider(categoryService),
        ),
        ChangeNotifierProvider<AuthorProvider>(
          create: (context) => AuthorProvider(authorService),
        ),
        ChangeNotifierProvider<OrderProvider>(
          create: (context) => OrderProvider(orderService),
        ),
      ],
      child: Builder(
        builder: (context) {
          final authProvider = context.read<AuthProvider>();
          return MaterialApp.router(
            title: AppStrings.appName,
            theme: AppTheme.lightTheme,
            debugShowCheckedModeBanner: false,
            routerConfig: AppRouter.createRouter(authProvider: authProvider),
          );
        },
      ),
    );
  }
}
