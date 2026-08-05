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

  String get baseUrl => _dio.options.baseUrl;

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

  Future<List<Map<String, dynamic>>> listInventoryBalances(
    String accessToken,
    String organisationId, {
    String? warehouseId,
  }) async {
    final response = await _dio.get<List<dynamic>>(
      '/api/v1/inventory/balances',
      queryParameters: {
        'organisationId': organisationId,
        if (warehouseId != null) 'warehouseId': warehouseId,
      },
      options: _auth(accessToken),
    );

    return (response.data ?? []).whereType<Map<String, dynamic>>().toList();
  }

  Future<List<Map<String, dynamic>>> listInventoryMovements(
    String accessToken,
    String organisationId, {
    String? warehouseId,
    String? productId,
  }) async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/inventory/movements',
      queryParameters: {
        'organisationId': organisationId,
        if (warehouseId != null) 'warehouseId': warehouseId,
        if (productId != null) 'productId': productId,
        'pageNumber': 1,
        'pageSize': 100,
      },
      options: _auth(accessToken),
    );

    return _items(response.data);
  }

  Future<Map<String, dynamic>> createStockAdjustment(
    String accessToken, {
    required String organisationId,
    required String warehouseId,
    required String productId,
    required String movementType,
    required double quantity,
    required String notes,
  }) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/inventory/adjustments',
      data: {
        'organisationId': organisationId,
        'warehouseId': warehouseId,
        'productId': productId,
        'movementType': movementType,
        'quantity': quantity,
        'notes': notes,
      },
      options: _auth(accessToken),
    );

    return response.data ?? {};
  }

  Future<List<Map<String, dynamic>>> listSuppliers(
    String accessToken,
    String organisationId,
  ) async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/suppliers',
      queryParameters: {
        'organisationId': organisationId,
        'pageNumber': 1,
        'pageSize': 100,
      },
      options: _auth(accessToken),
    );

    return _items(response.data);
  }

  Future<Map<String, dynamic>> createSupplier(
    String accessToken, {
    required String organisationId,
    required String supplierNumber,
    required String displayName,
    required String phone,
    required String email,
  }) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/suppliers/',
      data: {
        'organisationId': organisationId,
        'supplierNumber': supplierNumber,
        'displayName': displayName,
        'legalName': displayName,
        'phone': phone,
        'email': email,
        'taxNumber': null,
      },
      options: _auth(accessToken),
    );

    return response.data ?? {};
  }

  Future<List<Map<String, dynamic>>> listPurchaseOrders(
    String accessToken,
    String organisationId,
  ) async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/purchasing/orders',
      queryParameters: {
        'organisationId': organisationId,
        'pageNumber': 1,
        'pageSize': 100,
      },
      options: _auth(accessToken),
    );

    return _items(response.data);
  }

  Future<Map<String, dynamic>> createPurchaseOrder(
    String accessToken, {
    required String organisationId,
    required String branchId,
    required String supplierId,
    required String productId,
    required String description,
    required double quantity,
    required double unitCost,
  }) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/purchasing/orders',
      data: {
        'organisationId': organisationId,
        'branchId': branchId,
        'supplierId': supplierId,
        'orderNumber': 'PO-${DateTime.now().millisecondsSinceEpoch}',
        'orderDateUtc': DateTime.now().toUtc().toIso8601String(),
        'items': [
          {
            'productId': productId,
            'description': description,
            'quantity': quantity,
            'unitCost': unitCost,
            'taxAmount': 0,
          }
        ],
      },
      options: _auth(accessToken),
    );

    return response.data ?? {};
  }

  Future<List<Map<String, dynamic>>> listAccounts(
      String accessToken, String organisationId) async {
    final response = await _dio.get<List<dynamic>>(
      '/api/v1/accounting/accounts',
      queryParameters: {'organisationId': organisationId},
      options: _auth(accessToken),
    );

    return (response.data ?? []).whereType<Map<String, dynamic>>().toList();
  }

  Future<List<Map<String, dynamic>>> listFiscalPeriods(
      String accessToken, String organisationId) async {
    final response = await _dio.get<List<dynamic>>(
      '/api/v1/accounting/fiscal-periods',
      queryParameters: {'organisationId': organisationId},
      options: _auth(accessToken),
    );

    return (response.data ?? []).whereType<Map<String, dynamic>>().toList();
  }

  Future<Map<String, dynamic>> createAccount(
    String accessToken, {
    required String organisationId,
    required String code,
    required String name,
    required String type,
  }) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/accounting/accounts',
      data: {
        'organisationId': organisationId,
        'code': code,
        'name': name,
        'type': type,
      },
      options: _auth(accessToken),
    );

    return response.data ?? {};
  }

  Future<Map<String, dynamic>> createJournal(
    String accessToken, {
    required String organisationId,
    required String debitAccountId,
    required String creditAccountId,
    required String description,
    required double amount,
  }) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/accounting/journals',
      data: {
        'organisationId': organisationId,
        'journalNumber': 'JE-${DateTime.now().millisecondsSinceEpoch}',
        'journalDate': DateTime.now().toIso8601String().substring(0, 10),
        'description': description,
        'lines': [
          {
            'accountId': debitAccountId,
            'description': description,
            'debit': amount,
            'credit': 0,
          },
          {
            'accountId': creditAccountId,
            'description': description,
            'debit': 0,
            'credit': amount,
          },
        ],
      },
      options: _auth(accessToken),
    );

    return response.data ?? {};
  }

  Future<Map<String, dynamic>> getSalesReport(
    String accessToken, {
    required String organisationId,
    DateTime? fromUtc,
    DateTime? toUtc,
    String? customerId,
    String? productId,
  }) async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/reports/sales',
      queryParameters: {
        'organisationId': organisationId,
        if (fromUtc != null) 'fromUtc': fromUtc.toUtc().toIso8601String(),
        if (toUtc != null) 'toUtc': toUtc.toUtc().toIso8601String(),
        if (customerId != null && customerId.isNotEmpty)
          'customerId': customerId,
        if (productId != null && productId.isNotEmpty) 'productId': productId,
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
