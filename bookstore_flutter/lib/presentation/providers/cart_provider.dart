import 'package:flutter/foundation.dart';
import '../../data/models/cart_model.dart';
import '../../data/models/book_model.dart';
import '../../data/services/storage_service.dart';

class CartProvider extends ChangeNotifier {
  CartModel _cart = CartModel.empty();
  bool _isLoading = false;

  // Getters
  CartModel get cart => _cart;
  bool get isLoading => _isLoading;
  int get totalItems => _cart.totalItems;
  double get subTotal => _cart.subTotal;
  double get shippingFee => _cart.shippingFee;
  double get discount => _cart.discount;
  double get total => _cart.total;
  bool get isEmpty => _cart.isEmpty;
  bool get isNotEmpty => _cart.isNotEmpty;

  CartProvider() {
    _loadCart();
  }

  /// Load cart from storage
  Future<void> _loadCart() async {
    try {
      _setLoading(true);
      _cart = await StorageService.getCart();
      notifyListeners();
    } catch (e) {
      // Handle error silently, keep empty cart
    } finally {
      _setLoading(false);
    }
  }

  /// Save cart to storage
  Future<void> _saveCart() async {
    try {
      await StorageService.saveCart(_cart);
    } catch (e) {
      // Handle error silently
    }
  }

  /// Add item to cart
  Future<void> addItem(BookModel book, {int quantity = 1}) async {
    try {
      _setLoading(true);
      
      if (!book.isInStock) {
        throw Exception('Book is out of stock');
      }

      if (quantity <= 0) {
        throw Exception('Quantity must be greater than 0');
      }

      // Check if adding this quantity would exceed available stock
      final existingItem = _cart.items.firstWhere(
        (item) => item.bookId == book.id,
        orElse: () => const CartItemModel(
          bookId: 0,
          bookTitle: '',
          bookImageUrl: '',
          unitPrice: 0,
          quantity: 0,
        ),
      );

      final totalQuantity = existingItem.quantity + quantity;
      if (totalQuantity > book.quantity) {
        throw Exception('Not enough stock available. Available: ${book.quantity}');
      }

      _cart = _cart.addItem(book, quantity);
      await _saveCart();
      notifyListeners();
    } catch (e) {
      rethrow;
    } finally {
      _setLoading(false);
    }
  }

  /// Update item quantity
  Future<void> updateItemQuantity(int bookId, int quantity) async {
    try {
      _setLoading(true);
      
      if (quantity < 0) {
        throw Exception('Quantity cannot be negative');
      }

      _cart = _cart.updateItemQuantity(bookId, quantity);
      await _saveCart();
      notifyListeners();
    } catch (e) {
      rethrow;
    } finally {
      _setLoading(false);
    }
  }

  /// Remove item from cart
  Future<void> removeItem(int bookId) async {
    try {
      _setLoading(true);
      _cart = _cart.removeItem(bookId);
      await _saveCart();
      notifyListeners();
    } catch (e) {
      rethrow;
    } finally {
      _setLoading(false);
    }
  }

  /// Clear all items from cart
  Future<void> clearCart() async {
    try {
      _setLoading(true);
      _cart = CartModel.empty();
      await StorageService.clearCart();
      notifyListeners();
    } catch (e) {
      rethrow;
    } finally {
      _setLoading(false);
    }
  }

  /// Get item quantity for a specific book
  int getItemQuantity(int bookId) {
    final item = _cart.items.firstWhere(
      (item) => item.bookId == bookId,
      orElse: () => const CartItemModel(
        bookId: 0,
        bookTitle: '',
        bookImageUrl: '',
        unitPrice: 0,
        quantity: 0,
      ),
    );
    return item.quantity;
  }

  /// Check if book is in cart
  bool isInCart(int bookId) {
    return _cart.items.any((item) => item.bookId == bookId);
  }

  /// Get cart item for a specific book
  CartItemModel? getCartItem(int bookId) {
    try {
      return _cart.items.firstWhere((item) => item.bookId == bookId);
    } catch (e) {
      return null;
    }
  }

  /// Increment item quantity
  Future<void> incrementItem(int bookId) async {
    final currentQuantity = getItemQuantity(bookId);
    await updateItemQuantity(bookId, currentQuantity + 1);
  }

  /// Decrement item quantity
  Future<void> decrementItem(int bookId) async {
    final currentQuantity = getItemQuantity(bookId);
    if (currentQuantity > 1) {
      await updateItemQuantity(bookId, currentQuantity - 1);
    } else {
      await removeItem(bookId);
    }
  }

  /// Apply discount (for future use)
  Future<void> applyDiscount(double discountAmount) async {
    try {
      _setLoading(true);
      _cart = _cart.copyWith(discount: discountAmount);
      await _saveCart();
      notifyListeners();
    } catch (e) {
      rethrow;
    } finally {
      _setLoading(false);
    }
  }

  /// Update shipping fee (for future use)
  Future<void> updateShippingFee(double shippingFee) async {
    try {
      _setLoading(true);
      _cart = _cart.copyWith(shippingFee: shippingFee);
      await _saveCart();
      notifyListeners();
    } catch (e) {
      rethrow;
    } finally {
      _setLoading(false);
    }
  }

  /// Refresh cart (reload from storage)
  Future<void> refreshCart() async {
    await _loadCart();
  }

  // Private methods
  void _setLoading(bool loading) {
    _isLoading = loading;
    notifyListeners();
  }
}
