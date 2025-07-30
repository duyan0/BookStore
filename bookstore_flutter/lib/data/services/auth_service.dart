import '../../core/network/api_client.dart';
import '../../core/constants/api_constants.dart';
import '../../core/errors/api_exception.dart';
import '../models/auth_model.dart';
import '../models/user_model.dart';
import 'storage_service.dart';

class AuthService {
  final ApiClient _apiClient;

  AuthService(this._apiClient);

  /// Đăng nhập
  Future<AuthResponseModel> login(LoginUserModel loginData) async {
    try {
      final response = await _apiClient.post<Map<String, dynamic>>(
        ApiConstants.login,
        data: loginData.toJson(),
      );

      final authResponse = AuthResponseModel.fromJson(response);

      if (authResponse.success && authResponse.token.isNotEmpty) {
        // Lưu token vào secure storage
        await _apiClient.setAuthToken(authResponse.token);

        // Lưu thông tin user
        if (authResponse.user != null) {
          await _saveUserData(authResponse.user!);
        }
      }

      return authResponse;
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Login failed: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Đăng ký
  Future<AuthResponseModel> register(RegisterUserModel registerData) async {
    try {
      final response = await _apiClient.post<Map<String, dynamic>>(
        ApiConstants.register,
        data: registerData.toJson(),
      );

      final authResponse = AuthResponseModel.fromJson(response);

      if (authResponse.success && authResponse.token.isNotEmpty) {
        // Lưu token vào secure storage
        await _apiClient.setAuthToken(authResponse.token);

        // Lưu thông tin user
        if (authResponse.user != null) {
          await _saveUserData(authResponse.user!);
        }
      }

      return authResponse;
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException(
        message: 'Registration failed: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Đăng xuất
  Future<void> logout() async {
    try {
      // Clear token from secure storage
      await _apiClient.clearAuthToken();

      // Clear user data from shared preferences
      await _clearUserData();
    } catch (e) {
      throw ApiException(
        message: 'Logout failed: ${e.toString()}',
        statusCode: 0,
      );
    }
  }

  /// Kiểm tra xem user đã đăng nhập chưa
  Future<bool> isLoggedIn() async {
    try {
      final token = await _apiClient.getAuthToken();
      return token != null && token.isNotEmpty;
    } catch (e) {
      return false;
    }
  }

  /// Lấy thông tin user hiện tại
  Future<UserModel?> getCurrentUser() async {
    try {
      return await _getUserData();
    } catch (e) {
      return null;
    }
  }

  /// Kiểm tra xem user có phải admin không
  Future<bool> isAdmin() async {
    try {
      final user = await getCurrentUser();
      return user?.isAdmin ?? false;
    } catch (e) {
      return false;
    }
  }

  /// Refresh token (nếu API hỗ trợ)
  Future<AuthResponseModel?> refreshToken() async {
    try {
      // Implement refresh token logic if API supports it
      // For now, return null
      return null;
    } catch (e) {
      return null;
    }
  }

  /// Lưu thông tin user vào shared preferences
  Future<void> _saveUserData(UserModel user) async {
    try {
      await StorageService.saveUserData(user);
    } catch (e) {
      // Handle error silently
    }
  }

  /// Lấy thông tin user từ shared preferences
  Future<UserModel?> _getUserData() async {
    try {
      return await StorageService.getUserData();
    } catch (e) {
      return null;
    }
  }

  /// Xóa thông tin user từ shared preferences
  Future<void> _clearUserData() async {
    try {
      await StorageService.clearUserData();
    } catch (e) {
      // Handle error silently
    }
  }
}
