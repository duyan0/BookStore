import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import '../../../core/constants/app_colors.dart';
import '../../../core/constants/app_strings.dart';
import '../../providers/cart_provider.dart';
import '../../providers/auth_provider.dart';
import '../../providers/order_provider.dart';
import '../../widgets/loading_overlay_widget.dart';
import '../../widgets/price_widget.dart';
import '../../../data/models/order_model.dart';
import '../../routes/app_routes.dart';

class CheckoutPage extends StatefulWidget {
  const CheckoutPage({super.key});

  @override
  State<CheckoutPage> createState() => _CheckoutPageState();
}

class _CheckoutPageState extends State<CheckoutPage> {
  final _formKey = GlobalKey<FormState>();
  final _addressController = TextEditingController();
  final _noteController = TextEditingController();

  bool _isProcessing = false;

  @override
  void initState() {
    super.initState();
    // Pre-fill address from user profile
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final user = context.read<AuthProvider>().user;
      if (user != null && user.address.isNotEmpty) {
        _addressController.text = user.address;
      }
    });
  }

  @override
  void dispose() {
    _addressController.dispose();
    _noteController.dispose();
    super.dispose();
  }

  Future<void> _proceedToPayment() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final cartProvider = context.read<CartProvider>();
    final orderProvider = context.read<OrderProvider>();
    final authProvider = context.read<AuthProvider>();

    if (cartProvider.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Giỏ hàng trống'),
          backgroundColor: AppColors.error,
        ),
      );
      return;
    }

    if (authProvider.user == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Vui lòng đăng nhập để đặt hàng'),
          backgroundColor: AppColors.error,
        ),
      );
      return;
    }

    setState(() {
      _isProcessing = true;
    });

    try {
      // Debug logging
      print('Starting order creation...');
      print('User ID: ${authProvider.user!.id}');
      print('Cart items count: ${cartProvider.cart.items.length}');
      print('Shipping address: ${_addressController.text.trim()}');
      print('Sub total: ${cartProvider.subTotal}');
      print('Shipping fee: ${cartProvider.shippingFee}');

      // Create order details from cart items
      final orderDetails =
          cartProvider.cart.items.map((item) {
            print(
              'Order detail - Book ID: ${item.bookId}, Quantity: ${item.quantity}, Unit Price: ${item.unitPrice}',
            );
            return CreateOrderDetailModel(
              bookId: item.bookId,
              quantity: item.quantity,
              unitPrice: item.unitPrice,
            );
          }).toList();

      // Validate required fields
      if (_addressController.text.trim().isEmpty) {
        throw Exception('Địa chỉ giao hàng không được để trống');
      }

      if (orderDetails.isEmpty) {
        throw Exception('Không có sản phẩm nào trong đơn hàng');
      }

      // Create order with COD payment
      final orderData = CreateOrderModel(
        userId: authProvider.user!.id,
        shippingAddress: _addressController.text.trim(),
        paymentMethod: 'COD', // Simplified payment method
        voucherCode: null,
        voucherDiscount: 0.0,
        freeShipping: false,
        shippingFee: cartProvider.shippingFee,
        subTotal: cartProvider.subTotal,
        orderDetails: orderDetails,
      );

      print('Order data JSON: ${orderData.toJson()}');

      print('Order data created, calling API...');
      final newOrder = await orderProvider.createOrder(orderData);
      print('API call completed. New order: ${newOrder?.id}');

      if (newOrder != null) {
        print('Order created successfully with ID: ${newOrder.id}');

        // Clear cart after successful order
        await cartProvider.clearCart();
        print('Cart cleared');

        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text(
                'Đặt hàng thành công! Bạn sẽ thanh toán khi nhận hàng.',
              ),
              backgroundColor: AppColors.success,
            ),
          );

          // Navigate to order detail
          print('Navigating to order detail: ${newOrder.id}');
          context.go(AppRoutes.orderDetailPath(newOrder.id));
        }
      } else {
        print('Order creation failed. Error: ${orderProvider.errorMessage}');
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(orderProvider.errorMessage ?? 'Đặt hàng thất bại'),
              backgroundColor: AppColors.error,
            ),
          );
        }
      }
    } catch (e) {
      print('Exception during order creation: $e');
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Đặt hàng thất bại: ${e.toString()}'),
            backgroundColor: AppColors.error,
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isProcessing = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.scaffoldBackground,
      appBar: AppBar(
        title: const Text(AppStrings.checkout),
        elevation: 1,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.pop(),
        ),
      ),
      body: Consumer<CartProvider>(
        builder: (context, cartProvider, child) {
          if (cartProvider.isEmpty) {
            return const Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(
                    Icons.shopping_cart_outlined,
                    size: 64,
                    color: AppColors.secondaryText,
                  ),
                  SizedBox(height: 16),
                  Text(
                    'Giỏ hàng trống',
                    style: TextStyle(
                      fontSize: 18,
                      color: AppColors.secondaryText,
                    ),
                  ),
                ],
              ),
            );
          }

          return LoadingOverlayWidget(
            isLoading: _isProcessing,
            loadingMessage: 'Đang xử lý đơn hàng...',
            child: Column(
              children: [
                // Form
                Expanded(
                  child: SingleChildScrollView(
                    padding: const EdgeInsets.all(16),
                    child: Form(
                      key: _formKey,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // Order Summary
                          _buildOrderSummary(cartProvider),
                          const SizedBox(height: 24),

                          // Shipping Address
                          _buildShippingAddressSection(),
                          const SizedBox(height: 24),

                          // Payment Method
                          _buildPaymentMethodSection(),
                          const SizedBox(height: 24),

                          // Order Note
                          _buildOrderNoteSection(),
                        ],
                      ),
                    ),
                  ),
                ),

                // Bottom Summary & Place Order
                _buildBottomSection(cartProvider),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildOrderSummary(CartProvider cartProvider) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Tóm tắt đơn hàng',
              style: Theme.of(
                context,
              ).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 16),

            // Items count
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text('Số lượng sản phẩm'),
                Text('${cartProvider.totalItems} sản phẩm'),
              ],
            ),
            const SizedBox(height: 8),

            // Subtotal
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text('Tạm tính'),
                SimplePriceWidget(price: cartProvider.subTotal),
              ],
            ),
            const SizedBox(height: 8),

            // Shipping fee
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text('Phí vận chuyển'),
                SimplePriceWidget(price: cartProvider.shippingFee),
              ],
            ),

            if (cartProvider.discount > 0) ...[
              const SizedBox(height: 8),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text('Giảm giá'),
                  Text(
                    '-${cartProvider.discount.toStringAsFixed(0).replaceAllMapped(RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'), (Match m) => '${m[1]},')}₫',
                    style: TextStyle(color: AppColors.success),
                  ),
                ],
              ),
            ],

            const Divider(height: 24),

            // Total
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Tổng cộng',
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                SimplePriceWidget(
                  price: cartProvider.total,
                  style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: AppColors.primary,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildShippingAddressSection() {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Địa chỉ giao hàng',
              style: Theme.of(
                context,
              ).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _addressController,
              decoration: const InputDecoration(
                labelText: 'Địa chỉ chi tiết *',
                hintText: 'Nhập địa chỉ giao hàng đầy đủ',
                prefixIcon: Icon(Icons.location_on),
              ),
              maxLines: 3,
              validator: (value) {
                if (value == null || value.trim().isEmpty) {
                  return 'Vui lòng nhập địa chỉ giao hàng';
                }
                if (value.trim().length < 10) {
                  return 'Địa chỉ quá ngắn, vui lòng nhập chi tiết hơn';
                }
                return null;
              },
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPaymentMethodSection() {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Payment Method - COD Only
            Text(
              'Phương thức thanh toán',
              style: Theme.of(
                context,
              ).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                border: Border.all(color: AppColors.primary, width: 2),
                borderRadius: BorderRadius.circular(12),
                color: AppColors.primary.withValues(alpha: 0.05),
              ),
              child: Row(
                children: [
                  Container(
                    width: 48,
                    height: 48,
                    decoration: BoxDecoration(
                      color: AppColors.primary.withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: const Icon(
                      Icons.local_shipping,
                      color: AppColors.primary,
                      size: 24,
                    ),
                  ),
                  const SizedBox(width: 16),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Thanh toán khi nhận hàng (COD)',
                          style: TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.w600,
                            color: AppColors.primary,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Bạn sẽ thanh toán bằng tiền mặt khi nhận được hàng',
                          style: TextStyle(
                            fontSize: 14,
                            color: AppColors.secondaryText,
                          ),
                        ),
                      ],
                    ),
                  ),
                  const Icon(
                    Icons.check_circle,
                    color: AppColors.success,
                    size: 24,
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildOrderNoteSection() {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Ghi chú đơn hàng',
              style: Theme.of(
                context,
              ).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _noteController,
              decoration: const InputDecoration(
                labelText: 'Ghi chú (tùy chọn)',
                hintText: 'Nhập ghi chú cho đơn hàng nếu có',
                prefixIcon: Icon(Icons.note),
              ),
              maxLines: 3,
              maxLength: 500,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildBottomSection(CartProvider cartProvider) {
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
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Total summary
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Tổng thanh toán',
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                SimplePriceWidget(
                  price: cartProvider.total,
                  style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: AppColors.primary,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),

            // Place Order Button
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: _isProcessing ? null : _proceedToPayment,
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 16),
                ),
                child:
                    _isProcessing
                        ? const SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            valueColor: AlwaysStoppedAnimation<Color>(
                              AppColors.white,
                            ),
                          ),
                        )
                        : const Text(
                          'Đặt hàng',
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
    );
  }
}
