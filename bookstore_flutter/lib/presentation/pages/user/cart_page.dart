import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../../core/constants/app_colors.dart';
import '../../../core/constants/app_strings.dart';
import '../../../core/utils/image_utils.dart';
import '../../providers/cart_provider.dart';
import '../../widgets/loading_widget.dart';
import '../../widgets/error_widget.dart';
import '../../widgets/price_widget.dart';
import '../../routes/app_routes.dart';
import '../../../data/models/cart_model.dart';

class CartPage extends StatelessWidget {
  const CartPage({super.key});

  Future<void> _updateQuantity(
    BuildContext context,
    int bookId,
    int newQuantity,
  ) async {
    try {
      final cartProvider = context.read<CartProvider>();
      await cartProvider.updateItemQuantity(bookId, newQuantity);
    } catch (e) {
      if (context.mounted) {
        ErrorSnackBar.show(context, e.toString());
      }
    }
  }

  Future<void> _removeItem(BuildContext context, int bookId) async {
    try {
      final cartProvider = context.read<CartProvider>();
      await cartProvider.removeItem(bookId);

      if (context.mounted) {
        SuccessSnackBar.show(context, 'Đã xóa sách khỏi giỏ hàng');
      }
    } catch (e) {
      if (context.mounted) {
        ErrorSnackBar.show(context, e.toString());
      }
    }
  }

  Future<void> _clearCart(BuildContext context) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder:
          (context) => AlertDialog(
            title: const Text('Xác nhận'),
            content: const Text(
              'Bạn có chắc chắn muốn xóa tất cả sách trong giỏ hàng?',
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(false),
                child: const Text(AppStrings.cancel),
              ),
              ElevatedButton(
                onPressed: () => Navigator.of(context).pop(true),
                child: const Text(AppStrings.confirm),
              ),
            ],
          ),
    );

    if (confirmed == true && context.mounted) {
      try {
        final cartProvider = context.read<CartProvider>();
        await cartProvider.clearCart();

        if (context.mounted) {
          SuccessSnackBar.show(context, 'Đã xóa tất cả sách khỏi giỏ hàng');
        }
      } catch (e) {
        if (context.mounted) {
          ErrorSnackBar.show(context, e.toString());
        }
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.scaffoldBackground,
      appBar: AppBar(
        title: const Text(AppStrings.cart),
        actions: [
          Consumer<CartProvider>(
            builder: (context, cartProvider, child) {
              if (cartProvider.isEmpty) return const SizedBox.shrink();

              return TextButton(
                onPressed: () => _clearCart(context),
                child: const Text('Xóa tất cả'),
              );
            },
          ),
        ],
      ),
      body: Consumer<CartProvider>(
        builder: (context, cartProvider, child) {
          if (cartProvider.isLoading) {
            return const LoadingWidget(message: 'Đang tải giỏ hàng...');
          }

          if (cartProvider.isEmpty) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(
                    Icons.shopping_cart_outlined,
                    size: 64,
                    color: AppColors.secondaryText,
                  ),
                  const SizedBox(height: 16),
                  const Text(
                    'Giỏ hàng trống',
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.w500,
                      color: AppColors.secondaryText,
                    ),
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'Hãy thêm sách vào giỏ hàng để tiếp tục mua sắm',
                    style: TextStyle(color: AppColors.secondaryText),
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 24),
                  ElevatedButton(
                    onPressed: () => context.go(AppRoutes.bookList),
                    child: const Text('Mua sắm ngay'),
                  ),
                ],
              ),
            );
          }

          return Column(
            children: [
              // Cart Items
              Expanded(
                child: ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: cartProvider.cart.items.length,
                  itemBuilder: (context, index) {
                    final item = cartProvider.cart.items[index];
                    return _CartItemWidget(
                      item: item,
                      onQuantityChanged:
                          (quantity) =>
                              _updateQuantity(context, item.bookId, quantity),
                      onRemove: () => _removeItem(context, item.bookId),
                      onTap:
                          () =>
                              context.go(AppRoutes.bookDetailPath(item.bookId)),
                    );
                  },
                ),
              ),

              // Cart Summary
              Container(
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
                  child: Column(
                    children: [
                      // Price Summary
                      _buildSummaryRow(
                        context,
                        'Tạm tính (${cartProvider.totalItems} sản phẩm)',
                        cartProvider.subTotal,
                      ),
                      const SizedBox(height: 8),
                      _buildSummaryRow(
                        context,
                        'Phí vận chuyển',
                        cartProvider.shippingFee,
                      ),
                      if (cartProvider.discount > 0) ...[
                        const SizedBox(height: 8),
                        _buildSummaryRow(
                          context,
                          'Giảm giá',
                          -cartProvider.discount,
                          valueColor: AppColors.success,
                        ),
                      ],
                      const Divider(height: 24),
                      _buildSummaryRow(
                        context,
                        'Tổng cộng',
                        cartProvider.total,
                        isTotal: true,
                      ),
                      const SizedBox(height: 16),

                      // Checkout Button
                      SizedBox(
                        width: double.infinity,
                        child: ElevatedButton(
                          onPressed: () => context.go(AppRoutes.checkout),
                          style: ElevatedButton.styleFrom(
                            padding: const EdgeInsets.symmetric(vertical: 16),
                          ),
                          child: const Text(
                            AppStrings.checkout,
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  Widget _buildSummaryRow(
    BuildContext context,
    String label,
    double value, {
    Color? valueColor,
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
        Text(
          '${value < 0 ? '-' : ''}${value.abs().toStringAsFixed(0).replaceAllMapped(RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'), (Match m) => '${m[1]},')}₫',
          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
            fontWeight: isTotal ? FontWeight.bold : FontWeight.w600,
            color: valueColor ?? (isTotal ? AppColors.primary : null),
          ),
        ),
      ],
    );
  }
}

class _CartItemWidget extends StatelessWidget {
  final CartItemModel item;
  final Function(int) onQuantityChanged;
  final VoidCallback onRemove;
  final VoidCallback? onTap;

  const _CartItemWidget({
    required this.item,
    required this.onQuantityChanged,
    required this.onRemove,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Row(
            children: [
              // Book Image
              ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: CachedNetworkImage(
                  imageUrl: ImageUtils.getFullImageUrl(item.bookImageUrl),
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
              const SizedBox(width: 12),

              // Book Info
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      item.bookTitle,
                      style: Theme.of(context).textTheme.titleSmall?.copyWith(
                        fontWeight: FontWeight.w600,
                      ),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 8),
                    SimplePriceWidget(price: item.unitPrice),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        // Quantity Controls
                        Container(
                          decoration: BoxDecoration(
                            border: Border.all(color: AppColors.borderGray),
                            borderRadius: BorderRadius.circular(6),
                          ),
                          child: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              InkWell(
                                onTap:
                                    item.quantity > 1
                                        ? () =>
                                            onQuantityChanged(item.quantity - 1)
                                        : null,
                                child: Container(
                                  padding: const EdgeInsets.all(4),
                                  child: const Icon(Icons.remove, size: 16),
                                ),
                              ),
                              Container(
                                padding: const EdgeInsets.symmetric(
                                  horizontal: 12,
                                ),
                                child: Text(
                                  '${item.quantity}',
                                  style: Theme.of(context).textTheme.bodyMedium
                                      ?.copyWith(fontWeight: FontWeight.bold),
                                ),
                              ),
                              InkWell(
                                onTap:
                                    () => onQuantityChanged(item.quantity + 1),
                                child: Container(
                                  padding: const EdgeInsets.all(4),
                                  child: const Icon(Icons.add, size: 16),
                                ),
                              ),
                            ],
                          ),
                        ),
                        const Spacer(),

                        // Remove Button
                        IconButton(
                          onPressed: onRemove,
                          icon: const Icon(Icons.delete_outline),
                          color: AppColors.error,
                          iconSize: 20,
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
