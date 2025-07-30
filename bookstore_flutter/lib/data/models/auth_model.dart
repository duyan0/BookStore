import 'package:json_annotation/json_annotation.dart';
import 'package:equatable/equatable.dart';
import 'user_model.dart';

part 'auth_model.g.dart';

@JsonSerializable()
class AuthResponseModel extends Equatable {
  final bool success;
  final String message;
  final String token;
  final UserModel? user;

  const AuthResponseModel({
    required this.success,
    required this.message,
    required this.token,
    this.user,
  });

  factory AuthResponseModel.fromJson(Map<String, dynamic> json) =>
      _$AuthResponseModelFromJson(json);

  Map<String, dynamic> toJson() => _$AuthResponseModelToJson(this);

  AuthResponseModel copyWith({
    bool? success,
    String? message,
    String? token,
    UserModel? user,
  }) {
    return AuthResponseModel(
      success: success ?? this.success,
      message: message ?? this.message,
      token: token ?? this.token,
      user: user ?? this.user,
    );
  }

  @override
  List<Object?> get props => [success, message, token, user];
}

@JsonSerializable()
class AuthTokenModel extends Equatable {
  final String token;
  final DateTime expiresAt;
  final UserModel user;

  const AuthTokenModel({
    required this.token,
    required this.expiresAt,
    required this.user,
  });

  factory AuthTokenModel.fromJson(Map<String, dynamic> json) =>
      _$AuthTokenModelFromJson(json);

  Map<String, dynamic> toJson() => _$AuthTokenModelToJson(this);

  bool get isExpired => DateTime.now().isAfter(expiresAt);
  bool get isValid => !isExpired && token.isNotEmpty;

  AuthTokenModel copyWith({
    String? token,
    DateTime? expiresAt,
    UserModel? user,
  }) {
    return AuthTokenModel(
      token: token ?? this.token,
      expiresAt: expiresAt ?? this.expiresAt,
      user: user ?? this.user,
    );
  }

  @override
  List<Object> get props => [token, expiresAt, user];
}
