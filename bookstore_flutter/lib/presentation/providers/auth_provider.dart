import 'package:flutter/foundation.dart';
import '../../data/models/user_model.dart';

import '../../data/services/auth_service.dart';
import '../../data/services/storage_service.dart';
import '../../core/errors/api_exception.dart';

enum AuthState { initial, loading, authenticated, unauthenticated, error }

class AuthProvider extends ChangeNotifier {
  final AuthService _authService;

  AuthProvider(this._authService) {
    _checkAuthStatus();
  }

  AuthState _state = AuthState.initial;
  UserModel? _user;
  String? _errorMessage;
  bool _isLoading = false;

  // Getters
  AuthState get state => _state;
  UserModel? get user => _user;
  String? get errorMessage => _errorMessage;
  bool get isLoading => _isLoading;
  bool get isAuthenticated =>
      _state == AuthState.authenticated && _user != null;
  bool get isAdmin => _user?.isAdmin ?? false;
  bool get isUser => !isAdmin && isAuthenticated;

  /// Kiểm tra trạng thái đăng nhập khi khởi tạo
  Future<void> _checkAuthStatus() async {
    try {
      _setLoading(true);

      final isLoggedIn = await _authService.isLoggedIn();
      if (isLoggedIn) {
        final user = await _authService.getCurrentUser();
        if (user != null) {
          _user = user;
          _setState(AuthState.authenticated);
        } else {
          _setState(AuthState.unauthenticated);
        }
      } else {
        _setState(AuthState.unauthenticated);
      }
    } catch (e) {
      _setError('Failed to check auth status: ${e.toString()}');
    } finally {
      _setLoading(false);
    }
  }

  /// Đăng nhập
  Future<bool> login(String username, String password) async {
    try {
      _setLoading(true);
      _clearError();

      final loginData = LoginUserModel(username: username, password: password);

      final response = await _authService.login(loginData);

      if (response.success && response.user != null) {
        _user = response.user;
        _setState(AuthState.authenticated);
        print(
          'Login successful: user=${_user?.username}, isAdmin=${_user?.isAdmin}',
        );
        return true;
      } else {
        _setError(response.message);
        return false;
      }
    } on ApiException catch (e) {
      _setError(e.message);
      return false;
    } catch (e) {
      _setError('Login failed: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Đăng ký
  Future<bool> register({
    required String username,
    required String email,
    required String password,
    required String confirmPassword,
    required String firstName,
    required String lastName,
    required String phone,
    required String address,
  }) async {
    try {
      _setLoading(true);
      _clearError();

      if (password != confirmPassword) {
        _setError('Passwords do not match');
        return false;
      }

      final registerData = RegisterUserModel(
        username: username,
        email: email,
        password: password,
        confirmPassword: confirmPassword,
        firstName: firstName,
        lastName: lastName,
        phone: phone,
        address: address,
      );

      final response = await _authService.register(registerData);

      if (response.success && response.user != null) {
        _user = response.user;
        _setState(AuthState.authenticated);
        return true;
      } else {
        _setError(response.message);
        return false;
      }
    } on ApiException catch (e) {
      _setError(e.message);
      return false;
    } catch (e) {
      _setError('Registration failed: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Đăng xuất
  Future<void> logout() async {
    try {
      _setLoading(true);
      await _authService.logout();
      _user = null;
      _setState(AuthState.unauthenticated);
    } catch (e) {
      _setError('Logout failed: ${e.toString()}');
    } finally {
      _setLoading(false);
    }
  }

  /// Cập nhật thông tin user
  Future<bool> updateUser(UserModel updatedUser) async {
    try {
      _setLoading(true);
      _clearError();

      // Update user in storage
      await StorageService.saveUserData(updatedUser);
      _user = updatedUser;
      notifyListeners();
      return true;
    } catch (e) {
      _setError('Failed to update user: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Refresh user data
  Future<void> refreshUser() async {
    try {
      final user = await _authService.getCurrentUser();
      if (user != null) {
        _user = user;
        notifyListeners();
      }
    } catch (e) {
      // Handle error silently
    }
  }

  /// Clear error message
  void clearError() {
    _clearError();
  }

  // Private methods
  void _setState(AuthState newState) {
    _state = newState;
    notifyListeners();
  }

  void _setLoading(bool loading) {
    _isLoading = loading;
    notifyListeners();
  }

  void _setError(String error) {
    _errorMessage = error;
    _state = AuthState.error;
    notifyListeners();
  }

  void _clearError() {
    _errorMessage = null;
    if (_state == AuthState.error) {
      _state =
          _user != null ? AuthState.authenticated : AuthState.unauthenticated;
    }
    notifyListeners();
  }
}
