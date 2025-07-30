import 'package:json_annotation/json_annotation.dart';
import 'package:equatable/equatable.dart';
import 'book_model.dart';

part 'cart_model.g.dart';

@JsonSerializable()
class CartModel extends Equatable {
  final List<CartItemModel> items;
  final double subTotal;
  final double shippingFee;
  final double discount;
  final double total;

  const CartModel({
    required this.items,
    required this.subTotal,
    required this.shippingFee,
    required this.discount,
    required this.total,
  });

  factory CartModel.empty() {
    return const CartModel(
      items: [],
      subTotal: 0,
      shippingFee: 0,
      discount: 0,
      total: 0,
    );
  }

  factory CartModel.fromJson(Map<String, dynamic> json) =>
      _$CartModelFromJson(json);

  Map<String, dynamic> toJson() => _$CartModelToJson(this);

  int get totalItems => items.fold(0, (sum, item) => sum + item.quantity);
  bool get isEmpty => items.isEmpty;
  bool get isNotEmpty => items.isNotEmpty;

  CartModel copyWith({
    List<CartItemModel>? items,
    double? subTotal,
    double? shippingFee,
    double? discount,
    double? total,
  }) {
    return CartModel(
      items: items ?? this.items,
      subTotal: subTotal ?? this.subTotal,
      shippingFee: shippingFee ?? this.shippingFee,
      discount: discount ?? this.discount,
      total: total ?? this.total,
    );
  }

  // Helper methods for cart operations
  CartModel addItem(BookModel book, int quantity) {
    final existingItemIndex = items.indexWhere((item) => item.bookId == book.id);
    List<CartItemModel> newItems;

    if (existingItemIndex >= 0) {
      // Update existing item
      newItems = List.from(items);
      final existingItem = newItems[existingItemIndex];
      newItems[existingItemIndex] = existingItem.copyWith(
        quantity: existingItem.quantity + quantity,
      );
    } else {
      // Add new item
      newItems = [
        ...items,
        CartItemModel(
          bookId: book.id,
          bookTitle: book.title,
          bookImageUrl: book.imageUrl,
          unitPrice: book.effectivePrice,
          quantity: quantity,
          book: book,
        ),
      ];
    }

    return _recalculate(newItems);
  }

  CartModel updateItemQuantity(int bookId, int quantity) {
    if (quantity <= 0) {
      return removeItem(bookId);
    }

    final newItems = items.map((item) {
      if (item.bookId == bookId) {
        return item.copyWith(quantity: quantity);
      }
      return item;
    }).toList();

    return _recalculate(newItems);
  }

  CartModel removeItem(int bookId) {
    final newItems = items.where((item) => item.bookId != bookId).toList();
    return _recalculate(newItems);
  }

  CartModel clear() {
    return CartModel.empty();
  }

  CartModel _recalculate(List<CartItemModel> newItems) {
    final newSubTotal = newItems.fold<double>(
      0,
      (sum, item) => sum + item.totalPrice,
    );
    const newShippingFee = 30000.0; // Fixed shipping fee
    const newDiscount = 0.0; // No discount for now
    final newTotal = newSubTotal + newShippingFee - newDiscount;

    return CartModel(
      items: newItems,
      subTotal: newSubTotal,
      shippingFee: newShippingFee,
      discount: newDiscount,
      total: newTotal,
    );
  }

  @override
  List<Object> get props => [items, subTotal, shippingFee, discount, total];
}

@JsonSerializable()
class CartItemModel extends Equatable {
  final int bookId;
  final String bookTitle;
  final String bookImageUrl;
  final double unitPrice;
  final int quantity;
  final BookModel? book; // Optional full book data

  const CartItemModel({
    required this.bookId,
    required this.bookTitle,
    required this.bookImageUrl,
    required this.unitPrice,
    required this.quantity,
    this.book,
  });

  double get totalPrice => unitPrice * quantity;

  factory CartItemModel.fromJson(Map<String, dynamic> json) =>
      _$CartItemModelFromJson(json);

  Map<String, dynamic> toJson() => _$CartItemModelToJson(this);

  CartItemModel copyWith({
    int? bookId,
    String? bookTitle,
    String? bookImageUrl,
    double? unitPrice,
    int? quantity,
    BookModel? book,
  }) {
    return CartItemModel(
      bookId: bookId ?? this.bookId,
      bookTitle: bookTitle ?? this.bookTitle,
      bookImageUrl: bookImageUrl ?? this.bookImageUrl,
      unitPrice: unitPrice ?? this.unitPrice,
      quantity: quantity ?? this.quantity,
      book: book ?? this.book,
    );
  }

  @override
  List<Object?> get props => [
        bookId,
        bookTitle,
        bookImageUrl,
        unitPrice,
        quantity,
        book,
      ];
}
