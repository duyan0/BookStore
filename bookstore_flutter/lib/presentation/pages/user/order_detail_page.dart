import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import 'package:cached_network_image/cached_network_image.dart';
import 'package:go_router/go_router.dart';
import '../../../core/constants/app_colors.dart';
import '../../../core/constants/app_strings.dart';
import '../../../core/utils/image_utils.dart';
import '../../providers/order_provider.dart';
import '../../widgets/loading_widget.dart';
import '../../widgets/error_widget.dart';
import '../../widgets/price_widget.dart';
import '../../../data/models/order_model.dart';

class OrderDetailPage extends StatefulWidget {
  final int orderId;

  const OrderDetailPage({super.key, required this.orderId});

  @override
  State<OrderDetailPage> createState() => _OrderDetailPageState();
}

class _OrderDetailPageState extends State<OrderDetailPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<OrderProvider>().loadOrderById(widget.orderId);
    });
  }

  Future<void> _cancelOrder(OrderModel order) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder:
          (context) => AlertDialog(
            title: const Text('Xác nhận hủy đơn hàng'),
            content: Text('Bạn có chắc chắn muốn hủy đơn hàng #${order.id}?'),
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
                child: const Text('Hủy đơn hàng'),
              ),
            ],
          ),
    );

    if (confirmed == true && mounted) {
      final success = await context.read<OrderProvider>().cancelOrder(order.id);
      if (mounted) {
        if (success) {
          SuccessSnackBar.show(context, 'Đã hủy đơn hàng thành công');
        } else {
          final errorMessage = context.read<OrderProvider>().errorMessage;
          ErrorSnackBar.show(context, errorMessage ?? 'Không thể hủy đơn hàng');
        }
      }
    }
  }

  bool _canCancelOrder(OrderModel order) {
    final status = order.status.toLowerCase();
    return status == 'pending' || status == 'processing';
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.scaffoldBackground,
      appBar: AppBar(
        title: Text('Đơn hàng #${widget.orderId}'),
        elevation: 1,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.pop(),
        ),
      ),
      body: Consumer<OrderProvider>(
        builder: (context, orderProvider, child) {
          if (orderProvider.isLoading) {
            return const LoadingWidget(
              message: 'Đang tải chi tiết đơn hàng...',
            );
          }

          if (orderProvider.hasError) {
            return ErrorDisplayWidget(
              message: orderProvider.errorMessage ?? AppStrings.unknownError,
              onRetry: () => orderProvider.loadOrderById(widget.orderId),
            );
          }

          final order = orderProvider.selectedOrder;
          if (order == null) {
            return const ErrorDisplayWidget(
              message: 'Không tìm thấy thông tin đơn hàng',
            );
          }

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Order Status Card
                _buildOrderStatusCard(context, order),
                const SizedBox(height: 16),

                // Order Info Card
                _buildOrderInfoCard(context, order),
                const SizedBox(height: 16),

                // Items Card
                _buildOrderItemsCard(context, order),
                const SizedBox(height: 16),

                // Payment Summary Card
                _buildPaymentSummaryCard(context, order),
                const SizedBox(height: 16),

                // Cancel Button (if applicable)
                if (_canCancelOrder(order))
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: () => _cancelOrder(order),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.error,
                        foregroundColor: AppColors.white,
                        padding: const EdgeInsets.symmetric(vertical: 16),
                      ),
                      child: const Text(
                        'Hủy đơn hàng',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  ),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildOrderStatusCard(BuildContext context, OrderModel order) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Trạng thái đơn hàng',
              style: Theme.of(
                context,
              ).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                _buildStatusChip(context, order.status),
                const Spacer(),
                Text(
                  DateFormat('dd/MM/yyyy HH:mm').format(order.orderDate),
                  style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    color: AppColors.secondaryText,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildOrderInfoCard(BuildContext context, OrderModel order) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Thông tin đơn hàng',
              style: Theme.of(
                context,
              ).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 16),
            _buildInfoRow(context, 'Mã đơn hàng', '#${order.id}'),
            const SizedBox(height: 8),
            _buildInfoRow(context, 'Khách hàng', order.userFullName),
            const SizedBox(height: 8),
            _buildInfoRow(context, 'Địa chỉ giao hàng', order.shippingAddress),
            const SizedBox(height: 8),
            _buildInfoRow(
              context,
              'Phương thức thanh toán',
              order.paymentMethod,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildOrderItemsCard(BuildContext context, OrderModel order) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Sản phẩm (${order.orderDetails.length})',
              style: Theme.of(
                context,
              ).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 16),
            ...order.orderDetails.map(
              (detail) => _buildOrderItem(context, detail),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildOrderItem(BuildContext context, OrderDetailModel detail) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: AppColors.lightGray,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        children: [
          // Book Image
          ClipRRect(
            borderRadius: BorderRadius.circular(6),
            child: CachedNetworkImage(
              imageUrl: ImageUtils.getFullImageUrl(detail.bookImageUrl),
              width: 50,
              height: 60,
              fit: BoxFit.cover,
              placeholder:
                  (context, url) => Container(
                    width: 50,
                    height: 60,
                    color: AppColors.borderGray,
                    child: const Icon(
                      Icons.book,
                      color: AppColors.secondaryText,
                    ),
                  ),
              errorWidget:
                  (context, url, error) => Container(
                    width: 50,
                    height: 60,
                    color: AppColors.borderGray,
                    child: const Icon(
                      Icons.book,
                      color: AppColors.secondaryText,
                    ),
                  ),
            ),
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
                  ).textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w600),
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: 4),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      'SL: ${detail.quantity}',
                      style: Theme.of(context).textTheme.bodySmall?.copyWith(
                        color: AppColors.secondaryText,
                      ),
                    ),
                    SimplePriceWidget(
                      price: detail.unitPrice,
                      style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildPaymentSummaryCard(BuildContext context, OrderModel order) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Tóm tắt thanh toán',
              style: Theme.of(
                context,
              ).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 16),

            // Calculate subtotal from order details
            _buildSummaryRow(
              context,
              'Tạm tính',
              order.orderDetails.fold(
                0.0,
                (sum, detail) => sum + (detail.quantity * detail.unitPrice),
              ),
            ),
            const SizedBox(height: 8),
            _buildSummaryRow(
              context,
              'Phí vận chuyển',
              30000,
            ), // Fixed shipping fee
            const Divider(height: 24),
            _buildSummaryRow(
              context,
              'Tổng cộng',
              order.totalAmount,
              isTotal: true,
            ),
          ],
        ),
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
            ).textTheme.bodyMedium?.copyWith(color: AppColors.secondaryText),
          ),
        ),
        Expanded(
          child: Text(
            value,
            style: Theme.of(
              context,
            ).textTheme.bodyMedium?.copyWith(fontWeight: FontWeight.w500),
          ),
        ),
      ],
    );
  }

  Widget _buildSummaryRow(
    BuildContext context,
    String label,
    double value, {
    bool isTotal = false,
  }) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          label,
          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
            fontWeight: isTotal ? FontWeight.bold : FontWeight.normal,
          ),
        ),
        SimplePriceWidget(
          price: value,
          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
            fontWeight: isTotal ? FontWeight.bold : FontWeight.w600,
            color: isTotal ? AppColors.primary : null,
          ),
        ),
      ],
    );
  }

  Widget _buildStatusChip(BuildContext context, String status) {
    Color backgroundColor;
    Color textColor;
    String displayText;

    switch (status.toLowerCase()) {
      case 'pending':
        backgroundColor = AppColors.warning.withValues(alpha: 0.1);
        textColor = AppColors.warning;
        displayText = 'Chờ xử lý';
        break;
      case 'processing':
        backgroundColor = AppColors.info.withValues(alpha: 0.1);
        textColor = AppColors.info;
        displayText = 'Đang xử lý';
        break;
      case 'shipped':
        backgroundColor = AppColors.primary.withValues(alpha: 0.1);
        textColor = AppColors.primary;
        displayText = 'Đã gửi';
        break;
      case 'delivered':
        backgroundColor = AppColors.success.withValues(alpha: 0.1);
        textColor = AppColors.success;
        displayText = 'Đã giao';
        break;
      case 'cancelled':
        backgroundColor = AppColors.error.withValues(alpha: 0.1);
        textColor = AppColors.error;
        displayText = 'Đã hủy';
        break;
      default:
        backgroundColor = AppColors.lightGray;
        textColor = AppColors.secondaryText;
        displayText = status;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: backgroundColor,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Text(
        displayText,
        style: Theme.of(context).textTheme.bodyMedium?.copyWith(
          color: textColor,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }
}
