import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/network/api_client.dart';

final authControllerProvider = ChangeNotifierProvider<AuthController>((ref) {
  return AuthController(ref.watch(apiClientProvider));
});

class AuthController extends ChangeNotifier {
  AuthController(this._apiClient);

  final ApiClient _apiClient;
  bool _isAuthenticated = false;
  bool _isBusy = false;
  String? _accessToken;
  String? _email;
  String? _error;

  bool get isAuthenticated => _isAuthenticated;

  bool get isBusy => _isBusy;

  String? get accessToken => _accessToken;

  String? get email => _email;

  String? get error => _error;

  String get displayEmail => _email ?? 'owner@unify.local';

  Future<bool> login({
    required String email,
    required String password,
  }) async {
    _setBusy(true);
    _error = null;

    try {
      final result = await _apiClient.login(
        email: email,
        password: password,
        deviceName: defaultTargetPlatform.name,
      );
      _accessToken = result['accessToken'] as String?;
      _email = email;
      _isAuthenticated = _accessToken != null && _accessToken!.isNotEmpty;
      return _isAuthenticated;
    } catch (_) {
      _error = 'Unable to sign in. Check the API is running and credentials are correct.';
      return false;
    } finally {
      _setBusy(false);
    }
  }

  Future<bool> forgotPassword(String email) async {
    _setBusy(true);
    _error = null;

    try {
      await _apiClient.forgotPassword(email);
      return true;
    } catch (_) {
      _error = 'Password reset request could not be sent.';
      return false;
    } finally {
      _setBusy(false);
    }
  }

  Future<bool> resetPassword({
    required String email,
    required String resetToken,
    required String newPassword,
  }) async {
    _setBusy(true);
    _error = null;

    try {
      await _apiClient.resetPassword(
        email: email,
        resetToken: resetToken,
        newPassword: newPassword,
      );
      return true;
    } catch (_) {
      _error = 'Password reset failed. Check the token and password requirements.';
      return false;
    } finally {
      _setBusy(false);
    }
  }

  Future<bool> changePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    final token = _accessToken;
    if (token == null) {
      _error = 'Sign in again before changing your password.';
      notifyListeners();
      return false;
    }

    _setBusy(true);
    _error = null;

    try {
      await _apiClient.changePassword(
        accessToken: token,
        currentPassword: currentPassword,
        newPassword: newPassword,
      );
      logout();
      return true;
    } catch (_) {
      _error = 'Password change failed. Check your current password.';
      return false;
    } finally {
      _setBusy(false);
    }
  }

  void logout() {
    _isAuthenticated = false;
    _accessToken = null;
    _email = null;
    notifyListeners();
  }

  void _setBusy(bool value) {
    _isBusy = value;
    notifyListeners();
  }
}
