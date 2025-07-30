// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'book_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

BookModel _$BookModelFromJson(Map<String, dynamic> json) => BookModel(
  id: (json['id'] as num).toInt(),
  title: json['title'] as String,
  description: json['description'] as String,
  price: (json['price'] as num).toDouble(),
  originalPrice: (json['originalPrice'] as num?)?.toDouble(),
  discountPercentage: (json['discountPercentage'] as num?)?.toDouble(),
  discountAmount: (json['discountAmount'] as num?)?.toDouble(),
  isOnSale: json['isOnSale'] as bool,
  saleStartDate:
      json['saleStartDate'] == null
          ? null
          : DateTime.parse(json['saleStartDate'] as String),
  saleEndDate:
      json['saleEndDate'] == null
          ? null
          : DateTime.parse(json['saleEndDate'] as String),
  discountedPrice: (json['discountedPrice'] as num?)?.toDouble(),
  isDiscountActive: json['isDiscountActive'] as bool,
  totalDiscountAmount: (json['totalDiscountAmount'] as num?)?.toDouble(),
  quantity: (json['quantity'] as num).toInt(),
  isbn: json['isbn'] as String,
  publisher: json['publisher'] as String,
  publicationYear: (json['publicationYear'] as num?)?.toInt(),
  imageUrl: json['imageUrl'] as String,
  categoryId: (json['categoryId'] as num).toInt(),
  categoryName: json['categoryName'] as String,
  authorId: (json['authorId'] as num).toInt(),
  authorName: json['authorName'] as String,
  createdAt: DateTime.parse(json['createdAt'] as String),
  updatedAt:
      json['updatedAt'] == null
          ? null
          : DateTime.parse(json['updatedAt'] as String),
);

Map<String, dynamic> _$BookModelToJson(BookModel instance) => <String, dynamic>{
  'id': instance.id,
  'title': instance.title,
  'description': instance.description,
  'price': instance.price,
  'originalPrice': instance.originalPrice,
  'discountPercentage': instance.discountPercentage,
  'discountAmount': instance.discountAmount,
  'isOnSale': instance.isOnSale,
  'saleStartDate': instance.saleStartDate?.toIso8601String(),
  'saleEndDate': instance.saleEndDate?.toIso8601String(),
  'discountedPrice': instance.discountedPrice,
  'isDiscountActive': instance.isDiscountActive,
  'totalDiscountAmount': instance.totalDiscountAmount,
  'quantity': instance.quantity,
  'isbn': instance.isbn,
  'publisher': instance.publisher,
  'publicationYear': instance.publicationYear,
  'imageUrl': instance.imageUrl,
  'categoryId': instance.categoryId,
  'categoryName': instance.categoryName,
  'authorId': instance.authorId,
  'authorName': instance.authorName,
  'createdAt': instance.createdAt.toIso8601String(),
  'updatedAt': instance.updatedAt?.toIso8601String(),
};

CreateBookModel _$CreateBookModelFromJson(Map<String, dynamic> json) =>
    CreateBookModel(
      title: json['title'] as String,
      description: json['description'] as String,
      price: (json['price'] as num).toDouble(),
      discountPercentage: (json['discountPercentage'] as num?)?.toDouble(),
      discountAmount: (json['discountAmount'] as num).toDouble(),
      isOnSale: json['isOnSale'] as bool,
      saleStartDate:
          json['saleStartDate'] == null
              ? null
              : DateTime.parse(json['saleStartDate'] as String),
      saleEndDate:
          json['saleEndDate'] == null
              ? null
              : DateTime.parse(json['saleEndDate'] as String),
      quantity: (json['quantity'] as num).toInt(),
      isbn: json['isbn'] as String,
      publisher: json['publisher'] as String,
      publicationYear: (json['publicationYear'] as num?)?.toInt(),
      imageUrl: json['imageUrl'] as String,
      categoryId: (json['categoryId'] as num).toInt(),
      authorId: (json['authorId'] as num).toInt(),
    );

Map<String, dynamic> _$CreateBookModelToJson(CreateBookModel instance) =>
    <String, dynamic>{
      'title': instance.title,
      'description': instance.description,
      'price': instance.price,
      'discountPercentage': instance.discountPercentage,
      'discountAmount': instance.discountAmount,
      'isOnSale': instance.isOnSale,
      'saleStartDate': instance.saleStartDate?.toIso8601String(),
      'saleEndDate': instance.saleEndDate?.toIso8601String(),
      'quantity': instance.quantity,
      'isbn': instance.isbn,
      'publisher': instance.publisher,
      'publicationYear': instance.publicationYear,
      'imageUrl': instance.imageUrl,
      'categoryId': instance.categoryId,
      'authorId': instance.authorId,
    };
