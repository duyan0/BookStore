import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import 'package:fl_chart/fl_chart.dart';
import '../../../core/constants/app_colors.dart';
import '../../../core/constants/app_strings.dart';
import '../../providers/auth_provider.dart';
import '../../providers/book_provider.dart';
import '../../providers/order_provider.dart';
import '../../routes/app_routes.dart';

class AdminMainPage extends StatefulWidget {
  const AdminMainPage({super.key});

  @override
  State<AdminMainPage> createState() => _AdminMainPageState();
}

class _AdminMainPageState extends State<AdminMainPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadStatistics();
    });
  }

  Future<void> _loadStatistics() async {
    final bookProvider = context.read<BookProvider>();
    final orderProvider = context.read<OrderProvider>();

    await Future.wait([
      if (bookProvider.books.isEmpty) bookProvider.loadBooks(),
      orderProvider.loadOrderStatistics(),
    ]);
  }

  Future<void> _logout(BuildContext context) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder:
          (context) => AlertDialog(
            title: const Text('Xác nhận đăng xuất'),
            content: const Text('Bạn có chắc chắn muốn đăng xuất?'),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(false),
                child: const Text(AppStrings.cancel),
              ),
              ElevatedButton(
                onPressed: () => Navigator.of(context).pop(true),
                child: const Text(AppStrings.logout),
              ),
            ],
          ),
    );

    if (confirmed == true && context.mounted) {
      await context.read<AuthProvider>().logout();
      if (context.mounted) {
        context.go(AppRoutes.login);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.scaffoldBackground,
      appBar: AppBar(
        title: const Text('Admin Dashboard'),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () => _logout(context),
          ),
        ],
      ),
      drawer: Drawer(
        child: ListView(
          padding: EdgeInsets.zero,
          children: [
            Consumer<AuthProvider>(
              builder: (context, authProvider, child) {
                final user = authProvider.user;
                return UserAccountsDrawerHeader(
                  accountName: Text(user?.fullName ?? 'Admin'),
                  accountEmail: Text(user?.email ?? ''),
                  currentAccountPicture: CircleAvatar(
                    backgroundColor: AppColors.white,
                    child: Text(
                      user?.firstName.substring(0, 1).toUpperCase() ?? 'A',
                      style: const TextStyle(
                        fontSize: 24,
                        fontWeight: FontWeight.bold,
                        color: AppColors.primary,
                      ),
                    ),
                  ),
                  decoration: const BoxDecoration(color: AppColors.primary),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.dashboard),
              title: const Text('Dashboard'),
              onTap: () {
                Navigator.pop(context);
                context.go(AppRoutes.adminHome);
              },
            ),
            ListTile(
              leading: const Icon(Icons.book),
              title: const Text('Quản lý sách'),
              onTap: () {
                Navigator.pop(context);
                context.go(AppRoutes.adminBooks);
              },
            ),
            ListTile(
              leading: const Icon(Icons.person),
              title: const Text('Quản lý tác giả'),
              onTap: () {
                Navigator.pop(context);
                context.go(AppRoutes.adminAuthors);
              },
            ),
            ListTile(
              leading: const Icon(Icons.category),
              title: const Text('Quản lý thể loại'),
              onTap: () {
                Navigator.pop(context);
                context.go(AppRoutes.adminCategories);
              },
            ),
            ListTile(
              leading: const Icon(Icons.shopping_cart),
              title: const Text('Quản lý đơn hàng'),
              onTap: () {
                Navigator.pop(context);
                context.go(AppRoutes.adminOrders);
              },
            ),
            const Divider(),
            ListTile(
              leading: const Icon(Icons.logout),
              title: const Text(AppStrings.logout),
              onTap: () {
                Navigator.pop(context);
                _logout(context);
              },
            ),
          ],
        ),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Chào mừng đến với Admin Dashboard',
              style: Theme.of(
                context,
              ).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 24),

            // Quick Stats Cards
            Consumer2<BookProvider, OrderProvider>(
              builder: (context, bookProvider, orderProvider, child) {
                return GridView.count(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  crossAxisCount: 2,
                  crossAxisSpacing: 16,
                  mainAxisSpacing: 16,
                  children: [
                    _buildStatCard(
                      context,
                      title: 'Tổng sách',
                      value: '${bookProvider.books.length}',
                      icon: Icons.book,
                      color: AppColors.primary,
                      onTap: () => context.go(AppRoutes.adminBooks),
                    ),
                    _buildStatCard(
                      context,
                      title: 'Đơn hàng',
                      value: '${orderProvider.orders.length}',
                      icon: Icons.shopping_cart,
                      color: AppColors.success,
                      onTap: () => context.go(AppRoutes.adminOrders),
                    ),
                    _buildStatCard(
                      context,
                      title: 'Doanh thu',
                      value:
                          '${(orderProvider.getTotalRevenue() / 1000000).toStringAsFixed(1)}M',
                      icon: Icons.attach_money,
                      color: AppColors.warning,
                      onTap: () => context.go(AppRoutes.adminOrders),
                    ),
                    _buildStatCard(
                      context,
                      title: 'Sách bán chạy',
                      value:
                          '${bookProvider.books.where((book) => book.quantity < 10).length}',
                      icon: Icons.trending_up,
                      color: AppColors.info,
                      onTap: () => context.go(AppRoutes.adminBooks),
                    ),
                  ],
                );
              },
            ),
            const SizedBox(height: 24),

            // Charts Section
            Consumer<OrderProvider>(
              builder: (context, orderProvider, child) {
                if (orderProvider.orders.isEmpty) {
                  return const SizedBox.shrink();
                }

                return Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Thống kê đơn hàng',
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 16),
                    _buildOrderStatusChart(context, orderProvider),
                  ],
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatCard(
    BuildContext context, {
    required String title,
    required String value,
    required IconData icon,
    required Color color,
    required VoidCallback onTap,
  }) {
    return Card(
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(icon, size: 48, color: color),
              const SizedBox(height: 12),
              Text(
                value,
                style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: color,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                title,
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: AppColors.secondaryText,
                ),
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildOrderStatusChart(
    BuildContext context,
    OrderProvider orderProvider,
  ) {
    final statusCounts = orderProvider.getOrderCountsByStatus();

    if (statusCounts.isEmpty) {
      return const SizedBox.shrink();
    }

    final pieChartData =
        statusCounts.entries.map((entry) {
          Color color;
          switch (entry.key) {
            case 'pending':
              color = AppColors.warning;
              break;
            case 'processing':
              color = AppColors.info;
              break;
            case 'shipped':
              color = AppColors.primary;
              break;
            case 'delivered':
              color = AppColors.success;
              break;
            case 'cancelled':
              color = AppColors.error;
              break;
            default:
              color = AppColors.secondaryText;
          }

          return PieChartSectionData(
            color: color,
            value: entry.value.toDouble(),
            title: '${entry.value}',
            radius: 60,
            titleStyle: const TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.bold,
              color: AppColors.white,
            ),
          );
        }).toList();

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            SizedBox(
              height: 200,
              child: PieChart(
                PieChartData(
                  sections: pieChartData,
                  centerSpaceRadius: 40,
                  sectionsSpace: 2,
                ),
              ),
            ),
            const SizedBox(height: 16),
            Wrap(
              spacing: 16,
              runSpacing: 8,
              children:
                  statusCounts.entries.map((entry) {
                    Color color;
                    String displayName;
                    switch (entry.key) {
                      case 'pending':
                        color = AppColors.warning;
                        displayName = 'Chờ xử lý';
                        break;
                      case 'processing':
                        color = AppColors.info;
                        displayName = 'Đang xử lý';
                        break;
                      case 'shipped':
                        color = AppColors.primary;
                        displayName = 'Đã gửi';
                        break;
                      case 'delivered':
                        color = AppColors.success;
                        displayName = 'Đã giao';
                        break;
                      case 'cancelled':
                        color = AppColors.error;
                        displayName = 'Đã hủy';
                        break;
                      default:
                        color = AppColors.secondaryText;
                        displayName = entry.key;
                    }

                    return Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Container(
                          width: 12,
                          height: 12,
                          decoration: BoxDecoration(
                            color: color,
                            shape: BoxShape.circle,
                          ),
                        ),
                        const SizedBox(width: 4),
                        Text(
                          '$displayName (${entry.value})',
                          style: Theme.of(context).textTheme.bodySmall,
                        ),
                      ],
                    );
                  }).toList(),
            ),
          ],
        ),
      ),
    );
  }
}
