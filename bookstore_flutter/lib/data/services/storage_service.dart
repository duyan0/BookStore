import 'dart:convert';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../models/user_model.dart';
import '../models/cart_model.dart';
import '../../core/constants/api_constants.dart';

class StorageService {
  static const FlutterSecureStorage _secureStorage = FlutterSecureStorage();
  static SharedPreferences? _prefs;

  /// Initialize shared preferences
  static Future<void> init() async {
    _prefs ??= await SharedPreferences.getInstance();
  }

  /// Ensure preferences are initialized
  static Future<SharedPreferences> get _preferences async {
    if (_prefs == null) {
      await init();
    }
    return _prefs!;
  }

  // ===== SECURE STORAGE (for sensitive data) =====

  /// Save auth token securely
  static Future<void> saveAuthToken(String token) async {
    await _secureStorage.write(key: ApiConstants.tokenKey, value: token);
  }

  /// Get auth token
  static Future<String?> getAuthToken() async {
    return await _secureStorage.read(key: ApiConstants.tokenKey);
  }

  /// Clear auth token
  static Future<void> clearAuthToken() async {
    await _secureStorage.delete(key: ApiConstants.tokenKey);
  }

  /// Check if user is logged in
  static Future<bool> isLoggedIn() async {
    final token = await getAuthToken();
    return token != null && token.isNotEmpty;
  }

  // ===== SHARED PREFERENCES (for non-sensitive data) =====

  /// Save user data
  static Future<void> saveUserData(UserModel user) async {
    final prefs = await _preferences;
    final userJson = jsonEncode(user.toJson());
    await prefs.setString(ApiConstants.userKey, userJson);
    await prefs.setBool(ApiConstants.isAdminKey, user.isAdmin);
  }

  /// Get user data
  static Future<UserModel?> getUserData() async {
    try {
      final prefs = await _preferences;
      final userJson = prefs.getString(ApiConstants.userKey);
      if (userJson != null) {
        final userMap = jsonDecode(userJson) as Map<String, dynamic>;
        return UserModel.fromJson(userMap);
      }
      return null;
    } catch (e) {
      return null;
    }
  }

  /// Check if user is admin
  static Future<bool> isAdmin() async {
    final prefs = await _preferences;
    return prefs.getBool(ApiConstants.isAdminKey) ?? false;
  }

  /// Clear user data
  static Future<void> clearUserData() async {
    final prefs = await _preferences;
    await prefs.remove(ApiConstants.userKey);
    await prefs.remove(ApiConstants.isAdminKey);
  }

  // ===== CART MANAGEMENT =====

  static const String _cartKey = 'cart_data';

  /// Save cart data
  static Future<void> saveCart(CartModel cart) async {
    try {
      final prefs = await _preferences;
      final cartJson = jsonEncode(cart.toJson());
      await prefs.setString(_cartKey, cartJson);
    } catch (e) {
      // Handle error silently
    }
  }

  /// Get cart data
  static Future<CartModel> getCart() async {
    try {
      final prefs = await _preferences;
      final cartJson = prefs.getString(_cartKey);
      if (cartJson != null) {
        final cartMap = jsonDecode(cartJson) as Map<String, dynamic>;
        return CartModel.fromJson(cartMap);
      }
      return CartModel.empty();
    } catch (e) {
      return CartModel.empty();
    }
  }

  /// Clear cart data
  static Future<void> clearCart() async {
    final prefs = await _preferences;
    await prefs.remove(_cartKey);
  }

  // ===== APP SETTINGS =====

  static const String _themeKey = 'theme_mode';
  static const String _languageKey = 'language_code';
  static const String _firstLaunchKey = 'first_launch';

  /// Save theme mode
  static Future<void> saveThemeMode(String themeMode) async {
    final prefs = await _preferences;
    await prefs.setString(_themeKey, themeMode);
  }

  /// Get theme mode
  static Future<String> getThemeMode() async {
    final prefs = await _preferences;
    return prefs.getString(_themeKey) ?? 'system';
  }

  /// Save language code
  static Future<void> saveLanguageCode(String languageCode) async {
    final prefs = await _preferences;
    await prefs.setString(_languageKey, languageCode);
  }

  /// Get language code
  static Future<String> getLanguageCode() async {
    final prefs = await _preferences;
    return prefs.getString(_languageKey) ?? 'vi';
  }

  /// Check if this is first launch
  static Future<bool> isFirstLaunch() async {
    final prefs = await _preferences;
    return prefs.getBool(_firstLaunchKey) ?? true;
  }

  /// Set first launch completed
  static Future<void> setFirstLaunchCompleted() async {
    final prefs = await _preferences;
    await prefs.setBool(_firstLaunchKey, false);
  }

  // ===== SEARCH HISTORY =====

  static const String _searchHistoryKey = 'search_history';
  static const int _maxSearchHistory = 10;

  /// Save search term to history
  static Future<void> saveSearchTerm(String term) async {
    if (term.trim().isEmpty) return;
    
    final prefs = await _preferences;
    final history = await getSearchHistory();
    
    // Remove if already exists
    history.remove(term);
    
    // Add to beginning
    history.insert(0, term);
    
    // Keep only max items
    if (history.length > _maxSearchHistory) {
      history.removeRange(_maxSearchHistory, history.length);
    }
    
    await prefs.setStringList(_searchHistoryKey, history);
  }

  /// Get search history
  static Future<List<String>> getSearchHistory() async {
    final prefs = await _preferences;
    return prefs.getStringList(_searchHistoryKey) ?? [];
  }

  /// Clear search history
  static Future<void> clearSearchHistory() async {
    final prefs = await _preferences;
    await prefs.remove(_searchHistoryKey);
  }

  // ===== CLEAR ALL DATA =====

  /// Clear all app data (logout)
  static Future<void> clearAllData() async {
    await clearAuthToken();
    await clearUserData();
    await clearCart();
    // Keep app settings and search history
  }

  /// Clear all data including settings (reset app)
  static Future<void> resetApp() async {
    await _secureStorage.deleteAll();
    final prefs = await _preferences;
    await prefs.clear();
  }
}
