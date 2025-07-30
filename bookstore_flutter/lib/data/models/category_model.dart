import 'package:json_annotation/json_annotation.dart';
import 'package:equatable/equatable.dart';

part 'category_model.g.dart';

@JsonSerializable()
class CategoryModel extends Equatable {
  final int id;
  final String name;
  final String description;
  final int bookCount;
  final DateTime createdAt;
  final DateTime? updatedAt;

  const CategoryModel({
    required this.id,
    required this.name,
    required this.description,
    required this.bookCount,
    required this.createdAt,
    this.updatedAt,
  });

  factory CategoryModel.fromJson(Map<String, dynamic> json) =>
      _$CategoryModelFromJson(json);

  Map<String, dynamic> toJson() => _$CategoryModelToJson(this);

  CategoryModel copyWith({
    int? id,
    String? name,
    String? description,
    int? bookCount,
    DateTime? createdAt,
    DateTime? updatedAt,
  }) {
    return CategoryModel(
      id: id ?? this.id,
      name: name ?? this.name,
      description: description ?? this.description,
      bookCount: bookCount ?? this.bookCount,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }

  @override
  List<Object?> get props => [
        id,
        name,
        description,
        bookCount,
        createdAt,
        updatedAt,
      ];
}

@JsonSerializable()
class CreateCategoryModel extends Equatable {
  final String name;
  final String description;

  const CreateCategoryModel({
    required this.name,
    required this.description,
  });

  factory CreateCategoryModel.fromJson(Map<String, dynamic> json) =>
      _$CreateCategoryModelFromJson(json);

  Map<String, dynamic> toJson() => _$CreateCategoryModelToJson(this);

  @override
  List<Object> get props => [name, description];
}

@JsonSerializable()
class UpdateCategoryModel extends Equatable {
  final String name;
  final String description;

  const UpdateCategoryModel({
    required this.name,
    required this.description,
  });

  factory UpdateCategoryModel.fromJson(Map<String, dynamic> json) =>
      _$UpdateCategoryModelFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateCategoryModelToJson(this);

  @override
  List<Object> get props => [name, description];
}
