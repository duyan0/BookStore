// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'author_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

AuthorModel _$AuthorModelFromJson(Map<String, dynamic> json) => AuthorModel(
  id: (json['id'] as num).toInt(),
  firstName: json['firstName'] as String,
  lastName: json['lastName'] as String,
  biography: json['biography'] as String,
  bookCount: (json['bookCount'] as num).toInt(),
  createdAt: DateTime.parse(json['createdAt'] as String),
  updatedAt:
      json['updatedAt'] == null
          ? null
          : DateTime.parse(json['updatedAt'] as String),
);

Map<String, dynamic> _$AuthorModelToJson(AuthorModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'firstName': instance.firstName,
      'lastName': instance.lastName,
      'biography': instance.biography,
      'bookCount': instance.bookCount,
      'createdAt': instance.createdAt.toIso8601String(),
      'updatedAt': instance.updatedAt?.toIso8601String(),
    };

CreateAuthorModel _$CreateAuthorModelFromJson(Map<String, dynamic> json) =>
    CreateAuthorModel(
      firstName: json['firstName'] as String,
      lastName: json['lastName'] as String,
      biography: json['biography'] as String,
    );

Map<String, dynamic> _$CreateAuthorModelToJson(CreateAuthorModel instance) =>
    <String, dynamic>{
      'firstName': instance.firstName,
      'lastName': instance.lastName,
      'biography': instance.biography,
    };

UpdateAuthorModel _$UpdateAuthorModelFromJson(Map<String, dynamic> json) =>
    UpdateAuthorModel(
      firstName: json['firstName'] as String,
      lastName: json['lastName'] as String,
      biography: json['biography'] as String,
    );

Map<String, dynamic> _$UpdateAuthorModelToJson(UpdateAuthorModel instance) =>
    <String, dynamic>{
      'firstName': instance.firstName,
      'lastName': instance.lastName,
      'biography': instance.biography,
    };
