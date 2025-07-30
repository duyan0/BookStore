import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import '../pages/auth/login_page.dart';
import '../pages/user/user_main_page.dart';
import '../pages/user/book_list_page.dart';
import '../pages/user/book_detail_page.dart';
import '../pages/user/search_page.dart';
import '../pages/user/cart_page.dart';
import '../pages/user/checkout_page.dart';
import '../pages/user/order_history_page.dart';
import '../pages/user/order_detail_page.dart';
import '../pages/user/profile_page.dart';
import '../pages/admin/admin_main_page.dart';
import '../pages/admin/admin_books_page.dart';
import '../pages/admin/admin_authors_page.dart';
import '../pages/admin/admin_categories_page.dart';
import '../pages/admin/admin_orders_page.dart';
import 'app_routes.dart';

class AppRouter {
  static GoRouter createRouter({AuthProvider? authProvider}) {
    return GoRouter(
      initialLocation: AppRoutes.login,
      refreshListenable: authProvider,
      redirect: (context, state) {
        final authProvider = context.read<AuthProvider>();
        final isLoggedIn = authProvider.isAuthenticated;
        final isAdmin = authProvider.isAdmin;
        final isLoginRoute = state.matchedLocation == AppRoutes.login;

        // Debug logging
        print(
          'Router redirect: isLoggedIn=$isLoggedIn, isAdmin=$isAdmin, location=${state.matchedLocation}',
        );

        // If not logged in and not on login page, redirect to login
        if (!isLoggedIn && !isLoginRoute) {
          return AppRoutes.login;
        }

        // If logged in and on login page, redirect based on role
        if (isLoggedIn && isLoginRoute) {
          return isAdmin ? AppRoutes.adminHome : AppRoutes.userHome;
        }

        // Role-based access control
        if (isLoggedIn) {
          final isAdminRoute = state.matchedLocation.startsWith('/admin');
          final isUserRoute = state.matchedLocation.startsWith('/user');

          // Admin trying to access user routes
          if (isAdmin && isUserRoute) {
            return AppRoutes.adminHome;
          }

          // User trying to access admin routes
          if (!isAdmin && isAdminRoute) {
            return AppRoutes.userHome;
          }
        }

        return null; // No redirect needed
      },
      routes: [
        // Auth routes
        GoRoute(
          path: AppRoutes.login,
          builder: (context, state) => const LoginPage(),
        ),

        // User routes
        GoRoute(
          path: AppRoutes.userHome,
          builder: (context, state) => const UserMainPage(),
        ),
        GoRoute(
          path: AppRoutes.bookList,
          builder: (context, state) => const BookListPage(),
        ),
        GoRoute(
          path: AppRoutes.bookDetail,
          builder: (context, state) {
            final id = int.parse(state.pathParameters['id']!);
            return BookDetailPage(bookId: id);
          },
        ),
        GoRoute(
          path: AppRoutes.search,
          builder: (context, state) => const SearchPage(),
        ),
        GoRoute(
          path: AppRoutes.cart,
          builder: (context, state) => const CartPage(),
        ),
        GoRoute(
          path: AppRoutes.checkout,
          builder: (context, state) => const CheckoutPage(),
        ),
        GoRoute(
          path: AppRoutes.orderHistory,
          builder: (context, state) => const OrderHistoryPage(),
        ),
        GoRoute(
          path: AppRoutes.orderDetail,
          builder: (context, state) {
            final id = int.parse(state.pathParameters['id']!);
            return OrderDetailPage(orderId: id);
          },
        ),
        GoRoute(
          path: AppRoutes.profile,
          builder: (context, state) => const ProfilePage(),
        ),

        // Admin routes
        GoRoute(
          path: AppRoutes.adminHome,
          builder: (context, state) => const AdminMainPage(),
        ),
        GoRoute(
          path: AppRoutes.adminBooks,
          builder: (context, state) => const AdminBooksPage(),
        ),
        GoRoute(
          path: AppRoutes.adminAuthors,
          builder: (context, state) => const AdminAuthorsPage(),
        ),
        GoRoute(
          path: AppRoutes.adminCategories,
          builder: (context, state) => const AdminCategoriesPage(),
        ),
        GoRoute(
          path: AppRoutes.adminOrders,
          builder: (context, state) => const AdminOrdersPage(),
        ),
      ],
      errorBuilder:
          (context, state) => Scaffold(
            body: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.error_outline, size: 64, color: Colors.red),
                  const SizedBox(height: 16),
                  Text(
                    'Page not found: ${state.matchedLocation}',
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () => context.go(AppRoutes.userHome),
                    child: const Text('Go Home'),
                  ),
                ],
              ),
            ),
          ),
    );
  }
}
