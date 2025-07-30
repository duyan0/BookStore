import 'package:json_annotation/json_annotation.dart';
import 'package:equatable/equatable.dart';

part 'author_model.g.dart';

@JsonSerializable()
class AuthorModel extends Equatable {
  final int id;
  final String firstName;
  final String lastName;
  final String biography;
  final int bookCount;
  final DateTime createdAt;
  final DateTime? updatedAt;

  const AuthorModel({
    required this.id,
    required this.firstName,
    required this.lastName,
    required this.biography,
    required this.bookCount,
    required this.createdAt,
    this.updatedAt,
  });

  String get fullName => '$firstName $lastName'.trim();

  factory AuthorModel.fromJson(Map<String, dynamic> json) =>
      _$AuthorModelFromJson(json);

  Map<String, dynamic> toJson() => _$AuthorModelToJson(this);

  AuthorModel copyWith({
    int? id,
    String? firstName,
    String? lastName,
    String? biography,
    int? bookCount,
    DateTime? createdAt,
    DateTime? updatedAt,
  }) {
    return AuthorModel(
      id: id ?? this.id,
      firstName: firstName ?? this.firstName,
      lastName: lastName ?? this.lastName,
      biography: biography ?? this.biography,
      bookCount: bookCount ?? this.bookCount,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }

  @override
  List<Object?> get props => [
        id,
        firstName,
        lastName,
        biography,
        bookCount,
        createdAt,
        updatedAt,
      ];
}

@JsonSerializable()
class CreateAuthorModel extends Equatable {
  final String firstName;
  final String lastName;
  final String biography;

  const CreateAuthorModel({
    required this.firstName,
    required this.lastName,
    required this.biography,
  });

  factory CreateAuthorModel.fromJson(Map<String, dynamic> json) =>
      _$CreateAuthorModelFromJson(json);

  Map<String, dynamic> toJson() => _$CreateAuthorModelToJson(this);

  @override
  List<Object> get props => [firstName, lastName, biography];
}

@JsonSerializable()
class UpdateAuthorModel extends Equatable {
  final String firstName;
  final String lastName;
  final String biography;

  const UpdateAuthorModel({
    required this.firstName,
    required this.lastName,
    required this.biography,
  });

  factory UpdateAuthorModel.fromJson(Map<String, dynamic> json) =>
      _$UpdateAuthorModelFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateAuthorModelToJson(this);

  @override
  List<Object> get props => [firstName, lastName, biography];
}
