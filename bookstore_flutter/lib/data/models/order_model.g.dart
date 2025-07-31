// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'order_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

OrderModel _$OrderModelFromJson(Map<String, dynamic> json) => OrderModel(
  id: (json['id'] as num).toInt(),
  userId: (json['userId'] as num).toInt(),
  userName: json['userName'] as String,
  userFullName: json['userFullName'] as String,
  orderDate: DateTime.parse(json['orderDate'] as String),
  totalAmount: (json['totalAmount'] as num).toDouble(),
  status: json['status'] as String,
  shippingAddress: json['shippingAddress'] as String,
  paymentMethod: json['paymentMethod'] as String,
  orderDetails:
      (json['orderDetails'] as List<dynamic>)
          .map((e) => OrderDetailModel.fromJson(e as Map<String, dynamic>))
          .toList(),
  createdAt: DateTime.parse(json['createdAt'] as String),
  updatedAt:
      json['updatedAt'] == null
          ? null
          : DateTime.parse(json['updatedAt'] as String),
);

Map<String, dynamic> _$OrderModelToJson(OrderModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'userId': instance.userId,
      'userName': instance.userName,
      'userFullName': instance.userFullName,
      'orderDate': instance.orderDate.toIso8601String(),
      'totalAmount': instance.totalAmount,
      'status': instance.status,
      'shippingAddress': instance.shippingAddress,
      'paymentMethod': instance.paymentMethod,
      'orderDetails': instance.orderDetails,
      'createdAt': instance.createdAt.toIso8601String(),
      'updatedAt': instance.updatedAt?.toIso8601String(),
    };

OrderDetailModel _$OrderDetailModelFromJson(Map<String, dynamic> json) =>
    OrderDetailModel(
      id: (json['id'] as num).toInt(),
      orderId: (json['orderId'] as num).toInt(),
      bookId: (json['bookId'] as num).toInt(),
      bookTitle: json['bookTitle'] as String,
      bookImageUrl: json['bookImageUrl'] as String,
      quantity: (json['quantity'] as num).toInt(),
      unitPrice: (json['unitPrice'] as num).toDouble(),
    );

Map<String, dynamic> _$OrderDetailModelToJson(OrderDetailModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'orderId': instance.orderId,
      'bookId': instance.bookId,
      'bookTitle': instance.bookTitle,
      'bookImageUrl': instance.bookImageUrl,
      'quantity': instance.quantity,
      'unitPrice': instance.unitPrice,
    };

CreateOrderModel _$CreateOrderModelFromJson(Map<String, dynamic> json) =>
    CreateOrderModel(
      userId: (json['UserId'] as num).toInt(),
      shippingAddress: json['ShippingAddress'] as String,
      paymentMethod: json['PaymentMethod'] as String,
      voucherCode: json['VoucherCode'] as String?,
      voucherDiscount: (json['VoucherDiscount'] as num).toDouble(),
      freeShipping: json['FreeShipping'] as bool,
      shippingFee: (json['ShippingFee'] as num).toDouble(),
      subTotal: (json['SubTotal'] as num).toDouble(),
      orderDetails:
          (json['OrderDetails'] as List<dynamic>)
              .map(
                (e) =>
                    CreateOrderDetailModel.fromJson(e as Map<String, dynamic>),
              )
              .toList(),
    );

Map<String, dynamic> _$CreateOrderModelToJson(CreateOrderModel instance) =>
    <String, dynamic>{
      'UserId': instance.userId,
      'ShippingAddress': instance.shippingAddress,
      'PaymentMethod': instance.paymentMethod,
      'VoucherCode': instance.voucherCode,
      'VoucherDiscount': instance.voucherDiscount,
      'FreeShipping': instance.freeShipping,
      'ShippingFee': instance.shippingFee,
      'SubTotal': instance.subTotal,
      'OrderDetails': instance.orderDetails,
    };

CreateOrderDetailModel _$CreateOrderDetailModelFromJson(
  Map<String, dynamic> json,
) => CreateOrderDetailModel(
  bookId: (json['BookId'] as num).toInt(),
  quantity: (json['Quantity'] as num).toInt(),
  unitPrice: (json['UnitPrice'] as num).toDouble(),
);

Map<String, dynamic> _$CreateOrderDetailModelToJson(
  CreateOrderDetailModel instance,
) => <String, dynamic>{
  'BookId': instance.bookId,
  'Quantity': instance.quantity,
  'UnitPrice': instance.unitPrice,
};
