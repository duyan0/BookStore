import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import '../../../core/constants/app_strings.dart';
import '../../../core/constants/app_colors.dart';
import '../../../core/utils/currency_formatter.dart';
import '../../../core/utils/image_utils.dart';
import '../../providers/order_provider.dart';
import '../../providers/auth_provider.dart';
import '../../widgets/loading_widget.dart';
import '../../widgets/error_widget.dart' as custom_error;
import '../../widgets/empty_state_widget.dart';
import '../../../data/models/order_model.dart';

class AdminOrdersPage extends StatefulWidget {
  const AdminOrdersPage({super.key});

  @override
  State<AdminOrdersPage> createState() => _AdminOrdersPageState();
}

class _AdminOrdersPageState extends State<AdminOrdersPage> {
  String _selectedStatus = 'all';
  String _searchQuery = '';
  final TextEditingController _searchController = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadOrders();
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _loadOrders() async {
    final authProvider = context.read<AuthProvider>();
    if (authProvider.isAdmin) {
      await context.read<OrderProvider>().loadAllOrders();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.scaffoldBackground,
      appBar: AppBar(
        title: const Text(AppStrings.orderManagement),
        elevation: 1,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.go('/admin'),
        ),
        actions: [
          IconButton(icon: const Icon(Icons.refresh), onPressed: _loadOrders),
        ],
      ),
      body: Column(
        children: [
          // Search and Filter Section
          Container(
            padding: const EdgeInsets.all(16),
            color: AppColors.white,
            child: Column(
              children: [
                // Search Bar
                TextField(
                  controller: _searchController,
                  decoration: InputDecoration(
                    hintText: 'Tìm kiếm theo mã đơn, tên khách hàng...',
                    prefixIcon: const Icon(Icons.search),
                    suffixIcon:
                        _searchQuery.isNotEmpty
                            ? IconButton(
                              icon: const Icon(Icons.clear),
                              onPressed: () {
                                _searchController.clear();
                                setState(() {
                                  _searchQuery = '';
                                });
                              },
                            )
                            : null,
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                  ),
                  onChanged: (value) {
                    setState(() {
                      _searchQuery = value;
                    });
                  },
                ),
                const SizedBox(height: 12),

                // Status Filter
                SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  child: Row(
                    children: [
                      _buildStatusChip('all', 'Tất cả'),
                      const SizedBox(width: 8),
                      _buildStatusChip('pending', 'Chờ xử lý'),
                      const SizedBox(width: 8),
                      _buildStatusChip('processing', 'Đang xử lý'),
                      const SizedBox(width: 8),
                      _buildStatusChip('shipped', 'Đã gửi'),
                      const SizedBox(width: 8),
                      _buildStatusChip('delivered', 'Đã giao'),
                      const SizedBox(width: 8),
                      _buildStatusChip('cancelled', 'Đã hủy'),
                    ],
                  ),
                ),
              ],
            ),
          ),

          // Orders List
          Expanded(
            child: Consumer<OrderProvider>(
              builder: (context, orderProvider, child) {
                if (orderProvider.state == OrderLoadingState.loading) {
                  return const LoadingWidget();
                }

                if (orderProvider.state == OrderLoadingState.error) {
                  return custom_error.ErrorDisplayWidget(
                    message: orderProvider.errorMessage ?? 'Có lỗi xảy ra',
                    onRetry: _loadOrders,
                  );
                }

                final filteredOrders = _getFilteredOrders(orderProvider.orders);

                if (filteredOrders.isEmpty) {
                  return EmptyStateWidget(
                    icon: Icons.shopping_cart_outlined,
                    title:
                        _searchQuery.isNotEmpty || _selectedStatus != 'all'
                            ? 'Không tìm thấy đơn hàng'
                            : 'Chưa có đơn hàng nào',
                    subtitle:
                        _searchQuery.isNotEmpty || _selectedStatus != 'all'
                            ? 'Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm'
                            : 'Đơn hàng sẽ hiển thị ở đây khi có khách hàng đặt hàng',
                  );
                }

                return RefreshIndicator(
                  onRefresh: _loadOrders,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: filteredOrders.length,
                    itemBuilder: (context, index) {
                      final order = filteredOrders[index];
                      return _buildOrderCard(order);
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

  Widget _buildStatusChip(String status, String label) {
    final isSelected = _selectedStatus == status;
    return FilterChip(
      label: Text(label),
      selected: isSelected,
      onSelected: (selected) {
        setState(() {
          _selectedStatus = status;
        });
      },
      backgroundColor: AppColors.white,
      selectedColor: AppColors.primary.withValues(alpha: 0.2),
      checkmarkColor: AppColors.primary,
      labelStyle: TextStyle(
        color: isSelected ? AppColors.primary : AppColors.primaryText,
        fontWeight: isSelected ? FontWeight.w600 : FontWeight.normal,
      ),
    );
  }

  List<OrderModel> _getFilteredOrders(List<OrderModel> orders) {
    var filtered = orders;

    // Filter by status
    if (_selectedStatus != 'all') {
      filtered =
          filtered
              .where((order) => order.status.toLowerCase() == _selectedStatus)
              .toList();
    }

    // Filter by search query
    if (_searchQuery.isNotEmpty) {
      final query = _searchQuery.toLowerCase();
      filtered =
          filtered.where((order) {
            return order.id.toString().contains(query) ||
                order.userFullName.toLowerCase().contains(query) ||
                order.userName.toLowerCase().contains(query);
          }).toList();
    }

    // Sort by order date (newest first)
    filtered.sort((a, b) => b.orderDate.compareTo(a.orderDate));

    return filtered;
  }

  Widget _buildOrderCard(OrderModel order) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header Row
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Đơn hàng #${order.id}',
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                _buildStatusBadge(order.status),
              ],
            ),
            const SizedBox(height: 8),

            // Customer Info
            Row(
              children: [
                const Icon(
                  Icons.person,
                  size: 16,
                  color: AppColors.secondaryText,
                ),
                const SizedBox(width: 4),
                Expanded(
                  child: Text(
                    order.userFullName.isNotEmpty
                        ? order.userFullName
                        : order.userName,
                    style: Theme.of(context).textTheme.bodyMedium,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 4),

            // Order Date
            Row(
              children: [
                const Icon(
                  Icons.calendar_today,
                  size: 16,
                  color: AppColors.secondaryText,
                ),
                const SizedBox(width: 4),
                Text(
                  '${order.orderDate.day}/${order.orderDate.month}/${order.orderDate.year}',
                  style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: AppColors.secondaryText,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),

            // Total Amount
            Row(
              children: [
                const Icon(
                  Icons.attach_money,
                  size: 16,
                  color: AppColors.success,
                ),
                const SizedBox(width: 4),
                Text(
                  CurrencyFormatter.formatVNDWithSymbol(order.totalAmount),
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    color: AppColors.success,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),

            // Action Buttons
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                TextButton.icon(
                  onPressed: () => _showOrderDetails(order),
                  icon: const Icon(Icons.visibility, size: 16),
                  label: const Text('Chi tiết'),
                ),
                const SizedBox(width: 8),
                if (_canUpdateStatus(order.status))
                  ElevatedButton.icon(
                    onPressed: () => _showUpdateStatusDialog(order),
                    icon: const Icon(Icons.edit, size: 16),
                    label: const Text('Cập nhật'),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primary,
                      foregroundColor: AppColors.white,
                    ),
                  ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatusBadge(String status) {
    Color backgroundColor;
    Color textColor;
    String displayText;

    switch (status.toLowerCase()) {
      case 'pending':
        backgroundColor = AppColors.warning.withValues(alpha: 0.2);
        textColor = AppColors.warning;
        displayText = 'Chờ xử lý';
        break;
      case 'processing':
        backgroundColor = AppColors.info.withValues(alpha: 0.2);
        textColor = AppColors.info;
        displayText = 'Đang xử lý';
        break;
      case 'shipped':
        backgroundColor = AppColors.primary.withValues(alpha: 0.2);
        textColor = AppColors.primary;
        displayText = 'Đã gửi';
        break;
      case 'delivered':
        backgroundColor = AppColors.success.withValues(alpha: 0.2);
        textColor = AppColors.success;
        displayText = 'Đã giao';
        break;
      case 'cancelled':
        backgroundColor = AppColors.error.withValues(alpha: 0.2);
        textColor = AppColors.error;
        displayText = 'Đã hủy';
        break;
      default:
        backgroundColor = AppColors.secondaryText.withValues(alpha: 0.2);
        textColor = AppColors.secondaryText;
        displayText = status;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: backgroundColor,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        displayText,
        style: TextStyle(
          color: textColor,
          fontSize: 12,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }

  bool _canUpdateStatus(String status) {
    final currentStatus = status.toLowerCase();
    return currentStatus != 'delivered' && currentStatus != 'cancelled';
  }

  void _showOrderDetails(OrderModel order) {
    showDialog(
      context: context,
      builder: (context) => _OrderDetailsDialog(order: order),
    );
  }

  void _showUpdateStatusDialog(OrderModel order) {
    showDialog(
      context: context,
      builder:
          (context) =>
              _UpdateStatusDialog(order: order, onStatusUpdated: _loadOrders),
    );
  }
}

// Order Details Dialog
class _OrderDetailsDialog extends StatelessWidget {
  final OrderModel order;

  const _OrderDetailsDialog({required this.order});

  @override
  Widget build(BuildContext context) {
    return Dialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Container(
        constraints: const BoxConstraints(maxWidth: 500, maxHeight: 600),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Header
            Container(
              padding: const EdgeInsets.all(20),
              decoration: const BoxDecoration(
                color: AppColors.primary,
                borderRadius: BorderRadius.only(
                  topLeft: Radius.circular(16),
                  topRight: Radius.circular(16),
                ),
              ),
              child: Row(
                children: [
                  const Icon(Icons.receipt_long, color: AppColors.white),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Text(
                      'Chi tiết đơn hàng #${order.id}',
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(
                        color: AppColors.white,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                  IconButton(
                    onPressed: () => Navigator.of(context).pop(),
                    icon: const Icon(Icons.close, color: AppColors.white),
                  ),
                ],
              ),
            ),

            // Content
            Flexible(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Customer Info
                    _buildInfoSection(context, 'Thông tin khách hàng', [
                      _buildInfoRow(
                        'Tên:',
                        order.userFullName.isNotEmpty
                            ? order.userFullName
                            : order.userName,
                      ),
                      _buildInfoRow('Username:', order.userName),
                    ]),
                    const SizedBox(height: 16),

                    // Order Info
                    _buildInfoSection(context, 'Thông tin đơn hàng', [
                      _buildInfoRow(
                        'Ngày đặt:',
                        '${order.orderDate.day}/${order.orderDate.month}/${order.orderDate.year}',
                      ),
                      _buildInfoRow('Trạng thái:', order.status),
                      _buildInfoRow(
                        'Phương thức thanh toán:',
                        order.paymentMethod,
                      ),
                      _buildInfoRow(
                        'Địa chỉ giao hàng:',
                        order.shippingAddress,
                      ),
                    ]),
                    const SizedBox(height: 16),

                    // Order Items
                    _buildInfoSection(
                      context,
                      'Sản phẩm đặt hàng',
                      order.orderDetails
                          .map((detail) => _buildOrderItem(context, detail))
                          .toList(),
                    ),
                    const SizedBox(height: 16),

                    // Total
                    Container(
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: AppColors.success.withValues(alpha: 0.1),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            'Tổng cộng:',
                            style: Theme.of(context).textTheme.titleMedium
                                ?.copyWith(fontWeight: FontWeight.bold),
                          ),
                          Text(
                            CurrencyFormatter.formatVNDWithSymbol(
                              order.totalAmount,
                            ),
                            style: Theme.of(
                              context,
                            ).textTheme.titleMedium?.copyWith(
                              color: AppColors.success,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildInfoSection(
    BuildContext context,
    String title,
    List<Widget> children,
  ) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: Theme.of(context).textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.bold,
            color: AppColors.primary,
          ),
        ),
        const SizedBox(height: 8),
        Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: AppColors.scaffoldBackground,
            borderRadius: BorderRadius.circular(8),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: children,
          ),
        ),
      ],
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 120,
            child: Text(
              label,
              style: const TextStyle(
                fontWeight: FontWeight.w500,
                color: AppColors.secondaryText,
              ),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: const TextStyle(color: AppColors.primaryText),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildOrderItem(BuildContext context, OrderDetailModel detail) {
    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        border: Border.all(
          color: AppColors.secondaryText.withValues(alpha: 0.2),
        ),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        children: [
          // Book Image
          Container(
            width: 50,
            height: 50,
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(4),
              color: AppColors.scaffoldBackground,
            ),
            child:
                detail.bookImageUrl.isNotEmpty
                    ? ClipRRect(
                      borderRadius: BorderRadius.circular(4),
                      child: Image.network(
                        ImageUtils.getFullImageUrl(detail.bookImageUrl),
                        fit: BoxFit.cover,
                        errorBuilder: (context, error, stackTrace) {
                          return const Icon(
                            Icons.book,
                            color: AppColors.secondaryText,
                          );
                        },
                      ),
                    )
                    : const Icon(Icons.book, color: AppColors.secondaryText),
          ),
          const SizedBox(width: 12),

          // Book Info
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  detail.bookTitle,
                  style: Theme.of(
                    context,
                  ).textTheme.bodyMedium?.copyWith(fontWeight: FontWeight.w500),
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: 4),
                Text(
                  'Số lượng: ${detail.quantity}',
                  style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: AppColors.secondaryText,
                  ),
                ),
                Text(
                  'Đơn giá: ${CurrencyFormatter.formatVNDWithSymbol(detail.unitPrice)}',
                  style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: AppColors.secondaryText,
                  ),
                ),
              ],
            ),
          ),

          // Total Price
          Text(
            CurrencyFormatter.formatVNDWithSymbol(detail.totalPrice),
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
              fontWeight: FontWeight.bold,
              color: AppColors.success,
            ),
          ),
        ],
      ),
    );
  }
}

// Update Status Dialog
class _UpdateStatusDialog extends StatefulWidget {
  final OrderModel order;
  final VoidCallback onStatusUpdated;

  const _UpdateStatusDialog({
    required this.order,
    required this.onStatusUpdated,
  });

  @override
  State<_UpdateStatusDialog> createState() => _UpdateStatusDialogState();
}

class _UpdateStatusDialogState extends State<_UpdateStatusDialog> {
  String? _selectedStatus;
  bool _isUpdating = false;

  final List<Map<String, String>> _statusOptions = [
    {'value': 'pending', 'label': 'Chờ xử lý'},
    {'value': 'processing', 'label': 'Đang xử lý'},
    {'value': 'shipped', 'label': 'Đã gửi'},
    {'value': 'delivered', 'label': 'Đã giao'},
    {'value': 'cancelled', 'label': 'Đã hủy'},
  ];

  @override
  void initState() {
    super.initState();
    _selectedStatus = widget.order.status.toLowerCase();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      title: Text(
        'Cập nhật trạng thái đơn hàng #${widget.order.id}',
        style: const TextStyle(fontWeight: FontWeight.bold),
      ),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Chọn trạng thái mới:',
            style: TextStyle(fontWeight: FontWeight.w500),
          ),
          const SizedBox(height: 12),
          ..._statusOptions.map((option) {
            return RadioListTile<String>(
              title: Text(option['label']!),
              value: option['value']!,
              groupValue: _selectedStatus,
              onChanged:
                  _isUpdating
                      ? null
                      : (value) {
                        setState(() {
                          _selectedStatus = value;
                        });
                      },
              contentPadding: EdgeInsets.zero,
            );
          }),
        ],
      ),
      actions: [
        TextButton(
          onPressed: _isUpdating ? null : () => Navigator.of(context).pop(),
          child: const Text('Hủy'),
        ),
        ElevatedButton(
          onPressed:
              _isUpdating ||
                      _selectedStatus == widget.order.status.toLowerCase()
                  ? null
                  : _updateStatus,
          style: ElevatedButton.styleFrom(
            backgroundColor: AppColors.primary,
            foregroundColor: AppColors.white,
          ),
          child:
              _isUpdating
                  ? const SizedBox(
                    width: 16,
                    height: 16,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      valueColor: AlwaysStoppedAnimation<Color>(
                        AppColors.white,
                      ),
                    ),
                  )
                  : const Text('Cập nhật'),
        ),
      ],
    );
  }

  Future<void> _updateStatus() async {
    if (_selectedStatus == null) return;

    setState(() {
      _isUpdating = true;
    });

    try {
      final success = await context.read<OrderProvider>().updateOrderStatus(
        widget.order.id,
        _selectedStatus!,
      );

      if (mounted) {
        if (success) {
          Navigator.of(context).pop();
          widget.onStatusUpdated();
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Cập nhật trạng thái thành công'),
              backgroundColor: AppColors.success,
            ),
          );
        } else {
          final errorMessage = context.read<OrderProvider>().errorMessage;
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(errorMessage ?? 'Không thể cập nhật trạng thái'),
              backgroundColor: AppColors.error,
            ),
          );
        }
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Lỗi: ${e.toString()}'),
            backgroundColor: AppColors.error,
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isUpdating = false;
        });
      }
    }
  }
}
