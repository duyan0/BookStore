import 'package:json_annotation/json_annotation.dart';
import 'package:equatable/equatable.dart';

part 'book_model.g.dart';

@JsonSerializable()
class BookModel extends Equatable {
  final int id;
  final String title;
  final String description;
  final double price;
  final double originalPrice;
  final double? discountPercentage;
  final double? discountAmount;
  final bool isOnSale;
  final DateTime? saleStartDate;
  final DateTime? saleEndDate;
  final double? discountedPrice;
  final bool isDiscountActive;
  final double? totalDiscountAmount;
  final int quantity;
  final String isbn;
  final String publisher;
  final int? publicationYear;
  final String imageUrl;
  final int categoryId;
  final String categoryName;
  final int authorId;
  final String authorName;
  final DateTime createdAt;
  final DateTime? updatedAt;

  const BookModel({
    required this.id,
    required this.title,
    required this.description,
    required this.price,
    double? originalPrice,
    this.discountPercentage,
    this.discountAmount,
    required this.isOnSale,
    this.saleStartDate,
    this.saleEndDate,
    this.discountedPrice,
    required this.isDiscountActive,
    this.totalDiscountAmount,
    required this.quantity,
    required this.isbn,
    required this.publisher,
    this.publicationYear,
    required this.imageUrl,
    required this.categoryId,
    required this.categoryName,
    required this.authorId,
    required this.authorName,
    required this.createdAt,
    this.updatedAt,
  }) : originalPrice = originalPrice ?? price;

  bool get isInStock => quantity > 0;
  bool get isLowStock => quantity > 0 && quantity <= 10;
  double get effectivePrice =>
      isDiscountActive ? (discountedPrice ?? price) : price;
  bool get hasDiscount =>
      isDiscountActive && (discountedPrice ?? price) < originalPrice;

  factory BookModel.fromJson(Map<String, dynamic> json) {
    return BookModel(
      id: (json['id'] as num).toInt(),
      title: json['title'] as String,
      description: json['description'] as String,
      price: (json['price'] as num).toDouble(),
      originalPrice:
          (json['price'] as num).toDouble(), // Use price as originalPrice
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
  }

  Map<String, dynamic> toJson() => _$BookModelToJson(this);

  BookModel copyWith({
    int? id,
    String? title,
    String? description,
    double? price,
    double? originalPrice,
    double? discountPercentage,
    double? discountAmount,
    bool? isOnSale,
    DateTime? saleStartDate,
    DateTime? saleEndDate,
    double? discountedPrice,
    bool? isDiscountActive,
    double? totalDiscountAmount,
    int? quantity,
    String? isbn,
    String? publisher,
    int? publicationYear,
    String? imageUrl,
    int? categoryId,
    String? categoryName,
    int? authorId,
    String? authorName,
    DateTime? createdAt,
    DateTime? updatedAt,
  }) {
    return BookModel(
      id: id ?? this.id,
      title: title ?? this.title,
      description: description ?? this.description,
      price: price ?? this.price,
      originalPrice: originalPrice ?? this.originalPrice,
      discountPercentage: discountPercentage ?? this.discountPercentage,
      discountAmount: discountAmount ?? this.discountAmount,
      isOnSale: isOnSale ?? this.isOnSale,
      saleStartDate: saleStartDate ?? this.saleStartDate,
      saleEndDate: saleEndDate ?? this.saleEndDate,
      discountedPrice: discountedPrice ?? this.discountedPrice,
      isDiscountActive: isDiscountActive ?? this.isDiscountActive,
      totalDiscountAmount: totalDiscountAmount ?? this.totalDiscountAmount,
      quantity: quantity ?? this.quantity,
      isbn: isbn ?? this.isbn,
      publisher: publisher ?? this.publisher,
      publicationYear: publicationYear ?? this.publicationYear,
      imageUrl: imageUrl ?? this.imageUrl,
      categoryId: categoryId ?? this.categoryId,
      categoryName: categoryName ?? this.categoryName,
      authorId: authorId ?? this.authorId,
      authorName: authorName ?? this.authorName,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }

  @override
  List<Object?> get props => [
    id,
    title,
    description,
    price,
    originalPrice,
    discountPercentage,
    discountAmount,
    isOnSale,
    saleStartDate,
    saleEndDate,
    discountedPrice,
    isDiscountActive,
    totalDiscountAmount,
    quantity,
    isbn,
    publisher,
    publicationYear,
    imageUrl,
    categoryId,
    categoryName,
    authorId,
    authorName,
    createdAt,
    updatedAt,
  ];
}

@JsonSerializable()
class CreateBookModel extends Equatable {
  final String title;
  final String description;
  final double price;
  final double? discountPercentage;
  final double discountAmount;
  final bool isOnSale;
  final DateTime? saleStartDate;
  final DateTime? saleEndDate;
  final int quantity;
  final String isbn;
  final String publisher;
  final int? publicationYear;
  final String imageUrl;
  final int categoryId;
  final int authorId;

  const CreateBookModel({
    required this.title,
    required this.description,
    required this.price,
    this.discountPercentage,
    required this.discountAmount,
    required this.isOnSale,
    this.saleStartDate,
    this.saleEndDate,
    required this.quantity,
    required this.isbn,
    required this.publisher,
    this.publicationYear,
    required this.imageUrl,
    required this.categoryId,
    required this.authorId,
  });

  factory CreateBookModel.fromJson(Map<String, dynamic> json) =>
      _$CreateBookModelFromJson(json);

  Map<String, dynamic> toJson() => _$CreateBookModelToJson(this);

  @override
  List<Object?> get props => [
    title,
    description,
    price,
    discountPercentage,
    discountAmount,
    isOnSale,
    saleStartDate,
    saleEndDate,
    quantity,
    isbn,
    publisher,
    publicationYear,
    imageUrl,
    categoryId,
    authorId,
  ];
}
