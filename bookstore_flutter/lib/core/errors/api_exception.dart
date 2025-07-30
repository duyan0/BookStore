class ApiException implements Exception {
  final String message;
  final int statusCode;
  final dynamic data;

  const ApiException({
    required this.message,
    required this.statusCode,
    this.data,
  });

  bool get isUnauthorized => statusCode == 401;
  bool get isForbidden => statusCode == 403;
  bool get isNotFound => statusCode == 404;
  bool get isServerError => statusCode >= 500;
  bool get isClientError => statusCode >= 400 && statusCode < 500;

  @override
  String toString() {
    return 'ApiException: $message (Status: $statusCode)';
  }
}

class NetworkException implements Exception {
  final String message;

  const NetworkException(this.message);

  @override
  String toString() {
    return 'NetworkException: $message';
  }
}

class ValidationException implements Exception {
  final String message;
  final Map<String, List<String>>? errors;

  const ValidationException({
    required this.message,
    this.errors,
  });

  @override
  String toString() {
    return 'ValidationException: $message';
  }
}

class AuthException implements Exception {
  final String message;

  const AuthException(this.message);

  @override
  String toString() {
    return 'AuthException: $message';
  }
}
