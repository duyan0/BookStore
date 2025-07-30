import 'package:json_annotation/json_annotation.dart';
import 'package:equatable/equatable.dart';

part 'user_model.g.dart';

@JsonSerializable()
class UserModel extends Equatable {
  final int id;
  final String username;
  final String email;
  final String firstName;
  final String lastName;
  final String phone;
  final String address;
  final String? avatarUrl;
  final bool isAdmin;
  final DateTime createdAt;
  final DateTime? updatedAt;

  const UserModel({
    required this.id,
    required this.username,
    required this.email,
    required this.firstName,
    required this.lastName,
    required this.phone,
    required this.address,
    this.avatarUrl,
    required this.isAdmin,
    required this.createdAt,
    this.updatedAt,
  });

  String get fullName => '$firstName $lastName'.trim();

  factory UserModel.fromJson(Map<String, dynamic> json) =>
      _$UserModelFromJson(json);

  Map<String, dynamic> toJson() => _$UserModelToJson(this);

  UserModel copyWith({
    int? id,
    String? username,
    String? email,
    String? firstName,
    String? lastName,
    String? phone,
    String? address,
    String? avatarUrl,
    bool? isAdmin,
    DateTime? createdAt,
    DateTime? updatedAt,
  }) {
    return UserModel(
      id: id ?? this.id,
      username: username ?? this.username,
      email: email ?? this.email,
      firstName: firstName ?? this.firstName,
      lastName: lastName ?? this.lastName,
      phone: phone ?? this.phone,
      address: address ?? this.address,
      avatarUrl: avatarUrl ?? this.avatarUrl,
      isAdmin: isAdmin ?? this.isAdmin,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }

  @override
  List<Object?> get props => [
        id,
        username,
        email,
        firstName,
        lastName,
        phone,
        address,
        avatarUrl,
        isAdmin,
        createdAt,
        updatedAt,
      ];
}

@JsonSerializable()
class RegisterUserModel extends Equatable {
  final String username;
  final String email;
  final String password;
  final String confirmPassword;
  final String firstName;
  final String lastName;
  final String phone;
  final String address;

  const RegisterUserModel({
    required this.username,
    required this.email,
    required this.password,
    required this.confirmPassword,
    required this.firstName,
    required this.lastName,
    required this.phone,
    required this.address,
  });

  factory RegisterUserModel.fromJson(Map<String, dynamic> json) =>
      _$RegisterUserModelFromJson(json);

  Map<String, dynamic> toJson() => _$RegisterUserModelToJson(this);

  @override
  List<Object> get props => [
        username,
        email,
        password,
        confirmPassword,
        firstName,
        lastName,
        phone,
        address,
      ];
}

@JsonSerializable()
class LoginUserModel extends Equatable {
  final String username;
  final String password;

  const LoginUserModel({
    required this.username,
    required this.password,
  });

  factory LoginUserModel.fromJson(Map<String, dynamic> json) =>
      _$LoginUserModelFromJson(json);

  Map<String, dynamic> toJson() => _$LoginUserModelToJson(this);

  @override
  List<Object> get props => [username, password];
}

@JsonSerializable()
class UpdateUserModel extends Equatable {
  final String firstName;
  final String lastName;
  final String? phone;
  final String? address;

  const UpdateUserModel({
    required this.firstName,
    required this.lastName,
    this.phone,
    this.address,
  });

  factory UpdateUserModel.fromJson(Map<String, dynamic> json) =>
      _$UpdateUserModelFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateUserModelToJson(this);

  @override
  List<Object?> get props => [firstName, lastName, phone, address];
}
