import 'package:flutter/foundation.dart';
import '../../data/models/order_model.dart';
import '../../data/services/order_service.dart';
import '../../core/errors/api_exception.dart';

enum OrderLoadingState {
  initial,
  loading,
  loaded,
  error,
}

class OrderProvider extends ChangeNotifier {
  final OrderService _orderService;

  OrderProvider(this._orderService);

  // State
  OrderLoadingState _state = OrderLoadingState.initial;
  List<OrderModel> _orders = [];
  List<OrderModel> _userOrders = [];
  OrderModel? _selectedOrder;
  Map<String, dynamic>? _statistics;
  String? _errorMessage;

  // Getters
  OrderLoadingState get state => _state;
  List<OrderModel> get orders => _orders;
  List<OrderModel> get userOrders => _userOrders;
  OrderModel? get selectedOrder => _selectedOrder;
  Map<String, dynamic>? get statistics => _statistics;
  String? get errorMessage => _errorMessage;
  bool get isLoading => _state == OrderLoadingState.loading;
  bool get hasError => _state == OrderLoadingState.error;
  bool get isEmpty => _orders.isEmpty && _state == OrderLoadingState.loaded;

  /// Load all orders (Admin only)
  Future<void> loadAllOrders() async {
    try {
      _setState(OrderLoadingState.loading);
      _clearError();

      _orders = await _orderService.getAllOrders();
      _setState(OrderLoadingState.loaded);
    } on ApiException catch (e) {
      _setError(e.message);
    } catch (e) {
      _setError('Failed to load orders: ${e.toString()}');
    }
  }

  /// Load user orders
  Future<void> loadUserOrders() async {
    try {
      _setState(OrderLoadingState.loading);
      _clearError();

      _userOrders = await _orderService.getUserOrders();
      _setState(OrderLoadingState.loaded);
    } on ApiException catch (e) {
      _setError(e.message);
    } catch (e) {
      _setError('Failed to load user orders: ${e.toString()}');
    }
  }

  /// Load order by ID
  Future<void> loadOrderById(int id) async {
    try {
      _setState(OrderLoadingState.loading);
      _clearError();

      _selectedOrder = await _orderService.getOrderById(id);
      _setState(OrderLoadingState.loaded);
    } on ApiException catch (e) {
      _setError(e.message);
    } catch (e) {
      _setError('Failed to load order details: ${e.toString()}');
    }
  }

  /// Create new order
  Future<OrderModel?> createOrder(CreateOrderModel orderData) async {
    try {
      _setState(OrderLoadingState.loading);
      _clearError();

      final newOrder = await _orderService.createOrder(orderData);
      
      // Add to lists if they exist
      if (_orders.isNotEmpty) {
        _orders.insert(0, newOrder);
      }
      if (_userOrders.isNotEmpty) {
        _userOrders.insert(0, newOrder);
      }
      
      _setState(OrderLoadingState.loaded);
      return newOrder;
    } on ApiException catch (e) {
      _setError(e.message);
      return null;
    } catch (e) {
      _setError('Failed to create order: ${e.toString()}');
      return null;
    }
  }

  /// Update order status (Admin only)
  Future<bool> updateOrderStatus(int id, String status) async {
    try {
      _setState(OrderLoadingState.loading);
      _clearError();

      final updatedOrder = await _orderService.updateOrderStatus(id, status);
      
      // Update in lists
      final allOrdersIndex = _orders.indexWhere((order) => order.id == id);
      if (allOrdersIndex != -1) {
        _orders[allOrdersIndex] = updatedOrder;
      }
      
      final userOrdersIndex = _userOrders.indexWhere((order) => order.id == id);
      if (userOrdersIndex != -1) {
        _userOrders[userOrdersIndex] = updatedOrder;
      }
      
      if (_selectedOrder?.id == id) {
        _selectedOrder = updatedOrder;
      }
      
      _setState(OrderLoadingState.loaded);
      return true;
    } on ApiException catch (e) {
      _setError(e.message);
      return false;
    } catch (e) {
      _setError('Failed to update order status: ${e.toString()}');
      return false;
    }
  }

  /// Cancel order
  Future<bool> cancelOrder(int id) async {
    try {
      _setState(OrderLoadingState.loading);
      _clearError();

      final updatedOrder = await _orderService.cancelOrder(id);
      
      // Update in lists
      final allOrdersIndex = _orders.indexWhere((order) => order.id == id);
      if (allOrdersIndex != -1) {
        _orders[allOrdersIndex] = updatedOrder;
      }
      
      final userOrdersIndex = _userOrders.indexWhere((order) => order.id == id);
      if (userOrdersIndex != -1) {
        _userOrders[userOrdersIndex] = updatedOrder;
      }
      
      if (_selectedOrder?.id == id) {
        _selectedOrder = updatedOrder;
      }
      
      _setState(OrderLoadingState.loaded);
      return true;
    } on ApiException catch (e) {
      _setError(e.message);
      return false;
    } catch (e) {
      _setError('Failed to cancel order: ${e.toString()}');
      return false;
    }
  }

  /// Load order statistics (Admin only)
  Future<void> loadOrderStatistics() async {
    try {
      _statistics = await _orderService.getOrderStatistics();
      notifyListeners();
    } on ApiException catch (e) {
      // Handle error silently for statistics
      debugPrint('Failed to load order statistics: ${e.message}');
    } catch (e) {
      debugPrint('Failed to load order statistics: ${e.toString()}');
    }
  }

  /// Get orders by status
  List<OrderModel> getOrdersByStatus(String status) {
    return _orders.where((order) => 
        order.status.toLowerCase() == status.toLowerCase()).toList();
  }

  /// Get recent orders
  List<OrderModel> getRecentOrders({int limit = 10}) {
    final sortedOrders = List<OrderModel>.from(_orders);
    sortedOrders.sort((a, b) => b.orderDate.compareTo(a.orderDate));
    return sortedOrders.take(limit).toList();
  }

  /// Calculate total revenue
  double getTotalRevenue() {
    return _orders
        .where((order) => order.status.toLowerCase() == 'delivered')
        .fold(0.0, (sum, order) => sum + order.totalAmount);
  }

  /// Get order counts by status
  Map<String, int> getOrderCountsByStatus() {
    final counts = <String, int>{};
    for (final order in _orders) {
      final status = order.status.toLowerCase();
      counts[status] = (counts[status] ?? 0) + 1;
    }
    return counts;
  }

  /// Refresh orders
  Future<void> refreshOrders() async {
    await loadAllOrders();
  }

  /// Refresh user orders
  Future<void> refreshUserOrders() async {
    await loadUserOrders();
  }

  /// Clear selected order
  void clearSelectedOrder() {
    _selectedOrder = null;
    notifyListeners();
  }

  /// Clear error
  void clearError() {
    _clearError();
  }

  // Private methods
  void _setState(OrderLoadingState newState) {
    _state = newState;
    notifyListeners();
  }

  void _setError(String error) {
    _errorMessage = error;
    _state = OrderLoadingState.error;
    notifyListeners();
  }

  void _clearError() {
    _errorMessage = null;
    if (_state == OrderLoadingState.error) {
      _state = _orders.isEmpty && _userOrders.isEmpty 
          ? OrderLoadingState.initial 
          : OrderLoadingState.loaded;
    }
    notifyListeners();
  }
}
