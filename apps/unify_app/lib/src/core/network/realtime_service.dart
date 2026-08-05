import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:signalr_netcore/signalr_client.dart';

import 'api_client.dart';

final realtimeServiceProvider = Provider<RealtimeService>((ref) {
  final service = RealtimeService(ref.watch(apiClientProvider));
  ref.onDispose(service.dispose);
  return service;
});

class RealtimeService {
  RealtimeService(this._apiClient);

  final ApiClient _apiClient;
  final _changes = StreamController<OperationChanged>.broadcast();
  HubConnection? _connection;
  String? _joinedOrganisationId;

  Stream<OperationChanged> get changes => _changes.stream;

  Future<void> connect({
    required String accessToken,
    required String organisationId,
  }) async {
    if (_connection?.state == HubConnectionState.Connected &&
        _joinedOrganisationId == organisationId) {
      return;
    }

    await disconnect();

    final hubUrl = '${_apiClient.baseUrl}/hubs/operations';
    final connection = HubConnectionBuilder()
        .withUrl(
          hubUrl,
          options: HttpConnectionOptions(
            accessTokenFactory: () async => accessToken,
          ),
        )
        .withAutomaticReconnect()
        .build();

    connection.on('operationChanged', (arguments) {
      final first = arguments?.isNotEmpty == true ? arguments!.first : null;
      if (first is Map) {
        _changes.add(OperationChanged.fromMap(first));
      }
    });

    await connection.start();
    await connection.invoke('JoinOrganisation', args: [organisationId]);
    _connection = connection;
    _joinedOrganisationId = organisationId;
  }

  Future<void> disconnect() async {
    final connection = _connection;
    if (connection == null) {
      return;
    }

    final organisationId = _joinedOrganisationId;
    if (organisationId != null &&
        connection.state == HubConnectionState.Connected) {
      await connection.invoke('LeaveOrganisation', args: [organisationId]);
    }

    await connection.stop();
    _connection = null;
    _joinedOrganisationId = null;
  }

  void dispose() {
    disconnect();
    _changes.close();
  }
}

class OperationChanged {
  const OperationChanged({
    required this.module,
    required this.action,
    required this.organisationId,
    required this.entityId,
    required this.changedAtUtc,
  });

  final String module;
  final String action;
  final String organisationId;
  final String entityId;
  final DateTime? changedAtUtc;

  factory OperationChanged.fromMap(Map<dynamic, dynamic> map) {
    return OperationChanged(
      module: '${map['module'] ?? ''}',
      action: '${map['action'] ?? ''}',
      organisationId: '${map['organisationId'] ?? ''}',
      entityId: '${map['entityId'] ?? ''}',
      changedAtUtc: DateTime.tryParse('${map['changedAtUtc'] ?? ''}'),
    );
  }
}
