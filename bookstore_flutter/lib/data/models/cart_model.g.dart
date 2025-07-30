// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'cart_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CartModel _$CartModelFromJson(Map<String, dynamic> json) => CartModel(
  items:
      (json['items'] as List<dynamic>)
          .map((e) => CartItemModel.fromJson(e as Map<String, dynamic>))
          .toList(),
  subTotal: (json['subTotal'] as num).toDouble(),
  shippingFee: (json['shippingFee'] as num).toDouble(),
  discount: (json['discount'] as num).toDouble(),
  total: (json['total'] as num).toDouble(),
);

Map<String, dynamic> _$CartModelToJson(CartModel instance) => <String, dynamic>{
  'items': instance.items,
  'subTotal': instance.subTotal,
  'shippingFee': instance.shippingFee,
  'discount': instance.discount,
  'total': instance.total,
};

CartItemModel _$CartItemModelFromJson(Map<String, dynamic> json) =>
    CartItemModel(
      bookId: (json['bookId'] as num).toInt(),
      bookTitle: json['bookTitle'] as String,
      bookImageUrl: json['bookImageUrl'] as String,
      unitPrice: (json['unitPrice'] as num).toDouble(),
      quantity: (json['quantity'] as num).toInt(),
      book:
          json['book'] == null
              ? null
              : BookModel.fromJson(json['book'] as Map<String, dynamic>),
    );

Map<String, dynamic> _$CartItemModelToJson(CartItemModel instance) =>
    <String, dynamic>{
      'bookId': instance.bookId,
      'bookTitle': instance.bookTitle,
      'bookImageUrl': instance.bookImageUrl,
      'unitPrice': instance.unitPrice,
      'quantity': instance.quantity,
      'book': instance.book,
    };
