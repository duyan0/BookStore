import '../../core/network/api_client.dart';
import '../../core/constants/api_constants.dart';
import '../../core/errors/api_exception.dart';
import '../models/order_model.dart';
import 'storage_service.dart';

class OrderService {
  final ApiClient _apiClient;

  OrderService(this._apiClient);

  /// Lấy danh sách đơn hàng
  /// Admin: lấy tất cả đơn hàng
  /// User: chỉ lấy đơn hàng của mình
  Future<List<OrderModel>> getAllOrders() async {
    try {
      final response = await _apiClient.get<List<dynamic>>(ApiConstants.orders);

      return response.map((json) => OrderModel.fromJson(json)).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch orders: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy chi tiết đơn hàng theo ID
  Future<OrderModel> getOrderById(int id) async {
    try {
      final response = await _apiClient.get<Map<String, dynamic>>(
        '${ApiConstants.orders}/$id',
      );

      return OrderModel.fromJson(response);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch order details: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy đơn hàng của user hiện tại
  Future<List<OrderModel>> getUserOrders() async {
    try {
      // Get current user ID from storage
      final user = await StorageService.getUserData();

      if (user == null) {
        throw ApiException(message: 'User not authenticated', statusCode: 401);
      }

      final response = await _apiClient.get<List<dynamic>>(
        '${ApiConstants.orders}/user/${user.id}',
      );

      return response.map((json) => OrderModel.fromJson(json)).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch user orders: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Tạo đơn hàng mới
  Future<OrderModel> createOrder(CreateOrderModel orderData) async {
    try {
      final response = await _apiClient.post<Map<String, dynamic>>(
        ApiConstants.orders,
        data: orderData.toJson(),
      );

      return OrderModel.fromJson(response);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to create order: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Cập nhật trạng thái đơn hàng (Admin only)
  Future<OrderModel> updateOrderStatus(int id, String status) async {
    try {
      final response = await _apiClient.put<Map<String, dynamic>>(
        '${ApiConstants.orders}/$id',
        data: {'status': status},
      );

      return OrderModel.fromJson(response);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to update order status: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Hủy đơn hàng
  Future<OrderModel> cancelOrder(int id) async {
    try {
      return await updateOrderStatus(id, 'Cancelled');
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to cancel order: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy thống kê đơn hàng (Admin only)
  Future<Map<String, dynamic>> getOrderStatistics() async {
    try {
      final response = await _apiClient.get<Map<String, dynamic>>(
        ApiConstants.ordersStatistics,
      );

      return response;
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch order statistics: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy đơn hàng theo trạng thái
  Future<List<OrderModel>> getOrdersByStatus(String status) async {
    try {
      final allOrders = await getAllOrders();
      return allOrders
          .where((order) => order.status.toLowerCase() == status.toLowerCase())
          .toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch orders by status: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Lấy đơn hàng gần đây
  Future<List<OrderModel>> getRecentOrders({int limit = 10}) async {
    try {
      final allOrders = await getAllOrders();
      // Sort by order date in descending order
      allOrders.sort((a, b) => b.orderDate.compareTo(a.orderDate));
      return allOrders.take(limit).toList();
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to fetch recent orders: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Tính tổng doanh thu
  Future<double> getTotalRevenue() async {
    try {
      final allOrders = await getAllOrders();
      double total = 0.0;
      for (final order in allOrders) {
        if (order.status.toLowerCase() == 'delivered') {
          total += order.totalAmount;
        }
      }
      return total;
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Failed to calculate total revenue: ${e.toString()}',
        statusCode: 0,
      );
    }
  }
}
