import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../constants/api_constants.dart';
import '../errors/api_exception.dart';

class ApiClient {
  late final Dio _dio;
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  ApiClient() {
    _dio = Dio(
      BaseOptions(
        baseUrl: ApiConstants.baseUrl,
        connectTimeout: const Duration(
          milliseconds: ApiConstants.connectionTimeout,
        ),
        receiveTimeout: const Duration(
          milliseconds: ApiConstants.receiveTimeout,
        ),
        headers: {'Content-Type': ApiConstants.contentType},
      ),
    );

    _setupInterceptors();
  }

  void _setupInterceptors() {
    // Request interceptor - add auth token
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await _storage.read(key: ApiConstants.tokenKey);
          if (token != null && token.isNotEmpty) {
            options.headers[ApiConstants.authorization] =
                '${ApiConstants.bearer} $token';
          }
          handler.next(options);
        },
        onError: (error, handler) {
          final apiException = _handleError(error);
          handler.reject(
            DioException(
              requestOptions: error.requestOptions,
              error: apiException,
              type: DioExceptionType.unknown,
            ),
          );
        },
      ),
    );

    // Logging interceptor (only in debug mode)
    if (kDebugMode) {
      _dio.interceptors.add(
        LogInterceptor(
          requestBody: true,
          responseBody: true,
          requestHeader: true,
          responseHeader: true,
          error: true,
          logPrint: (obj) => debugPrint('[API] $obj'),
        ),
      );
    }
  }

  ApiException _handleError(DioException error) {
    switch (error.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
        return ApiException(message: 'Connection timeout', statusCode: 408);
      case DioExceptionType.badResponse:
        final statusCode = error.response?.statusCode ?? 0;
        final message = _extractErrorMessage(error.response?.data);
        return ApiException(message: message, statusCode: statusCode);
      case DioExceptionType.cancel:
        return ApiException(message: 'Request cancelled', statusCode: 0);
      default:
        return ApiException(message: 'Network error occurred', statusCode: 0);
    }
  }

  String _extractErrorMessage(dynamic data) {
    if (data is Map<String, dynamic>) {
      // Try to get detailed error message
      if (data.containsKey('message')) {
        return data['message'].toString();
      }
      if (data.containsKey('error')) {
        return data['error'].toString();
      }
      if (data.containsKey('details')) {
        return data['details'].toString();
      }
      // If it's a validation error, try to extract field errors
      if (data.containsKey('errors')) {
        final errors = data['errors'];
        if (errors is Map<String, dynamic>) {
          final errorMessages = <String>[];
          errors.forEach((field, messages) {
            if (messages is List) {
              errorMessages.addAll(messages.map((m) => '$field: $m'));
            } else {
              errorMessages.add('$field: $messages');
            }
          });
          if (errorMessages.isNotEmpty) {
            return errorMessages.join(', ');
          }
        }
      }
      return 'Unknown error occurred';
    }
    if (data is String && data.isNotEmpty) {
      return data;
    }
    return 'Unknown error occurred';
  }

  // GET request
  Future<T> get<T>(
    String endpoint, {
    Map<String, dynamic>? queryParameters,
    T Function(dynamic)? fromJson,
  }) async {
    try {
      final response = await _dio.get(
        endpoint,
        queryParameters: queryParameters,
      );

      if (fromJson != null) {
        return fromJson(response.data);
      }
      return response.data as T;
    } on DioException catch (e) {
      if (e.error is ApiException) {
        throw e.error as ApiException;
      }
      throw _handleError(e);
    }
  }

  // POST request
  Future<T> post<T>(
    String endpoint, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    T Function(dynamic)? fromJson,
  }) async {
    try {
      final response = await _dio.post(
        endpoint,
        data: data,
        queryParameters: queryParameters,
      );

      if (fromJson != null) {
        return fromJson(response.data);
      }
      return response.data as T;
    } on DioException catch (e) {
      if (e.error is ApiException) {
        throw e.error as ApiException;
      }
      throw _handleError(e);
    }
  }

  // PUT request
  Future<T> put<T>(
    String endpoint, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    T Function(dynamic)? fromJson,
  }) async {
    try {
      final response = await _dio.put(
        endpoint,
        data: data,
        queryParameters: queryParameters,
      );

      if (fromJson != null) {
        return fromJson(response.data);
      }
      return response.data as T;
    } on DioException catch (e) {
      if (e.error is ApiException) {
        throw e.error as ApiException;
      }
      throw _handleError(e);
    }
  }

  // DELETE request
  Future<T> delete<T>(
    String endpoint, {
    Map<String, dynamic>? queryParameters,
    T Function(dynamic)? fromJson,
  }) async {
    try {
      final response = await _dio.delete(
        endpoint,
        queryParameters: queryParameters,
      );

      if (fromJson != null) {
        return fromJson(response.data);
      }
      return response.data as T;
    } on DioException catch (e) {
      if (e.error is ApiException) {
        throw e.error as ApiException;
      }
      throw _handleError(e);
    }
  }

  // Set auth token
  Future<void> setAuthToken(String token) async {
    await _storage.write(key: ApiConstants.tokenKey, value: token);
  }

  // Clear auth token
  Future<void> clearAuthToken() async {
    await _storage.delete(key: ApiConstants.tokenKey);
  }

  // Get auth token
  Future<String?> getAuthToken() async {
    return await _storage.read(key: ApiConstants.tokenKey);
  }
}
