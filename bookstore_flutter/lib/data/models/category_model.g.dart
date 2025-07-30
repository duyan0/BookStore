// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'category_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CategoryModel _$CategoryModelFromJson(Map<String, dynamic> json) =>
    CategoryModel(
      id: (json['id'] as num).toInt(),
      name: json['name'] as String,
      description: json['description'] as String,
      bookCount: (json['bookCount'] as num).toInt(),
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt:
          json['updatedAt'] == null
              ? null
              : DateTime.parse(json['updatedAt'] as String),
    );

Map<String, dynamic> _$CategoryModelToJson(CategoryModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'description': instance.description,
      'bookCount': instance.bookCount,
      'createdAt': instance.createdAt.toIso8601String(),
      'updatedAt': instance.updatedAt?.toIso8601String(),
    };

CreateCategoryModel _$CreateCategoryModelFromJson(Map<String, dynamic> json) =>
    CreateCategoryModel(
      name: json['name'] as String,
      description: json['description'] as String,
    );

Map<String, dynamic> _$CreateCategoryModelToJson(
  CreateCategoryModel instance,
) => <String, dynamic>{
  'name': instance.name,
  'description': instance.description,
};

UpdateCategoryModel _$UpdateCategoryModelFromJson(Map<String, dynamic> json) =>
    UpdateCategoryModel(
      name: json['name'] as String,
      description: json['description'] as String,
    );

Map<String, dynamic> _$UpdateCategoryModelToJson(
  UpdateCategoryModel instance,
) => <String, dynamic>{
  'name': instance.name,
  'description': instance.description,
};
