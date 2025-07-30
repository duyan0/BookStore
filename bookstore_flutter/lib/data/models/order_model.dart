import 'package:json_annotation/json_annotation.dart';
import 'package:equatable/equatable.dart';

part 'order_model.g.dart';

@JsonSerializable()
class OrderModel extends Equatable {
  final int id;
  final int userId;
  final String userName;
  final String userFullName;
  final DateTime orderDate;
  final double totalAmount;
  final String status;
  final String shippingAddress;
  final String paymentMethod;
  final List<OrderDetailModel> orderDetails;
  final DateTime createdAt;
  final DateTime? updatedAt;

  const OrderModel({
    required this.id,
    required this.userId,
    required this.userName,
    required this.userFullName,
    required this.orderDate,
    required this.totalAmount,
    required this.status,
    required this.shippingAddress,
    required this.paymentMethod,
    required this.orderDetails,
    required this.createdAt,
    this.updatedAt,
  });

  factory OrderModel.fromJson(Map<String, dynamic> json) =>
      _$OrderModelFromJson(json);

  Map<String, dynamic> toJson() => _$OrderModelToJson(this);

  OrderModel copyWith({
    int? id,
    int? userId,
    String? userName,
    String? userFullName,
    DateTime? orderDate,
    double? totalAmount,
    String? status,
    String? shippingAddress,
    String? paymentMethod,
    List<OrderDetailModel>? orderDetails,
    DateTime? createdAt,
    DateTime? updatedAt,
  }) {
    return OrderModel(
      id: id ?? this.id,
      userId: userId ?? this.userId,
      userName: userName ?? this.userName,
      userFullName: userFullName ?? this.userFullName,
      orderDate: orderDate ?? this.orderDate,
      totalAmount: totalAmount ?? this.totalAmount,
      status: status ?? this.status,
      shippingAddress: shippingAddress ?? this.shippingAddress,
      paymentMethod: paymentMethod ?? this.paymentMethod,
      orderDetails: orderDetails ?? this.orderDetails,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }

  @override
  List<Object?> get props => [
    id,
    userId,
    userName,
    userFullName,
    orderDate,
    totalAmount,
    status,
    shippingAddress,
    paymentMethod,
    orderDetails,
    createdAt,
    updatedAt,
  ];
}

@JsonSerializable()
class OrderDetailModel extends Equatable {
  final int id;
  final int orderId;
  final int bookId;
  final String bookTitle;
  final String bookImageUrl;
  final int quantity;
  final double unitPrice;

  const OrderDetailModel({
    required this.id,
    required this.orderId,
    required this.bookId,
    required this.bookTitle,
    required this.bookImageUrl,
    required this.quantity,
    required this.unitPrice,
  });

  double get totalPrice => quantity * unitPrice;

  factory OrderDetailModel.fromJson(Map<String, dynamic> json) =>
      _$OrderDetailModelFromJson(json);

  Map<String, dynamic> toJson() => _$OrderDetailModelToJson(this);

  @override
  List<Object> get props => [
    id,
    orderId,
    bookId,
    bookTitle,
    bookImageUrl,
    quantity,
    unitPrice,
  ];
}

@JsonSerializable()
class CreateOrderModel extends Equatable {
  final int userId;
  final String shippingAddress;
  final String paymentMethod;
  final String? voucherCode;
  final double voucherDiscount;
  final bool freeShipping;
  final double shippingFee;
  final double subTotal;
  final List<CreateOrderDetailModel> orderDetails;

  const CreateOrderModel({
    required this.userId,
    required this.shippingAddress,
    required this.paymentMethod,
    this.voucherCode,
    required this.voucherDiscount,
    required this.freeShipping,
    required this.shippingFee,
    required this.subTotal,
    required this.orderDetails,
  });

  factory CreateOrderModel.fromJson(Map<String, dynamic> json) =>
      _$CreateOrderModelFromJson(json);

  Map<String, dynamic> toJson() => _$CreateOrderModelToJson(this);

  @override
  List<Object?> get props => [
    userId,
    shippingAddress,
    paymentMethod,
    voucherCode,
    voucherDiscount,
    freeShipping,
    shippingFee,
    subTotal,
    orderDetails,
  ];
}

@JsonSerializable()
class CreateOrderDetailModel extends Equatable {
  final int bookId;
  final int quantity;
  final double unitPrice;

  const CreateOrderDetailModel({
    required this.bookId,
    required this.quantity,
    required this.unitPrice,
  });

  factory CreateOrderDetailModel.fromJson(Map<String, dynamic> json) =>
      _$CreateOrderDetailModelFromJson(json);

  Map<String, dynamic> toJson() => _$CreateOrderDetailModelToJson(this);

  @override
  List<Object> get props => [bookId, quantity, unitPrice];
}
