import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final apiClientProvider = Provider<ApiClient>((ref) {
  return ApiClient();
});

class ApiClient {
  ApiClient()
      : _dio = Dio(
          BaseOptions(
            baseUrl: const String.fromEnvironment(
              'UNIFY_API_URL',
              defaultValue: 'http://localhost:5080',
            ),
            connectTimeout: const Duration(seconds: 10),
            receiveTimeout: const Duration(seconds: 20),
            headers: {'Content-Type': 'application/json'},
          ),
        );

  final Dio _dio;

  Future<Map<String, dynamic>> login({
    required String email,
    required String password,
    required String deviceName,
  }) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/auth/login',
      data: {
        'email': email,
        'password': password,
        'deviceName': deviceName,
      },
    );

    return response.data ?? {};
  }

  Future<void> forgotPassword(String email) async {
    await _dio.post<void>(
      '/api/v1/auth/forgot-password',
      data: {'email': email},
    );
  }

  Future<void> resetPassword({
    required String email,
    required String resetToken,
    required String newPassword,
  }) async {
    await _dio.post<void>(
      '/api/v1/auth/reset-password',
      data: {
        'email': email,
        'resetToken': resetToken,
        'newPassword': newPassword,
      },
    );
  }

  Future<void> changePassword({
    required String accessToken,
    required String currentPassword,
    required String newPassword,
  }) async {
    await _dio.post<void>(
      '/api/v1/auth/change-password',
      data: {
        'currentPassword': currentPassword,
        'newPassword': newPassword,
      },
      options: Options(headers: {'Authorization': 'Bearer $accessToken'}),
    );
  }

  Future<Map<String, dynamic>> health() async {
    final response =
        await _dio.get<Map<String, dynamic>>('/api/v1/system/health');

    return response.data ?? {};
  }

  Future<List<Map<String, dynamic>>> listAccessPermissions(
      String accessToken) async {
    final response = await _dio.get<List<dynamic>>(
      '/api/v1/access/permissions',
      options: _auth(accessToken),
    );

    return (response.data ?? []).whereType<Map<String, dynamic>>().toList();
  }

  Future<List<Map<String, dynamic>>> listAccessUsers(String accessToken) async {
    final response = await _dio.get<List<dynamic>>(
      '/api/v1/access/users',
      options: _auth(accessToken),
    );

    return (response.data ?? []).whereType<Map<String, dynamic>>().toList();
  }

  Future<Map<String, dynamic>> createAccessUser(
    String accessToken, {
    required String email,
    required String displayName,
    required String password,
    required List<String> permissions,
  }) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/access/users',
      data: {
        'email': email,
        'displayName': displayName,
        'password': password,
        'permissions': permissions,
      },
      options: _auth(accessToken),
    );

    return response.data ?? {};
  }

  Future<Map<String, dynamic>> updateAccessUserPermissions(
    String accessToken, {
    required String userId,
    required List<String> permissions,
  }) async {
    final response = await _dio.put<Map<String, dynamic>>(
      '/api/v1/access/users/$userId/permissions',
      data: {'permissions': permissions},
      options: _auth(accessToken),
    );

    return response.data ?? {};
  }

  Future<Map<String, dynamic>> setAccessUserDisabled(
    String accessToken, {
    required String userId,
    required bool disabled,
  }) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/access/users/$userId/${disabled ? 'disable' : 'enable'}',
      options: _auth(accessToken),
    );

    return response.data ?? {};
  }

  Future<List<Map<String, dynamic>>> listOrganisations(
      String accessToken) async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/platform/organisations',
      queryParameters: {'pageNumber': 1, 'pageSize': 50},
      options: _auth(accessToken),
    );

    return _items(response.data);
  }

  Future<List<Map<String, dynamic>>> listBranches(
      String accessToken, String organisationId) async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/platform/organisations/$organisationId/branches',
      queryParameters: {'pageNumber': 1, 'pageSize': 50},
      options: _auth(accessToken),
    );

    return _items(response.data);
  }

  Future<List<Map<String, dynamic>>> listWarehouses(
      String accessToken, String organisationId) async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/platform/organisations/$organisationId/warehouses',
      queryParameters: {'pageNumber': 1, 'pageSize': 50},
      options: _auth(accessToken),
    );

    return _items(response.data);
  }

  Future<List<Map<String, dynamic>>> listCustomers(
    String accessToken,
    String organisationId, {
    String? search,
  }) async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/customers',
      queryParameters: {
        'organisationId': organisationId,
        if (search != null && search.trim().isNotEmpty) 'search': search.trim(),
        'pageNumber': 1,
        'pageSize': 100,
      },
      options: _auth(accessToken),
    );

    return _items(response.data);
  }

  Future<Map<String, dynamic>> createCustomer(
    String accessToken, {
    required String organisationId,
    required String branchId,
    required String customerNumber,
    required String displayName,
    required String phone,
    required String email,
    required double creditLimit,
  }) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/customers/',
      data: {
        'organisationId': organisationId,
        'branchId': branchId,
        'customerNumber': customerNumber,
        'displayName': displayName,
        'legalName': displayName,
        'phone': phone,
        'email': email,
        'taxNumber': null,
        'creditLimit': creditLimit,
      },
      options: _auth(accessToken),
    );

    return response.data ?? {};
  }

  Future<List<Map<String, dynamic>>> listProducts(
      String accessToken, String organisationId) async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/products',
      queryParameters: {
        'organisationId': organisationId,
        'pageNumber': 1,
        'pageSize': 100
      },
      options: _auth(accessToken),
    );

    return _items(response.data);
  }

  Future<List<Map<String, dynamic>>> listSales(
    String accessToken,
    String organisationId, {
    String? customerId,
  }) async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/sales',
      queryParameters: {
        'organisationId': organisationId,
        if (customerId != null) 'customerId': customerId,
        'pageNumber': 1,
        'pageSize': 100,
      },
      options: _auth(accessToken),
    );

    return _items(response.data);
  }

  Future<Map<String, dynamic>> createSale(
    String accessToken, {
    required String organisationId,
    required String branchId,
    required String warehouseId,
    required String customerId,
    required String productId,
    required String description,
    required double quantity,
    required double unitPrice,
  }) async {
    final invoiceNumber = 'INV-${DateTime.now().millisecondsSinceEpoch}';
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/sales/',
      data: {
        'organisationId': organisationId,
        'branchId': branchId,
        'warehouseId': warehouseId,
        'customerId': customerId,
        'invoiceNumber': invoiceNumber,
        'saleDateUtc': DateTime.now().toUtc().toIso8601String(),
        'items': [
          {
            'productId': productId,
            'description': description,
            'quantity': quantity,
            'unitPrice': unitPrice,
            'discountAmount': 0,
            'taxAmount': 0,
          }
        ],
      },
      options: _auth(accessToken),
    );

    return response.data ?? {};
  }

  Options _auth(String accessToken) {
    return Options(headers: {'Authorization': 'Bearer $accessToken'});
  }

  List<Map<String, dynamic>> _items(Map<String, dynamic>? response) {
    final rawItems = response?['items'];
    if (rawItems is! List) {
      return [];
    }

    return rawItems.whereType<Map<String, dynamic>>().toList();
  }
}
