import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/layout/app_breakpoints.dart';
import '../../core/network/api_client.dart';
import '../../core/network/realtime_service.dart';
import '../../core/theme/app_theme.dart';
import '../auth/auth_controller.dart';

class ModulePage extends StatelessWidget {
  const ModulePage({required this.moduleId, super.key});

  final String moduleId;

  @override
  Widget build(BuildContext context) {
    final module = ModuleCatalog.byId(moduleId);

    if (module.id == 'settings') {
      return const _SettingsWorkspace();
    }

    if (module.id == 'customers') {
      return const _CustomersWorkspace();
    }

    if (module.id == 'sales') {
      return const _SalesWorkspace();
    }

    if (module.id == 'inventory') {
      return const _InventoryWorkspace();
    }

    if (module.id == 'purchasing') {
      return const _PurchasingWorkspace();
    }

    if (module.id == 'accounting') {
      return const _AccountingWorkspace();
    }

    if (module.id == 'reports') {
      return const _ReportsWorkspace();
    }

    return LayoutBuilder(
      builder: (context, constraints) {
        final width = constraints.maxWidth;
        final padding = AppBreakpoints.pagePadding(width);
        final mobile = AppBreakpoints.isMobile(width);

        return SingleChildScrollView(
          padding: EdgeInsets.all(padding),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _ModuleHeader(module: module, compact: mobile),
              const SizedBox(height: AppSpacing.lg),
              _ModuleToolbar(module: module),
              const SizedBox(height: AppSpacing.lg),
              Flex(
                direction: width < 980 ? Axis.vertical : Axis.horizontal,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Expanded(
                    flex: width < 980 ? 0 : 7,
                    child: _PipelinePanel(module: module),
                  ),
                  SizedBox(
                      width: width < 980 ? 0 : AppSpacing.md,
                      height: width < 980 ? AppSpacing.md : 0),
                  Expanded(
                    flex: width < 980 ? 0 : 4,
                    child: _InsightPanel(module: module),
                  ),
                ],
              ),
              const SizedBox(height: AppSpacing.lg),
              _RecordsPanel(module: module),
            ],
          ),
        );
      },
    );
  }
}

class _CustomersWorkspace extends ConsumerStatefulWidget {
  const _CustomersWorkspace();

  @override
  ConsumerState<_CustomersWorkspace> createState() =>
      _CustomersWorkspaceState();
}

class _CustomersWorkspaceState extends ConsumerState<_CustomersWorkspace> {
  final _search = TextEditingController();
  bool _loading = true;
  String? _error;
  Map<String, dynamic>? _organisation;
  Map<String, dynamic>? _branch;
  List<Map<String, dynamic>> _customers = [];
  StreamSubscription<OperationChanged>? _realtimeSubscription;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  @override
  void dispose() {
    _realtimeSubscription?.cancel();
    _search.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return _LivePageFrame(
      title: 'Customers',
      subtitle: 'Live customer records from PostgreSQL through the API.',
      icon: Icons.people_alt,
      color: AppColors.royalPurple,
      loading: _loading,
      error: _error,
      onRefresh: _load,
      actions: [
        FilledButton.icon(
          onPressed: _loading || _branch == null ? null : _openCreateCustomer,
          icon: const Icon(Icons.add),
          label: const Text('New customer'),
        ),
      ],
      child: Column(
        children: [
          _ContextStrip(organisation: _organisation, branch: _branch),
          const SizedBox(height: AppSpacing.md),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.md),
              child: Row(
                children: [
                  Expanded(
                    child: TextField(
                      controller: _search,
                      decoration: const InputDecoration(
                          prefixIcon: Icon(Icons.search),
                          hintText: 'Search number, name, or phone'),
                      onSubmitted: (_) => _load(),
                    ),
                  ),
                  const SizedBox(width: AppSpacing.md),
                  IconButton.filledTonal(
                      onPressed: _load,
                      icon: const Icon(Icons.search),
                      tooltip: 'Search'),
                ],
              ),
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          _LiveDataTable(
            emptyText: 'No customers yet. Create one to write to the database.',
            columns: const [
              'Number',
              'Name',
              'Phone',
              'Email',
              'Credit limit',
              'Status'
            ],
            rows: [
              for (final customer in _customers)
                [
                  '${customer['customerNumber'] ?? ''}',
                  '${customer['displayName'] ?? ''}',
                  '${customer['phone'] ?? '-'}',
                  '${customer['email'] ?? '-'}',
                  _money(customer['creditLimit']),
                  '${customer['status'] ?? ''}',
                ],
            ],
          ),
        ],
      ),
    );
  }

  Future<void> _load() async {
    final token = ref.read(authControllerProvider).accessToken;
    if (token == null) {
      setState(() {
        _loading = false;
        _error = 'Please sign in again.';
      });
      return;
    }

    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final api = ref.read(apiClientProvider);
      final organisations = await api.listOrganisations(token);
      final organisation = await _selectOperationalOrganisation(
        organisations,
        (organisationId) async => api.listBranches(token, organisationId),
      );
      if (organisation == null) {
        throw StateError(
            'No organisation exists. Restart the API so development seed data can be created.');
      }

      final branches = await api.listBranches(token, '${organisation['id']}');
      final customers = await api.listCustomers(token, '${organisation['id']}',
          search: _search.text);
      await _connectRealtime(token, '${organisation['id']}');

      setState(() {
        _organisation = organisation;
        _branch = branches.isNotEmpty ? branches.first : null;
        _customers = customers;
      });
    } catch (error) {
      setState(() => _error = 'Could not load live customer data: $error');
    } finally {
      if (mounted) {
        setState(() => _loading = false);
      }
    }
  }

  Future<void> _openCreateCustomer() async {
    final created = await showDialog<bool>(
      context: context,
      builder: (context) =>
          _CreateCustomerDialog(organisation: _organisation, branch: _branch),
    );

    if (created == true) {
      await _load();
    }
  }

  Future<void> _connectRealtime(String token, String organisationId) async {
    await ref
        .read(realtimeServiceProvider)
        .connect(accessToken: token, organisationId: organisationId);
    _realtimeSubscription ??=
        ref.read(realtimeServiceProvider).changes.listen((event) {
      if (!mounted || event.organisationId != organisationId) {
        return;
      }

      if (event.module == 'customers' || event.module == 'sales') {
        _load();
      }
    });
  }
}

class _SalesWorkspace extends ConsumerStatefulWidget {
  const _SalesWorkspace();

  @override
  ConsumerState<_SalesWorkspace> createState() => _SalesWorkspaceState();
}

class _SalesWorkspaceState extends ConsumerState<_SalesWorkspace> {
  bool _loading = true;
  String? _error;
  String _range = 'today';
  Map<String, dynamic>? _organisation;
  Map<String, dynamic>? _branch;
  Map<String, dynamic>? _warehouse;
  List<Map<String, dynamic>> _customers = [];
  List<Map<String, dynamic>> _products = [];
  List<Map<String, dynamic>> _sales = [];
  StreamSubscription<OperationChanged>? _realtimeSubscription;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  @override
  void dispose() {
    _realtimeSubscription?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final filteredSales = _filteredSales();
    final total = filteredSales.fold<double>(
        0, (sum, sale) => sum + _asDouble(sale['grandTotal']));

    return _LivePageFrame(
      title: 'Sales',
      subtitle:
          'Live sales table with today, week, month, year, customer, and product-aware rows.',
      icon: Icons.point_of_sale,
      color: AppColors.cobalt,
      loading: _loading,
      error: _error,
      onRefresh: _load,
      actions: [
        FilledButton.icon(
          onPressed: _loading ||
                  _branch == null ||
                  _warehouse == null ||
                  _customers.isEmpty ||
                  _products.isEmpty
              ? null
              : _openCreateSale,
          icon: const Icon(Icons.add),
          label: const Text('New sale'),
        ),
      ],
      child: Column(
        children: [
          _ContextStrip(
              organisation: _organisation,
              branch: _branch,
              warehouse: _warehouse),
          const SizedBox(height: AppSpacing.md),
          LayoutBuilder(
            builder: (context, constraints) {
              final compact = constraints.maxWidth < 760;
              return GridView.count(
                crossAxisCount: compact ? 1 : 4,
                crossAxisSpacing: AppSpacing.md,
                mainAxisSpacing: AppSpacing.md,
                childAspectRatio: compact ? 3.1 : 1.8,
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                children: [
                  _SummaryTile('Filtered sales', _money(total),
                      Icons.payments_outlined, AppColors.cobalt),
                  _SummaryTile('Invoices', '${filteredSales.length}',
                      Icons.receipt_long_outlined, AppColors.royalPurple),
                  _SummaryTile('Customers', '${_customers.length}',
                      Icons.people_alt_outlined, AppColors.success),
                  _SummaryTile('Products', '${_products.length}',
                      Icons.inventory_2_outlined, AppColors.coral),
                ],
              );
            },
          ),
          const SizedBox(height: AppSpacing.md),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.md),
              child: Wrap(
                spacing: AppSpacing.md,
                runSpacing: AppSpacing.md,
                crossAxisAlignment: WrapCrossAlignment.center,
                children: [
                  SegmentedButton<String>(
                    segments: const [
                      ButtonSegment(value: 'today', label: Text('Today')),
                      ButtonSegment(value: 'week', label: Text('Week')),
                      ButtonSegment(value: 'month', label: Text('Month')),
                      ButtonSegment(value: 'year', label: Text('Year')),
                      ButtonSegment(value: 'all', label: Text('All')),
                    ],
                    selected: {_range},
                    onSelectionChanged: (value) =>
                        setState(() => _range = value.first),
                  ),
                  OutlinedButton.icon(
                      onPressed: _load,
                      icon: const Icon(Icons.refresh),
                      label: const Text('Refresh live data')),
                ],
              ),
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          _LiveDataTable(
            emptyText:
                'No sales for this range. Create a sale to post stock and ledger entries.',
            columns: const [
              'Invoice',
              'Date',
              'Customer',
              'Product / ID',
              'Qty',
              'Grand total',
              'Status'
            ],
            rows: [
              for (final sale in filteredSales)
                [
                  '${sale['invoiceNumber'] ?? ''}',
                  _shortDate(sale['saleDateUtc']),
                  _customerName('${sale['customerId']}'),
                  _saleProductLabel(sale),
                  _saleQuantity(sale),
                  _money(sale['grandTotal']),
                  '${sale['status'] ?? ''}',
                ],
            ],
          ),
        ],
      ),
    );
  }

  Future<void> _load() async {
    final token = ref.read(authControllerProvider).accessToken;
    if (token == null) {
      setState(() {
        _loading = false;
        _error = 'Please sign in again.';
      });
      return;
    }

    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final api = ref.read(apiClientProvider);
      final organisations = await api.listOrganisations(token);
      final organisation = await _selectOperationalOrganisation(
        organisations,
        (organisationId) async {
          final branches = await api.listBranches(token, organisationId);
          final warehouses = await api.listWarehouses(token, organisationId);
          return [...branches, ...warehouses];
        },
      );
      if (organisation == null) {
        throw StateError(
            'No organisation exists. Restart the API so development seed data can be created.');
      }

      final organisationId = '${organisation['id']}';
      final branches = await api.listBranches(token, organisationId);
      final warehouses = await api.listWarehouses(token, organisationId);
      final customers = await api.listCustomers(token, organisationId);
      final products = await api.listProducts(token, organisationId);
      final sales = await api.listSales(token, organisationId);
      await _connectRealtime(token, organisationId);

      setState(() {
        _organisation = organisation;
        _branch = branches.isNotEmpty ? branches.first : null;
        _warehouse = warehouses.isNotEmpty ? warehouses.first : null;
        _customers = customers;
        _products = products;
        _sales = sales;
      });
    } catch (error) {
      setState(() => _error = 'Could not load live sales data: $error');
    } finally {
      if (mounted) {
        setState(() => _loading = false);
      }
    }
  }

  List<Map<String, dynamic>> _filteredSales() {
    final now = DateTime.now();
    return _sales.where((sale) {
      final saleDate = DateTime.tryParse('${sale['saleDateUtc']}')?.toLocal();
      if (saleDate == null || _range == 'all') {
        return true;
      }

      return switch (_range) {
        'today' => saleDate.year == now.year &&
            saleDate.month == now.month &&
            saleDate.day == now.day,
        'week' => now.difference(saleDate).inDays < 7,
        'month' => saleDate.year == now.year && saleDate.month == now.month,
        'year' => saleDate.year == now.year,
        _ => true,
      };
    }).toList();
  }

  Future<void> _openCreateSale() async {
    final created = await showDialog<bool>(
      context: context,
      builder: (context) => _CreateSaleDialog(
        organisation: _organisation,
        branch: _branch,
        warehouse: _warehouse,
        customers: _customers,
        products: _products,
      ),
    );

    if (created == true) {
      await _load();
    }
  }

  Future<void> _connectRealtime(String token, String organisationId) async {
    await ref
        .read(realtimeServiceProvider)
        .connect(accessToken: token, organisationId: organisationId);
    _realtimeSubscription ??=
        ref.read(realtimeServiceProvider).changes.listen((event) {
      if (!mounted || event.organisationId != organisationId) {
        return;
      }

      if (event.module == 'sales' ||
          event.module == 'customers' ||
          event.module == 'inventory') {
        _load();
      }
    });
  }

  String _customerName(String customerId) {
    return _customers
            .where((customer) => '${customer['id']}' == customerId)
            .map((customer) => '${customer['displayName']}')
            .firstOrNull ??
        customerId;
  }

  String _saleProductLabel(Map<String, dynamic> sale) {
    final items = sale['items'];
    if (items is! List || items.isEmpty) {
      return '-';
    }

    final firstItem = items.first as Map<String, dynamic>;
    final productId = '${firstItem['productId']}';
    final product =
        _products.where((item) => '${item['id']}' == productId).firstOrNull;
    return '${product?['name'] ?? firstItem['description']} / $productId';
  }

  String _saleQuantity(Map<String, dynamic> sale) {
    final items = sale['items'];
    if (items is! List || items.isEmpty) {
      return '-';
    }

    final firstItem = items.first as Map<String, dynamic>;
    return '${firstItem['quantity']}';
  }
}

class _LivePageFrame extends StatelessWidget {
  const _LivePageFrame({
    required this.title,
    required this.subtitle,
    required this.icon,
    required this.color,
    required this.loading,
    required this.error,
    required this.onRefresh,
    required this.actions,
    required this.child,
  });

  final String title;
  final String subtitle;
  final IconData icon;
  final Color color;
  final bool loading;
  final String? error;
  final Future<void> Function() onRefresh;
  final List<Widget> actions;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final padding = AppBreakpoints.pagePadding(constraints.maxWidth);
        final compact = AppBreakpoints.isMobile(constraints.maxWidth);

        return RefreshIndicator(
          onRefresh: onRefresh,
          child: SingleChildScrollView(
            physics: const AlwaysScrollableScrollPhysics(),
            padding: EdgeInsets.all(padding),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Flex(
                  direction: compact ? Axis.vertical : Axis.horizontal,
                  crossAxisAlignment: compact
                      ? CrossAxisAlignment.start
                      : CrossAxisAlignment.center,
                  children: [
                    Container(
                      width: 54,
                      height: 54,
                      decoration: BoxDecoration(
                        color: color.withValues(alpha: 0.12),
                        borderRadius: BorderRadius.circular(AppRadius.md),
                      ),
                      child: Icon(icon, color: color, size: 28),
                    ),
                    const SizedBox(width: AppSpacing.md, height: AppSpacing.md),
                    Expanded(
                      flex: compact ? 0 : 1,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(title,
                              style: Theme.of(context).textTheme.headlineLarge),
                          const SizedBox(height: AppSpacing.xs),
                          Text(subtitle,
                              style: Theme.of(context).textTheme.bodyMedium),
                        ],
                      ),
                    ),
                    Wrap(
                        spacing: AppSpacing.sm,
                        runSpacing: AppSpacing.sm,
                        children: actions),
                  ],
                ),
                if (loading) ...[
                  const SizedBox(height: AppSpacing.lg),
                  const LinearProgressIndicator(),
                ],
                if (error != null) ...[
                  const SizedBox(height: AppSpacing.lg),
                  _InlinePanel(
                      message: error!,
                      color: AppColors.danger,
                      icon: Icons.error_outline),
                ],
                const SizedBox(height: AppSpacing.lg),
                child,
              ],
            ),
          ),
        );
      },
    );
  }
}

class _ContextStrip extends StatelessWidget {
  const _ContextStrip({this.organisation, this.branch, this.warehouse});

  final Map<String, dynamic>? organisation;
  final Map<String, dynamic>? branch;
  final Map<String, dynamic>? warehouse;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Wrap(
          spacing: AppSpacing.md,
          runSpacing: AppSpacing.sm,
          children: [
            _ContextChip(
                icon: Icons.business,
                label: '${organisation?['displayName'] ?? 'No organisation'}'),
            _ContextChip(
                icon: Icons.store_outlined,
                label: '${branch?['name'] ?? 'No branch'}'),
            if (warehouse != null)
              _ContextChip(
                  icon: Icons.warehouse_outlined,
                  label: '${warehouse?['name']}'),
          ],
        ),
      ),
    );
  }
}

class _ContextChip extends StatelessWidget {
  const _ContextChip({required this.icon, required this.label});

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Chip(
      avatar: Icon(icon, size: 18),
      label: Text(label),
    );
  }
}

class _SummaryTile extends StatelessWidget {
  const _SummaryTile(this.label, this.value, this.icon, this.color);

  final String label;
  final String value;
  final IconData icon;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Row(
          children: [
            CircleAvatar(
              backgroundColor: color.withValues(alpha: 0.12),
              child: Icon(icon, color: color),
            ),
            const SizedBox(width: AppSpacing.md),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(value, style: Theme.of(context).textTheme.titleLarge),
                  Text(label, style: Theme.of(context).textTheme.bodyMedium),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _LiveDataTable extends StatelessWidget {
  const _LiveDataTable({
    required this.columns,
    required this.rows,
    required this.emptyText,
    this.rowActions,
  });

  final List<String> columns;
  final List<List<String>> rows;
  final String emptyText;
  final List<Widget>? rowActions;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: rows.isEmpty
            ? _InlinePanel(
                message: emptyText,
                color: AppColors.cobalt,
                icon: Icons.info_outline)
            : SingleChildScrollView(
                scrollDirection: Axis.horizontal,
                child: DataTable(
                  columns: [
                    for (final column in columns)
                      DataColumn(label: Text(column))
                  ],
                  rows: [
                    for (var index = 0; index < rows.length; index++)
                      DataRow(cells: [
                        for (final cell in rows[index]) DataCell(Text(cell)),
                        if (rowActions != null) DataCell(rowActions![index]),
                      ]),
                  ],
                ),
              ),
      ),
    );
  }
}

class _InlinePanel extends StatelessWidget {
  const _InlinePanel(
      {required this.message, required this.color, required this.icon});

  final String message;
  final Color color;
  final IconData icon;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(AppRadius.md),
        border: Border.all(color: color.withValues(alpha: 0.22)),
      ),
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Row(
          children: [
            Icon(icon, color: color),
            const SizedBox(width: AppSpacing.sm),
            Expanded(child: Text(message)),
          ],
        ),
      ),
    );
  }
}

class _CreateCustomerDialog extends ConsumerStatefulWidget {
  const _CreateCustomerDialog(
      {required this.organisation, required this.branch});

  final Map<String, dynamic>? organisation;
  final Map<String, dynamic>? branch;

  @override
  ConsumerState<_CreateCustomerDialog> createState() =>
      _CreateCustomerDialogState();
}

class _CreateCustomerDialogState extends ConsumerState<_CreateCustomerDialog> {
  final _name = TextEditingController();
  final _phone = TextEditingController();
  final _email = TextEditingController();
  final _creditLimit = TextEditingController(text: '0');
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _name.dispose();
    _phone.dispose();
    _email.dispose();
    _creditLimit.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('New customer'),
      content: SizedBox(
        width: 460,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
                controller: _name,
                decoration: const InputDecoration(labelText: 'Display name')),
            const SizedBox(height: AppSpacing.md),
            TextField(
                controller: _phone,
                decoration: const InputDecoration(labelText: 'Phone')),
            const SizedBox(height: AppSpacing.md),
            TextField(
                controller: _email,
                decoration: const InputDecoration(labelText: 'Email')),
            const SizedBox(height: AppSpacing.md),
            TextField(
                controller: _creditLimit,
                keyboardType: TextInputType.number,
                decoration: const InputDecoration(labelText: 'Credit limit')),
            if (_error != null) ...[
              const SizedBox(height: AppSpacing.md),
              Text(_error!, style: const TextStyle(color: AppColors.danger)),
            ],
          ],
        ),
      ),
      actions: [
        TextButton(
            onPressed: _busy ? null : () => Navigator.of(context).pop(false),
            child: const Text('Cancel')),
        FilledButton.icon(
            onPressed: _busy ? null : _create,
            icon: const Icon(Icons.save_outlined),
            label: const Text('Create')),
      ],
    );
  }

  Future<void> _create() async {
    final token = ref.read(authControllerProvider).accessToken;
    final organisationId = '${widget.organisation?['id'] ?? ''}';
    final branchId = '${widget.branch?['id'] ?? ''}';
    if (token == null ||
        organisationId.isEmpty ||
        branchId.isEmpty ||
        _name.text.trim().isEmpty) {
      setState(() =>
          _error = 'Organisation, branch, and customer name are required.');
      return;
    }

    setState(() {
      _busy = true;
      _error = null;
    });

    try {
      await ref.read(apiClientProvider).createCustomer(
            token,
            organisationId: organisationId,
            branchId: branchId,
            customerNumber: 'C-${DateTime.now().millisecondsSinceEpoch}',
            displayName: _name.text.trim(),
            phone: _phone.text.trim(),
            email: _email.text.trim(),
            creditLimit: double.tryParse(_creditLimit.text) ?? 0,
          );
      if (mounted) {
        Navigator.of(context).pop(true);
      }
    } catch (error) {
      setState(() => _error = 'Customer was not created: $error');
    } finally {
      if (mounted) {
        setState(() => _busy = false);
      }
    }
  }
}

class _CreateSaleDialog extends ConsumerStatefulWidget {
  const _CreateSaleDialog({
    required this.organisation,
    required this.branch,
    required this.warehouse,
    required this.customers,
    required this.products,
  });

  final Map<String, dynamic>? organisation;
  final Map<String, dynamic>? branch;
  final Map<String, dynamic>? warehouse;
  final List<Map<String, dynamic>> customers;
  final List<Map<String, dynamic>> products;

  @override
  ConsumerState<_CreateSaleDialog> createState() => _CreateSaleDialogState();
}

class _CreateSaleDialogState extends ConsumerState<_CreateSaleDialog> {
  final _quantity = TextEditingController(text: '1');
  String? _customerId;
  String? _productId;
  bool _busy = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _customerId =
        widget.customers.isNotEmpty ? '${widget.customers.first['id']}' : null;
    _productId =
        widget.products.isNotEmpty ? '${widget.products.first['id']}' : null;
  }

  @override
  void dispose() {
    _quantity.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final selectedProduct = widget.products
        .where((item) => '${item['id']}' == _productId)
        .firstOrNull;
    return AlertDialog(
      title: const Text('New sale'),
      content: SizedBox(
        width: 520,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            DropdownButtonFormField<String>(
              initialValue: _customerId,
              decoration: const InputDecoration(labelText: 'Customer'),
              items: [
                for (final customer in widget.customers)
                  DropdownMenuItem(
                      value: '${customer['id']}',
                      child: Text('${customer['displayName']}')),
              ],
              onChanged: (value) => setState(() => _customerId = value),
            ),
            const SizedBox(height: AppSpacing.md),
            DropdownButtonFormField<String>(
              initialValue: _productId,
              decoration: const InputDecoration(labelText: 'Product'),
              items: [
                for (final product in widget.products)
                  DropdownMenuItem(
                      value: '${product['id']}',
                      child: Text(
                          '${product['productCode']} - ${product['name']}')),
              ],
              onChanged: (value) => setState(() => _productId = value),
            ),
            const SizedBox(height: AppSpacing.md),
            TextField(
                controller: _quantity,
                keyboardType: TextInputType.number,
                decoration: const InputDecoration(labelText: 'Quantity')),
            const SizedBox(height: AppSpacing.md),
            _InlinePanel(
              message:
                  'Unit price: ${_money(selectedProduct?['salesPrice'])}. Sale posts stock movement and customer ledger entry.',
              color: AppColors.success,
              icon: Icons.verified_outlined,
            ),
            if (_error != null) ...[
              const SizedBox(height: AppSpacing.md),
              Text(_error!, style: const TextStyle(color: AppColors.danger)),
            ],
          ],
        ),
      ),
      actions: [
        TextButton(
            onPressed: _busy ? null : () => Navigator.of(context).pop(false),
            child: const Text('Cancel')),
        FilledButton.icon(
            onPressed: _busy ? null : _create,
            icon: const Icon(Icons.point_of_sale),
            label: const Text('Post sale')),
      ],
    );
  }

  Future<void> _create() async {
    final token = ref.read(authControllerProvider).accessToken;
    final organisationId = '${widget.organisation?['id'] ?? ''}';
    final branchId = '${widget.branch?['id'] ?? ''}';
    final warehouseId = '${widget.warehouse?['id'] ?? ''}';
    final selectedProduct = widget.products
        .where((item) => '${item['id']}' == _productId)
        .firstOrNull;
    if (token == null ||
        organisationId.isEmpty ||
        branchId.isEmpty ||
        warehouseId.isEmpty ||
        _customerId == null ||
        selectedProduct == null) {
      setState(() => _error =
          'Organisation, branch, warehouse, customer, and product are required.');
      return;
    }

    setState(() {
      _busy = true;
      _error = null;
    });

    try {
      await ref.read(apiClientProvider).createSale(
            token,
            organisationId: organisationId,
            branchId: branchId,
            warehouseId: warehouseId,
            customerId: _customerId!,
            productId: _productId!,
            description: '${selectedProduct['name']}',
            quantity: double.tryParse(_quantity.text) ?? 1,
            unitPrice: _asDouble(selectedProduct['salesPrice']),
          );
      if (mounted) {
        Navigator.of(context).pop(true);
      }
    } catch (error) {
      setState(() => _error = 'Sale was not posted: $error');
    } finally {
      if (mounted) {
        setState(() => _busy = false);
      }
    }
  }
}

class _ModuleHeader extends StatelessWidget {
  const _ModuleHeader({required this.module, required this.compact});

  final ModuleDefinition module;
  final bool compact;

  @override
  Widget build(BuildContext context) {
    return Flex(
      direction: compact ? Axis.vertical : Axis.horizontal,
      crossAxisAlignment:
          compact ? CrossAxisAlignment.start : CrossAxisAlignment.center,
      children: [
        Container(
          width: 54,
          height: 54,
          decoration: BoxDecoration(
            color: module.color.withValues(alpha: 0.12),
            borderRadius: BorderRadius.circular(AppRadius.md),
          ),
          child: Icon(module.icon, color: module.color, size: 28),
        ),
        const SizedBox(width: AppSpacing.md, height: AppSpacing.md),
        Expanded(
          flex: compact ? 0 : 1,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(module.title,
                  style: Theme.of(context).textTheme.headlineLarge),
              const SizedBox(height: AppSpacing.xs),
              Text(module.description,
                  style: Theme.of(context).textTheme.bodyMedium),
            ],
          ),
        ),
        SizedBox(height: compact ? AppSpacing.md : 0),
        FilledButton.icon(
            onPressed: () {},
            icon: const Icon(Icons.add),
            label: Text(module.primaryAction)),
      ],
    );
  }
}

class _ModuleToolbar extends StatefulWidget {
  const _ModuleToolbar({required this.module});

  final ModuleDefinition module;

  @override
  State<_ModuleToolbar> createState() => _ModuleToolbarState();
}

class _ModuleToolbarState extends State<_ModuleToolbar> {
  String _view = 'Active';
  String _branch = 'All branches';

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Wrap(
          spacing: AppSpacing.md,
          runSpacing: AppSpacing.md,
          crossAxisAlignment: WrapCrossAlignment.center,
          children: [
            SizedBox(
              width: 260,
              child: TextField(
                decoration: InputDecoration(
                  hintText: 'Search ${widget.module.title.toLowerCase()}',
                  prefixIcon: const Icon(Icons.search),
                ),
              ),
            ),
            SizedBox(
              width: 180,
              child: DropdownButtonFormField<String>(
                initialValue: _branch,
                decoration: const InputDecoration(labelText: 'Branch'),
                items: const [
                  DropdownMenuItem(
                      value: 'All branches', child: Text('All branches')),
                  DropdownMenuItem(value: 'Main', child: Text('Main')),
                  DropdownMenuItem(value: 'North', child: Text('North')),
                ],
                onChanged: (value) =>
                    setState(() => _branch = value ?? _branch),
              ),
            ),
            SegmentedButton<String>(
              segments: const [
                ButtonSegment(value: 'Active', label: Text('Active')),
                ButtonSegment(value: 'Draft', label: Text('Draft')),
                ButtonSegment(value: 'Archived', label: Text('Archived')),
              ],
              selected: {_view},
              onSelectionChanged: (value) =>
                  setState(() => _view = value.first),
            ),
            OutlinedButton.icon(
                onPressed: () {},
                icon: const Icon(Icons.tune),
                label: const Text('Filters')),
          ],
        ),
      ),
    );
  }
}

class _PipelinePanel extends StatelessWidget {
  const _PipelinePanel({required this.module});

  final ModuleDefinition module;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('${module.title} workflow',
                style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: AppSpacing.md),
            for (var i = 0; i < module.stages.length; i++) ...[
              _StageTile(
                index: i + 1,
                title: module.stages[i],
                count: module.stageCounts[i],
                color: module.color,
              ),
              if (i != module.stages.length - 1)
                const SizedBox(height: AppSpacing.sm),
            ],
          ],
        ),
      ),
    );
  }
}

class _StageTile extends StatelessWidget {
  const _StageTile(
      {required this.index,
      required this.title,
      required this.count,
      required this.color});

  final int index;
  final String title;
  final int count;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: Duration(milliseconds: 240 + index * 60),
      curve: Curves.easeOut,
      builder: (context, value, child) => Transform.translate(
        offset: Offset(16 * (1 - value), 0),
        child: Opacity(opacity: value, child: child),
      ),
      child: Container(
        padding: const EdgeInsets.all(AppSpacing.md),
        decoration: BoxDecoration(
          color: AppColors.canvas,
          borderRadius: BorderRadius.circular(AppRadius.md),
          border: Border.all(color: AppColors.line),
        ),
        child: Row(
          children: [
            CircleAvatar(
              radius: 15,
              backgroundColor: color,
              child: Text('$index',
                  style: const TextStyle(
                      color: Colors.white, fontWeight: FontWeight.w700)),
            ),
            const SizedBox(width: AppSpacing.md),
            Expanded(
                child: Text(title,
                    style: Theme.of(context).textTheme.titleMedium)),
            Chip(label: Text('$count')),
          ],
        ),
      ),
    );
  }
}

class _InsightPanel extends StatelessWidget {
  const _InsightPanel({required this.module});

  final ModuleDefinition module;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Insights', style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: AppSpacing.md),
            _InsightRow(
                label: 'Cycle health', value: module.health, icon: Icons.speed),
            _InsightRow(
                label: 'Exceptions',
                value: module.exceptions,
                icon: Icons.warning_amber),
            _InsightRow(
                label: 'Audit coverage',
                value: 'Enabled',
                icon: Icons.verified_user_outlined),
            const SizedBox(height: AppSpacing.md),
            LinearProgressIndicator(
              value: module.progress,
              minHeight: 10,
              borderRadius: BorderRadius.circular(AppRadius.md),
              backgroundColor: AppColors.line,
              color: module.color,
            ),
          ],
        ),
      ),
    );
  }
}

class _InsightRow extends StatelessWidget {
  const _InsightRow(
      {required this.label, required this.value, required this.icon});

  final String label;
  final String value;
  final IconData icon;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.md),
      child: Row(
        children: [
          Icon(icon, color: AppColors.royalPurple),
          const SizedBox(width: AppSpacing.sm),
          Expanded(child: Text(label)),
          Text(value, style: Theme.of(context).textTheme.titleMedium),
        ],
      ),
    );
  }
}

class _RecordsPanel extends StatelessWidget {
  const _RecordsPanel({required this.module});

  final ModuleDefinition module;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Recent records',
                style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: AppSpacing.md),
            SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: DataTable(
                columns: const [
                  DataColumn(label: Text('Reference')),
                  DataColumn(label: Text('Name')),
                  DataColumn(label: Text('Owner')),
                  DataColumn(label: Text('Status')),
                  DataColumn(label: Text('Value')),
                ],
                rows: [
                  for (final record in module.records)
                    DataRow(cells: [
                      DataCell(Text(record.reference)),
                      DataCell(Text(record.name)),
                      DataCell(Text(record.owner)),
                      DataCell(Chip(label: Text(record.status))),
                      DataCell(Text(record.value)),
                    ]),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _SettingsWorkspace extends ConsumerStatefulWidget {
  const _SettingsWorkspace();

  @override
  ConsumerState<_SettingsWorkspace> createState() => _SettingsWorkspaceState();
}

class _ReportsWorkspace extends ConsumerStatefulWidget {
  const _ReportsWorkspace();

  @override
  ConsumerState<_ReportsWorkspace> createState() => _ReportsWorkspaceState();
}

class _ReportsWorkspaceState extends ConsumerState<_ReportsWorkspace> {
  bool _loading = true;
  String? _error;
  String _range = 'month';
  String? _customerId;
  String? _productId;
  Map<String, dynamic>? _organisation;
  Map<String, dynamic> _report = {};
  List<Map<String, dynamic>> _customers = [];
  List<Map<String, dynamic>> _products = [];
  StreamSubscription<OperationChanged>? _realtimeSubscription;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  @override
  void dispose() {
    _realtimeSubscription?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final productRows = (_report['products'] as List? ?? [])
        .whereType<Map<String, dynamic>>()
        .toList();
    final invoiceRows = (_report['invoices'] as List? ?? [])
        .whereType<Map<String, dynamic>>()
        .toList();

    return _LivePageFrame(
      title: 'Reports',
      subtitle: 'Sales reports by date, customer, product, and product ID.',
      icon: Icons.bar_chart,
      color: AppColors.mint,
      loading: _loading,
      error: _error,
      onRefresh: _load,
      actions: [
        OutlinedButton.icon(
          onPressed: _loading ? null : _load,
          icon: const Icon(Icons.refresh),
          label: const Text('Run report'),
        ),
      ],
      child: Column(
        children: [
          _ContextStrip(organisation: _organisation),
          const SizedBox(height: AppSpacing.md),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.md),
              child: Wrap(
                spacing: AppSpacing.md,
                runSpacing: AppSpacing.md,
                crossAxisAlignment: WrapCrossAlignment.center,
                children: [
                  SegmentedButton<String>(
                    segments: const [
                      ButtonSegment(value: 'today', label: Text('Today')),
                      ButtonSegment(value: 'week', label: Text('Week')),
                      ButtonSegment(value: 'month', label: Text('Month')),
                      ButtonSegment(value: 'year', label: Text('Year')),
                      ButtonSegment(value: 'all', label: Text('All')),
                    ],
                    selected: {_range},
                    onSelectionChanged: (value) {
                      setState(() => _range = value.first);
                      _load();
                    },
                  ),
                  SizedBox(
                    width: 260,
                    child: DropdownButtonFormField<String>(
                      initialValue: _customerId,
                      decoration: const InputDecoration(labelText: 'Customer'),
                      items: [
                        const DropdownMenuItem(
                            value: '', child: Text('All customers')),
                        for (final customer in _customers)
                          DropdownMenuItem(
                            value: '${customer['id']}',
                            child: Text('${customer['displayName']}'),
                          ),
                      ],
                      onChanged: (value) {
                        setState(() => _customerId =
                            value?.isEmpty == true ? null : value);
                        _load();
                      },
                    ),
                  ),
                  SizedBox(
                    width: 280,
                    child: DropdownButtonFormField<String>(
                      initialValue: _productId,
                      decoration:
                          const InputDecoration(labelText: 'Product / ID'),
                      items: [
                        const DropdownMenuItem(
                            value: '', child: Text('All products')),
                        for (final product in _products)
                          DropdownMenuItem(
                            value: '${product['id']}',
                            child: Text(
                                '${product['productCode']} - ${product['name']}'),
                          ),
                      ],
                      onChanged: (value) {
                        setState(() =>
                            _productId = value?.isEmpty == true ? null : value);
                        _load();
                      },
                    ),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          GridView.count(
            crossAxisCount: MediaQuery.sizeOf(context).width < 900 ? 1 : 4,
            crossAxisSpacing: AppSpacing.md,
            mainAxisSpacing: AppSpacing.md,
            childAspectRatio: 2.1,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            children: [
              _SummaryTile('Sales total', _money(_report['grandTotal']),
                  Icons.payments_outlined, AppColors.mint),
              _SummaryTile('Invoices', '${_report['invoiceCount'] ?? 0}',
                  Icons.receipt_long_outlined, AppColors.cobalt),
              _SummaryTile(
                  'Quantity',
                  _asDouble(_report['quantity']).toStringAsFixed(2),
                  Icons.inventory_2_outlined,
                  AppColors.success),
              _SummaryTile('Tax', _money(_report['taxTotal']), Icons.percent,
                  AppColors.coral),
            ],
          ),
          const SizedBox(height: AppSpacing.md),
          _LiveDataTable(
            emptyText: 'No product sales for this filter.',
            columns: const ['Product ID', 'Code', 'Product', 'Qty', 'Sales'],
            rows: [
              for (final row in productRows)
                [
                  '${row['productId']}',
                  '${row['productCode'] ?? ''}',
                  '${row['productName'] ?? ''}',
                  _asDouble(row['quantity']).toStringAsFixed(2),
                  _money(row['salesTotal']),
                ],
            ],
          ),
          const SizedBox(height: AppSpacing.md),
          _LiveDataTable(
            emptyText: 'No invoices for this filter.',
            columns: const [
              'Invoice',
              'Date',
              'Customer',
              'Customer ID',
              'Total'
            ],
            rows: [
              for (final row in invoiceRows)
                [
                  '${row['invoiceNumber'] ?? ''}',
                  _shortDate(row['saleDateUtc']),
                  '${row['customerName'] ?? ''}',
                  '${row['customerId'] ?? ''}',
                  _money(row['grandTotal']),
                ],
            ],
          ),
        ],
      ),
    );
  }

  Future<void> _load() async {
    final token = ref.read(authControllerProvider).accessToken;
    if (token == null) {
      setState(() {
        _loading = false;
        _error = 'Please sign in again.';
      });
      return;
    }

    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final api = ref.read(apiClientProvider);
      final organisations = await api.listOrganisations(token);
      final organisation = await _selectOperationalOrganisation(
        organisations,
        (organisationId) => api.listCustomers(token, organisationId),
      );
      if (organisation == null) {
        throw StateError('No organisation exists.');
      }

      final organisationId = '${organisation['id']}';
      final customers = await api.listCustomers(token, organisationId);
      final products = await api.listProducts(token, organisationId);
      final range = _reportRange();
      final report = await api.getSalesReport(
        token,
        organisationId: organisationId,
        fromUtc: range.$1,
        toUtc: range.$2,
        customerId: _customerId,
        productId: _productId,
      );
      await _connectRealtime(token, organisationId);

      setState(() {
        _organisation = organisation;
        _customers = customers;
        _products = products;
        _report = report;
      });
    } catch (error) {
      setState(() => _error = 'Could not load report: $error');
    } finally {
      if (mounted) {
        setState(() => _loading = false);
      }
    }
  }

  (DateTime?, DateTime?) _reportRange() {
    final now = DateTime.now();
    final end = DateTime(now.year, now.month, now.day, 23, 59, 59);
    return switch (_range) {
      'today' => (DateTime(now.year, now.month, now.day), end),
      'week' => (now.subtract(const Duration(days: 7)), end),
      'month' => (DateTime(now.year, now.month), end),
      'year' => (DateTime(now.year), end),
      _ => (null, null),
    };
  }

  Future<void> _connectRealtime(String token, String organisationId) async {
    await ref
        .read(realtimeServiceProvider)
        .connect(accessToken: token, organisationId: organisationId);
    _realtimeSubscription ??=
        ref.read(realtimeServiceProvider).changes.listen((event) {
      if (mounted &&
          event.organisationId == organisationId &&
          event.module == 'sales') {
        _load();
      }
    });
  }
}

class _InventoryWorkspace extends ConsumerStatefulWidget {
  const _InventoryWorkspace();

  @override
  ConsumerState<_InventoryWorkspace> createState() =>
      _InventoryWorkspaceState();
}

class _InventoryWorkspaceState extends ConsumerState<_InventoryWorkspace> {
  bool _loading = true;
  String? _error;
  Map<String, dynamic>? _organisation;
  Map<String, dynamic>? _warehouse;
  List<Map<String, dynamic>> _products = [];
  List<Map<String, dynamic>> _balances = [];
  List<Map<String, dynamic>> _movements = [];
  StreamSubscription<OperationChanged>? _realtimeSubscription;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  @override
  void dispose() {
    _realtimeSubscription?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final stockTotal = _balances.fold<double>(
        0, (sum, item) => sum + _asDouble(item['quantityOnHand']));

    return _LivePageFrame(
      title: 'Inventory',
      subtitle: 'Live stock balances, movements, and stock adjustments.',
      icon: Icons.inventory_2,
      color: AppColors.success,
      loading: _loading,
      error: _error,
      onRefresh: _load,
      actions: [
        FilledButton.icon(
          onPressed: _loading || _warehouse == null || _products.isEmpty
              ? null
              : _openAdjustment,
          icon: const Icon(Icons.tune),
          label: const Text('Adjust stock'),
        ),
      ],
      child: Column(
        children: [
          _ContextStrip(organisation: _organisation, warehouse: _warehouse),
          const SizedBox(height: AppSpacing.md),
          GridView.count(
            crossAxisCount: MediaQuery.sizeOf(context).width < 900 ? 1 : 3,
            crossAxisSpacing: AppSpacing.md,
            mainAxisSpacing: AppSpacing.md,
            childAspectRatio: 2.2,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            children: [
              _SummaryTile('Stock lines', '${_balances.length}',
                  Icons.list_alt_outlined, AppColors.success),
              _SummaryTile('Quantity on hand', stockTotal.toStringAsFixed(0),
                  Icons.inventory_outlined, AppColors.cobalt),
              _SummaryTile('Movements', '${_movements.length}', Icons.swap_vert,
                  AppColors.coral),
            ],
          ),
          const SizedBox(height: AppSpacing.md),
          _LiveDataTable(
            emptyText: 'No stock balances found.',
            columns: const ['Product', 'Product ID', 'Quantity', 'Updated'],
            rows: [
              for (final balance in _balances)
                [
                  _nameFor(_products, '${balance['productId']}', 'name'),
                  '${balance['productId']}',
                  _asDouble(balance['quantityOnHand']).toStringAsFixed(2),
                  _shortDate(balance['updatedAtUtc']),
                ],
            ],
          ),
          const SizedBox(height: AppSpacing.md),
          _LiveDataTable(
            emptyText: 'No stock movements found.',
            columns: const ['Date', 'Product', 'Type', 'Qty', 'Reference'],
            rows: [
              for (final movement in _movements)
                [
                  _shortDate(movement['occurredAtUtc']),
                  _nameFor(_products, '${movement['productId']}', 'name'),
                  '${movement['movementType']}',
                  _asDouble(movement['signedQuantity']).toStringAsFixed(2),
                  '${movement['referenceType'] ?? ''}',
                ],
            ],
          ),
        ],
      ),
    );
  }

  Future<void> _load() async {
    final token = ref.read(authControllerProvider).accessToken;
    if (token == null) {
      setState(() {
        _loading = false;
        _error = 'Please sign in again.';
      });
      return;
    }

    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final api = ref.read(apiClientProvider);
      final organisations = await api.listOrganisations(token);
      final organisation = await _selectOperationalOrganisation(
        organisations,
        (organisationId) => api.listWarehouses(token, organisationId),
      );
      if (organisation == null) {
        throw StateError('No organisation exists.');
      }

      final organisationId = '${organisation['id']}';
      final warehouses = await api.listWarehouses(token, organisationId);
      final warehouse = warehouses.isNotEmpty ? warehouses.first : null;
      final products = await api.listProducts(token, organisationId);
      final balances = await api.listInventoryBalances(
        token,
        organisationId,
        warehouseId: warehouse == null ? null : '${warehouse['id']}',
      );
      final movements = await api.listInventoryMovements(
        token,
        organisationId,
        warehouseId: warehouse == null ? null : '${warehouse['id']}',
      );
      await _connectRealtime(token, organisationId);

      setState(() {
        _organisation = organisation;
        _warehouse = warehouse;
        _products = products;
        _balances = balances;
        _movements = movements;
      });
    } catch (error) {
      setState(() => _error = 'Could not load inventory: $error');
    } finally {
      if (mounted) {
        setState(() => _loading = false);
      }
    }
  }

  Future<void> _connectRealtime(String token, String organisationId) async {
    await ref
        .read(realtimeServiceProvider)
        .connect(accessToken: token, organisationId: organisationId);
    _realtimeSubscription ??=
        ref.read(realtimeServiceProvider).changes.listen((event) {
      if (mounted &&
          event.organisationId == organisationId &&
          event.module == 'inventory') {
        _load();
      }
    });
  }

  Future<void> _openAdjustment() async {
    final created = await showDialog<bool>(
      context: context,
      builder: (context) => _StockAdjustmentDialog(
        organisation: _organisation,
        warehouse: _warehouse,
        products: _products,
      ),
    );
    if (created == true) {
      await _load();
    }
  }
}

class _PurchasingWorkspace extends ConsumerStatefulWidget {
  const _PurchasingWorkspace();

  @override
  ConsumerState<_PurchasingWorkspace> createState() =>
      _PurchasingWorkspaceState();
}

class _PurchasingWorkspaceState extends ConsumerState<_PurchasingWorkspace> {
  bool _loading = true;
  String? _error;
  Map<String, dynamic>? _organisation;
  Map<String, dynamic>? _branch;
  List<Map<String, dynamic>> _suppliers = [];
  List<Map<String, dynamic>> _products = [];
  List<Map<String, dynamic>> _orders = [];
  StreamSubscription<OperationChanged>? _realtimeSubscription;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  @override
  void dispose() {
    _realtimeSubscription?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final total = _orders.fold<double>(
        0, (sum, order) => sum + _asDouble(order['grandTotal']));

    return _LivePageFrame(
      title: 'Purchasing',
      subtitle: 'Live suppliers and purchase orders.',
      icon: Icons.shopping_cart,
      color: AppColors.coral,
      loading: _loading,
      error: _error,
      onRefresh: _load,
      actions: [
        OutlinedButton.icon(
          onPressed: _loading || _organisation == null ? null : _openSupplier,
          icon: const Icon(Icons.business_outlined),
          label: const Text('New supplier'),
        ),
        FilledButton.icon(
          onPressed: _loading ||
                  _branch == null ||
                  _suppliers.isEmpty ||
                  _products.isEmpty
              ? null
              : _openPurchaseOrder,
          icon: const Icon(Icons.add_shopping_cart),
          label: const Text('New PO'),
        ),
      ],
      child: Column(
        children: [
          _ContextStrip(organisation: _organisation, branch: _branch),
          const SizedBox(height: AppSpacing.md),
          GridView.count(
            crossAxisCount: MediaQuery.sizeOf(context).width < 900 ? 1 : 3,
            crossAxisSpacing: AppSpacing.md,
            mainAxisSpacing: AppSpacing.md,
            childAspectRatio: 2.2,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            children: [
              _SummaryTile('Purchase orders', '${_orders.length}',
                  Icons.receipt_long_outlined, AppColors.coral),
              _SummaryTile('Suppliers', '${_suppliers.length}',
                  Icons.business_outlined, AppColors.royalPurple),
              _SummaryTile('Ordered value', _money(total),
                  Icons.payments_outlined, AppColors.cobalt),
            ],
          ),
          const SizedBox(height: AppSpacing.md),
          _LiveDataTable(
            emptyText: 'No purchase orders yet.',
            columns: const ['Order', 'Date', 'Supplier', 'Total', 'Status'],
            rows: [
              for (final order in _orders)
                [
                  '${order['orderNumber'] ?? ''}',
                  _shortDate(order['orderDateUtc']),
                  _nameFor(_suppliers, '${order['supplierId']}', 'displayName'),
                  _money(order['grandTotal']),
                  '${order['status'] ?? ''}',
                ],
            ],
          ),
        ],
      ),
    );
  }

  Future<void> _load() async {
    final token = ref.read(authControllerProvider).accessToken;
    if (token == null) {
      setState(() {
        _loading = false;
        _error = 'Please sign in again.';
      });
      return;
    }

    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final api = ref.read(apiClientProvider);
      final organisations = await api.listOrganisations(token);
      final organisation = await _selectOperationalOrganisation(
        organisations,
        (organisationId) => api.listBranches(token, organisationId),
      );
      if (organisation == null) {
        throw StateError('No organisation exists.');
      }

      final organisationId = '${organisation['id']}';
      final branches = await api.listBranches(token, organisationId);
      final suppliers = await api.listSuppliers(token, organisationId);
      final products = await api.listProducts(token, organisationId);
      final orders = await api.listPurchaseOrders(token, organisationId);
      await _connectRealtime(token, organisationId);

      setState(() {
        _organisation = organisation;
        _branch = branches.isNotEmpty ? branches.first : null;
        _suppliers = suppliers;
        _products = products;
        _orders = orders;
      });
    } catch (error) {
      setState(() => _error = 'Could not load purchasing: $error');
    } finally {
      if (mounted) {
        setState(() => _loading = false);
      }
    }
  }

  Future<void> _connectRealtime(String token, String organisationId) async {
    await ref
        .read(realtimeServiceProvider)
        .connect(accessToken: token, organisationId: organisationId);
    _realtimeSubscription ??=
        ref.read(realtimeServiceProvider).changes.listen((event) {
      if (mounted &&
          event.organisationId == organisationId &&
          event.module == 'purchasing') {
        _load();
      }
    });
  }

  Future<void> _openSupplier() async {
    final created = await showDialog<bool>(
      context: context,
      builder: (context) => _CreateSupplierDialog(organisation: _organisation),
    );
    if (created == true) {
      await _load();
    }
  }

  Future<void> _openPurchaseOrder() async {
    final created = await showDialog<bool>(
      context: context,
      builder: (context) => _CreatePurchaseOrderDialog(
        organisation: _organisation,
        branch: _branch,
        suppliers: _suppliers,
        products: _products,
      ),
    );
    if (created == true) {
      await _load();
    }
  }
}

class _AccountingWorkspace extends ConsumerStatefulWidget {
  const _AccountingWorkspace();

  @override
  ConsumerState<_AccountingWorkspace> createState() =>
      _AccountingWorkspaceState();
}

class _AccountingWorkspaceState extends ConsumerState<_AccountingWorkspace> {
  bool _loading = true;
  String? _error;
  Map<String, dynamic>? _organisation;
  List<Map<String, dynamic>> _accounts = [];
  List<Map<String, dynamic>> _periods = [];
  StreamSubscription<OperationChanged>? _realtimeSubscription;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  @override
  void dispose() {
    _realtimeSubscription?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return _LivePageFrame(
      title: 'Accounting',
      subtitle: 'Live chart of accounts, fiscal periods, and journals.',
      icon: Icons.account_balance,
      color: AppColors.metallicGold,
      loading: _loading,
      error: _error,
      onRefresh: _load,
      actions: [
        OutlinedButton.icon(
          onPressed: _loading || _organisation == null ? null : _openAccount,
          icon: const Icon(Icons.account_tree_outlined),
          label: const Text('New account'),
        ),
        FilledButton.icon(
          onPressed: _loading || _organisation == null || _accounts.length < 2
              ? null
              : _openJournal,
          icon: const Icon(Icons.post_add),
          label: const Text('New journal'),
        ),
      ],
      child: Column(
        children: [
          _ContextStrip(organisation: _organisation),
          const SizedBox(height: AppSpacing.md),
          GridView.count(
            crossAxisCount: MediaQuery.sizeOf(context).width < 900 ? 1 : 3,
            crossAxisSpacing: AppSpacing.md,
            mainAxisSpacing: AppSpacing.md,
            childAspectRatio: 2.2,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            children: [
              _SummaryTile('Accounts', '${_accounts.length}',
                  Icons.account_tree_outlined, AppColors.metallicGold),
              _SummaryTile('Fiscal periods', '${_periods.length}',
                  Icons.calendar_month_outlined, AppColors.cobalt),
              _SummaryTile('Controls', 'Balanced', Icons.verified_user_outlined,
                  AppColors.success),
            ],
          ),
          const SizedBox(height: AppSpacing.md),
          _LiveDataTable(
            emptyText: 'No accounts yet.',
            columns: const ['Code', 'Name', 'Type', 'Status'],
            rows: [
              for (final account in _accounts)
                [
                  '${account['code'] ?? ''}',
                  '${account['name'] ?? ''}',
                  '${account['type'] ?? ''}',
                  '${account['status'] ?? ''}',
                ],
            ],
          ),
        ],
      ),
    );
  }

  Future<void> _load() async {
    final token = ref.read(authControllerProvider).accessToken;
    if (token == null) {
      setState(() {
        _loading = false;
        _error = 'Please sign in again.';
      });
      return;
    }

    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final api = ref.read(apiClientProvider);
      final organisations = await api.listOrganisations(token);
      final organisation = organisations.where((item) {
            return '${item['displayName']}' == 'Main Organisation';
          }).firstOrNull ??
          (organisations.isNotEmpty ? organisations.first : null);
      if (organisation == null) {
        throw StateError('No organisation exists.');
      }

      final organisationId = '${organisation['id']}';
      final accounts = await api.listAccounts(token, organisationId);
      final periods = await api.listFiscalPeriods(token, organisationId);
      await _connectRealtime(token, organisationId);

      setState(() {
        _organisation = organisation;
        _accounts = accounts;
        _periods = periods;
      });
    } catch (error) {
      setState(() => _error = 'Could not load accounting: $error');
    } finally {
      if (mounted) {
        setState(() => _loading = false);
      }
    }
  }

  Future<void> _connectRealtime(String token, String organisationId) async {
    await ref
        .read(realtimeServiceProvider)
        .connect(accessToken: token, organisationId: organisationId);
    _realtimeSubscription ??=
        ref.read(realtimeServiceProvider).changes.listen((event) {
      if (mounted &&
          event.organisationId == organisationId &&
          event.module == 'accounting') {
        _load();
      }
    });
  }

  Future<void> _openAccount() async {
    final created = await showDialog<bool>(
      context: context,
      builder: (context) => _CreateAccountDialog(organisation: _organisation),
    );
    if (created == true) {
      await _load();
    }
  }

  Future<void> _openJournal() async {
    final created = await showDialog<bool>(
      context: context,
      builder: (context) => _CreateJournalDialog(
        organisation: _organisation,
        accounts: _accounts,
      ),
    );
    if (created == true) {
      await _load();
    }
  }
}

class _SettingsWorkspaceState extends ConsumerState<_SettingsWorkspace> {
  final _currentPassword = TextEditingController();
  final _newPassword = TextEditingController();
  bool _obscure = true;
  bool _accessLoading = true;
  String? _accessError;
  List<Map<String, dynamic>> _accessUsers = [];
  List<Map<String, dynamic>> _permissions = [];

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadAccess());
  }

  @override
  void dispose() {
    _currentPassword.dispose();
    _newPassword.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final auth = ref.watch(authControllerProvider);

    return LayoutBuilder(
      builder: (context, constraints) {
        final padding = AppBreakpoints.pagePadding(constraints.maxWidth);

        return SingleChildScrollView(
          padding: EdgeInsets.all(padding),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Security settings',
                  style: Theme.of(context).textTheme.headlineLarge),
              const SizedBox(height: AppSpacing.xs),
              Text(
                  'Manage account access, sessions, password hygiene, and protected operations.',
                  style: Theme.of(context).textTheme.bodyMedium),
              const SizedBox(height: AppSpacing.lg),
              ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 720),
                child: Card(
                  child: Padding(
                    padding: const EdgeInsets.all(AppSpacing.lg),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        Text('Change password',
                            style: Theme.of(context).textTheme.titleLarge),
                        const SizedBox(height: AppSpacing.md),
                        TextField(
                          controller: _currentPassword,
                          obscureText: _obscure,
                          decoration: const InputDecoration(
                              labelText: 'Current password',
                              prefixIcon: Icon(Icons.lock_outline)),
                        ),
                        const SizedBox(height: AppSpacing.md),
                        TextField(
                          controller: _newPassword,
                          obscureText: _obscure,
                          decoration: InputDecoration(
                            labelText: 'New password',
                            prefixIcon: const Icon(Icons.password),
                            suffixIcon: IconButton(
                              tooltip:
                                  _obscure ? 'Show password' : 'Hide password',
                              onPressed: () =>
                                  setState(() => _obscure = !_obscure),
                              icon: Icon(_obscure
                                  ? Icons.visibility_outlined
                                  : Icons.visibility_off_outlined),
                            ),
                          ),
                        ),
                        if (auth.error != null) ...[
                          const SizedBox(height: AppSpacing.md),
                          Text(auth.error!,
                              style: const TextStyle(color: AppColors.danger)),
                        ],
                        const SizedBox(height: AppSpacing.lg),
                        FilledButton.icon(
                          onPressed: auth.isBusy ? null : _changePassword,
                          icon: const Icon(Icons.verified_user_outlined),
                          label: const Text('Update password'),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
              const SizedBox(height: AppSpacing.lg),
              _AccessManagementPanel(
                loading: _accessLoading,
                error: _accessError,
                users: _accessUsers,
                permissions: _permissions,
                onRefresh: _loadAccess,
                onCreateUser: _createAccessUser,
                onEditPermissions: _editAccessUserPermissions,
                onSetDisabled: _setAccessUserDisabled,
              ),
            ],
          ),
        );
      },
    );
  }

  Future<void> _changePassword() async {
    final ok = await ref.read(authControllerProvider).changePassword(
          currentPassword: _currentPassword.text,
          newPassword: _newPassword.text,
        );

    if (ok && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Password changed. Sign in again.')),
      );
    }
  }

  Future<void> _loadAccess() async {
    final token = ref.read(authControllerProvider).accessToken;
    if (token == null) {
      setState(() {
        _accessLoading = false;
        _accessError = 'Sign in again before managing access.';
      });
      return;
    }

    setState(() {
      _accessLoading = true;
      _accessError = null;
    });

    try {
      final api = ref.read(apiClientProvider);
      final permissions = await api.listAccessPermissions(token);
      final users = await api.listAccessUsers(token);
      setState(() {
        _permissions = permissions;
        _accessUsers = users;
      });
    } catch (error) {
      setState(() => _accessError = 'Could not load access management: $error');
    } finally {
      if (mounted) {
        setState(() => _accessLoading = false);
      }
    }
  }

  Future<void> _createAccessUser() async {
    final created = await showDialog<bool>(
      context: context,
      builder: (context) => _CreateAccessUserDialog(permissions: _permissions),
    );

    if (created == true) {
      await _loadAccess();
    }
  }

  Future<void> _editAccessUserPermissions(Map<String, dynamic> user) async {
    final updated = await showDialog<bool>(
      context: context,
      builder: (context) =>
          _EditPermissionsDialog(user: user, permissions: _permissions),
    );

    if (updated == true) {
      await _loadAccess();
    }
  }

  Future<void> _setAccessUserDisabled(
      Map<String, dynamic> user, bool disabled) async {
    final token = ref.read(authControllerProvider).accessToken;
    if (token == null) {
      return;
    }

    try {
      await ref.read(apiClientProvider).setAccessUserDisabled(
            token,
            userId: '${user['id']}',
            disabled: disabled,
          );
      await _loadAccess();
    } catch (error) {
      setState(() => _accessError = 'Could not update user status: $error');
    }
  }
}

class _AccessManagementPanel extends StatelessWidget {
  const _AccessManagementPanel({
    required this.loading,
    required this.error,
    required this.users,
    required this.permissions,
    required this.onRefresh,
    required this.onCreateUser,
    required this.onEditPermissions,
    required this.onSetDisabled,
  });

  final bool loading;
  final String? error;
  final List<Map<String, dynamic>> users;
  final List<Map<String, dynamic>> permissions;
  final VoidCallback onRefresh;
  final VoidCallback onCreateUser;
  final void Function(Map<String, dynamic> user) onEditPermissions;
  final void Function(Map<String, dynamic> user, bool disabled) onSetDisabled;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Wrap(
              spacing: AppSpacing.md,
              runSpacing: AppSpacing.sm,
              alignment: WrapAlignment.spaceBetween,
              crossAxisAlignment: WrapCrossAlignment.center,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Admin and managers',
                        style: Theme.of(context).textTheme.titleLarge),
                    const SizedBox(height: AppSpacing.xs),
                    Text('Create users and assign protected permissions.',
                        style: Theme.of(context).textTheme.bodyMedium),
                  ],
                ),
                Wrap(
                  spacing: AppSpacing.sm,
                  children: [
                    IconButton.filledTonal(
                        onPressed: loading ? null : onRefresh,
                        icon: const Icon(Icons.refresh),
                        tooltip: 'Refresh'),
                    FilledButton.icon(
                        onPressed: loading || permissions.isEmpty
                            ? null
                            : onCreateUser,
                        icon: const Icon(Icons.person_add_alt_1),
                        label: const Text('New manager')),
                  ],
                ),
              ],
            ),
            if (error != null) ...[
              const SizedBox(height: AppSpacing.md),
              Text(error!, style: const TextStyle(color: AppColors.danger)),
            ],
            const SizedBox(height: AppSpacing.md),
            if (loading)
              const LinearProgressIndicator()
            else
              _LiveDataTable(
                emptyText: 'No users found.',
                columns: const ['User', 'Status', 'Permissions', 'Actions'],
                rows: [
                  for (final user in users)
                    [
                      '${user['displayName'] ?? ''}\n${user['email'] ?? ''}',
                      (user['isDisabled'] == true) ? 'Disabled' : 'Active',
                      _permissionSummary(user['permissions']),
                    ],
                ],
                rowActions: [
                  for (final user in users)
                    Wrap(
                      spacing: AppSpacing.xs,
                      children: [
                        IconButton.filledTonal(
                          onPressed: () => onEditPermissions(user),
                          icon: const Icon(Icons.admin_panel_settings_outlined),
                          tooltip: 'Edit permissions',
                        ),
                        IconButton.filledTonal(
                          onPressed: () =>
                              onSetDisabled(user, user['isDisabled'] != true),
                          icon: Icon(user['isDisabled'] == true
                              ? Icons.lock_open_outlined
                              : Icons.lock_outline),
                          tooltip: user['isDisabled'] == true
                              ? 'Enable user'
                              : 'Disable user',
                        ),
                      ],
                    ),
                ],
              ),
          ],
        ),
      ),
    );
  }
}

class _StockAdjustmentDialog extends ConsumerStatefulWidget {
  const _StockAdjustmentDialog({
    required this.organisation,
    required this.warehouse,
    required this.products,
  });

  final Map<String, dynamic>? organisation;
  final Map<String, dynamic>? warehouse;
  final List<Map<String, dynamic>> products;

  @override
  ConsumerState<_StockAdjustmentDialog> createState() =>
      _StockAdjustmentDialogState();
}

class _StockAdjustmentDialogState
    extends ConsumerState<_StockAdjustmentDialog> {
  String? _productId;
  String _movementType = 'AdjustmentIn';
  final _quantity = TextEditingController(text: '1');
  final _notes = TextEditingController(text: 'Manual stock adjustment');
  bool _busy = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _productId =
        widget.products.isNotEmpty ? '${widget.products.first['id']}' : null;
  }

  @override
  void dispose() {
    _quantity.dispose();
    _notes.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Adjust stock'),
      content: SizedBox(
        width: 520,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            DropdownButtonFormField<String>(
              initialValue: _productId,
              items: [
                for (final product in widget.products)
                  DropdownMenuItem(
                    value: '${product['id']}',
                    child:
                        Text('${product['productCode']} - ${product['name']}'),
                  ),
              ],
              onChanged: (value) => setState(() => _productId = value),
              decoration: const InputDecoration(labelText: 'Product'),
            ),
            const SizedBox(height: AppSpacing.md),
            SegmentedButton<String>(
              segments: const [
                ButtonSegment(value: 'AdjustmentIn', label: Text('Add')),
                ButtonSegment(value: 'AdjustmentOut', label: Text('Remove')),
              ],
              selected: {_movementType},
              onSelectionChanged: (value) =>
                  setState(() => _movementType = value.first),
            ),
            const SizedBox(height: AppSpacing.md),
            TextField(
              controller: _quantity,
              keyboardType: TextInputType.number,
              decoration: const InputDecoration(labelText: 'Quantity'),
            ),
            const SizedBox(height: AppSpacing.md),
            TextField(
              controller: _notes,
              decoration: const InputDecoration(labelText: 'Notes'),
            ),
            if (_error != null) ...[
              const SizedBox(height: AppSpacing.md),
              Text(_error!, style: const TextStyle(color: AppColors.danger)),
            ],
          ],
        ),
      ),
      actions: [
        TextButton(
            onPressed: _busy ? null : () => Navigator.of(context).pop(false),
            child: const Text('Cancel')),
        FilledButton.icon(
            onPressed: _busy ? null : _save,
            icon: const Icon(Icons.save_outlined),
            label: const Text('Post')),
      ],
    );
  }

  Future<void> _save() async {
    final token = ref.read(authControllerProvider).accessToken;
    final organisationId = '${widget.organisation?['id'] ?? ''}';
    final warehouseId = '${widget.warehouse?['id'] ?? ''}';
    final quantity = double.tryParse(_quantity.text);
    if (token == null ||
        organisationId.isEmpty ||
        warehouseId.isEmpty ||
        _productId == null ||
        quantity == null ||
        quantity <= 0) {
      setState(() =>
          _error = 'Product, warehouse, and positive quantity are required.');
      return;
    }

    setState(() {
      _busy = true;
      _error = null;
    });

    try {
      await ref.read(apiClientProvider).createStockAdjustment(
            token,
            organisationId: organisationId,
            warehouseId: warehouseId,
            productId: _productId!,
            movementType: _movementType,
            quantity: quantity,
            notes: _notes.text.trim(),
          );
      if (mounted) {
        Navigator.of(context).pop(true);
      }
    } catch (error) {
      setState(() => _error = 'Stock adjustment failed: $error');
    } finally {
      if (mounted) {
        setState(() => _busy = false);
      }
    }
  }
}

class _CreateSupplierDialog extends ConsumerStatefulWidget {
  const _CreateSupplierDialog({required this.organisation});

  final Map<String, dynamic>? organisation;

  @override
  ConsumerState<_CreateSupplierDialog> createState() =>
      _CreateSupplierDialogState();
}

class _CreateSupplierDialogState extends ConsumerState<_CreateSupplierDialog> {
  final _name = TextEditingController();
  final _phone = TextEditingController();
  final _email = TextEditingController();
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _name.dispose();
    _phone.dispose();
    _email.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('New supplier'),
      content: SizedBox(
        width: 520,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
                controller: _name,
                decoration: const InputDecoration(labelText: 'Supplier name')),
            const SizedBox(height: AppSpacing.md),
            TextField(
                controller: _phone,
                decoration: const InputDecoration(labelText: 'Phone')),
            const SizedBox(height: AppSpacing.md),
            TextField(
                controller: _email,
                decoration: const InputDecoration(labelText: 'Email')),
            if (_error != null) ...[
              const SizedBox(height: AppSpacing.md),
              Text(_error!, style: const TextStyle(color: AppColors.danger)),
            ],
          ],
        ),
      ),
      actions: [
        TextButton(
            onPressed: _busy ? null : () => Navigator.of(context).pop(false),
            child: const Text('Cancel')),
        FilledButton.icon(
            onPressed: _busy ? null : _save,
            icon: const Icon(Icons.save_outlined),
            label: const Text('Save')),
      ],
    );
  }

  Future<void> _save() async {
    final token = ref.read(authControllerProvider).accessToken;
    final organisationId = '${widget.organisation?['id'] ?? ''}';
    if (token == null || organisationId.isEmpty || _name.text.trim().isEmpty) {
      setState(() => _error = 'Supplier name is required.');
      return;
    }

    setState(() {
      _busy = true;
      _error = null;
    });

    try {
      await ref.read(apiClientProvider).createSupplier(
            token,
            organisationId: organisationId,
            supplierNumber: 'S-${DateTime.now().millisecondsSinceEpoch}',
            displayName: _name.text.trim(),
            phone: _phone.text.trim(),
            email: _email.text.trim(),
          );
      if (mounted) {
        Navigator.of(context).pop(true);
      }
    } catch (error) {
      setState(() => _error = 'Supplier could not be saved: $error');
    } finally {
      if (mounted) {
        setState(() => _busy = false);
      }
    }
  }
}

class _CreatePurchaseOrderDialog extends ConsumerStatefulWidget {
  const _CreatePurchaseOrderDialog({
    required this.organisation,
    required this.branch,
    required this.suppliers,
    required this.products,
  });

  final Map<String, dynamic>? organisation;
  final Map<String, dynamic>? branch;
  final List<Map<String, dynamic>> suppliers;
  final List<Map<String, dynamic>> products;

  @override
  ConsumerState<_CreatePurchaseOrderDialog> createState() =>
      _CreatePurchaseOrderDialogState();
}

class _CreatePurchaseOrderDialogState
    extends ConsumerState<_CreatePurchaseOrderDialog> {
  String? _supplierId;
  String? _productId;
  final _quantity = TextEditingController(text: '1');
  final _unitCost = TextEditingController();
  bool _busy = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _supplierId =
        widget.suppliers.isNotEmpty ? '${widget.suppliers.first['id']}' : null;
    _productId =
        widget.products.isNotEmpty ? '${widget.products.first['id']}' : null;
    if (widget.products.isNotEmpty) {
      _unitCost.text = '${widget.products.first['purchasePrice'] ?? 0}';
    }
  }

  @override
  void dispose() {
    _quantity.dispose();
    _unitCost.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('New purchase order'),
      content: SizedBox(
        width: 560,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            DropdownButtonFormField<String>(
              initialValue: _supplierId,
              items: [
                for (final supplier in widget.suppliers)
                  DropdownMenuItem(
                      value: '${supplier['id']}',
                      child: Text('${supplier['displayName']}')),
              ],
              onChanged: (value) => setState(() => _supplierId = value),
              decoration: const InputDecoration(labelText: 'Supplier'),
            ),
            const SizedBox(height: AppSpacing.md),
            DropdownButtonFormField<String>(
              initialValue: _productId,
              items: [
                for (final product in widget.products)
                  DropdownMenuItem(
                      value: '${product['id']}',
                      child: Text(
                          '${product['productCode']} - ${product['name']}')),
              ],
              onChanged: (value) {
                final product = widget.products
                    .where((item) => '${item['id']}' == value)
                    .firstOrNull;
                setState(() {
                  _productId = value;
                  _unitCost.text =
                      '${product?['purchasePrice'] ?? _unitCost.text}';
                });
              },
              decoration: const InputDecoration(labelText: 'Product'),
            ),
            const SizedBox(height: AppSpacing.md),
            TextField(
                controller: _quantity,
                keyboardType: TextInputType.number,
                decoration: const InputDecoration(labelText: 'Quantity')),
            const SizedBox(height: AppSpacing.md),
            TextField(
                controller: _unitCost,
                keyboardType: TextInputType.number,
                decoration: const InputDecoration(labelText: 'Unit cost')),
            if (_error != null) ...[
              const SizedBox(height: AppSpacing.md),
              Text(_error!, style: const TextStyle(color: AppColors.danger)),
            ],
          ],
        ),
      ),
      actions: [
        TextButton(
            onPressed: _busy ? null : () => Navigator.of(context).pop(false),
            child: const Text('Cancel')),
        FilledButton.icon(
            onPressed: _busy ? null : _save,
            icon: const Icon(Icons.save_outlined),
            label: const Text('Create')),
      ],
    );
  }

  Future<void> _save() async {
    final token = ref.read(authControllerProvider).accessToken;
    final organisationId = '${widget.organisation?['id'] ?? ''}';
    final branchId = '${widget.branch?['id'] ?? ''}';
    final quantity = double.tryParse(_quantity.text);
    final unitCost = double.tryParse(_unitCost.text);
    final product = widget.products
        .where((item) => '${item['id']}' == _productId)
        .firstOrNull;
    if (token == null ||
        organisationId.isEmpty ||
        branchId.isEmpty ||
        _supplierId == null ||
        _productId == null ||
        quantity == null ||
        unitCost == null ||
        quantity <= 0 ||
        unitCost < 0) {
      setState(
          () => _error = 'Supplier, product, quantity, and cost are required.');
      return;
    }

    setState(() {
      _busy = true;
      _error = null;
    });

    try {
      await ref.read(apiClientProvider).createPurchaseOrder(
            token,
            organisationId: organisationId,
            branchId: branchId,
            supplierId: _supplierId!,
            productId: _productId!,
            description: '${product?['name'] ?? 'Purchase item'}',
            quantity: quantity,
            unitCost: unitCost,
          );
      if (mounted) {
        Navigator.of(context).pop(true);
      }
    } catch (error) {
      setState(() => _error = 'Purchase order could not be created: $error');
    } finally {
      if (mounted) {
        setState(() => _busy = false);
      }
    }
  }
}

class _CreateAccountDialog extends ConsumerStatefulWidget {
  const _CreateAccountDialog({required this.organisation});

  final Map<String, dynamic>? organisation;

  @override
  ConsumerState<_CreateAccountDialog> createState() =>
      _CreateAccountDialogState();
}

class _CreateAccountDialogState extends ConsumerState<_CreateAccountDialog> {
  final _code = TextEditingController();
  final _name = TextEditingController();
  String _type = 'Asset';
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _code.dispose();
    _name.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('New account'),
      content: SizedBox(
        width: 520,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
                controller: _code,
                decoration: const InputDecoration(labelText: 'Code')),
            const SizedBox(height: AppSpacing.md),
            TextField(
                controller: _name,
                decoration: const InputDecoration(labelText: 'Name')),
            const SizedBox(height: AppSpacing.md),
            DropdownButtonFormField<String>(
              initialValue: _type,
              items: const [
                DropdownMenuItem(value: 'Asset', child: Text('Asset')),
                DropdownMenuItem(value: 'Liability', child: Text('Liability')),
                DropdownMenuItem(value: 'Equity', child: Text('Equity')),
                DropdownMenuItem(value: 'Revenue', child: Text('Revenue')),
                DropdownMenuItem(value: 'Expense', child: Text('Expense')),
              ],
              onChanged: (value) => setState(() => _type = value ?? _type),
              decoration: const InputDecoration(labelText: 'Type'),
            ),
            if (_error != null) ...[
              const SizedBox(height: AppSpacing.md),
              Text(_error!, style: const TextStyle(color: AppColors.danger)),
            ],
          ],
        ),
      ),
      actions: [
        TextButton(
            onPressed: _busy ? null : () => Navigator.of(context).pop(false),
            child: const Text('Cancel')),
        FilledButton.icon(
            onPressed: _busy ? null : _save,
            icon: const Icon(Icons.save_outlined),
            label: const Text('Save')),
      ],
    );
  }

  Future<void> _save() async {
    final token = ref.read(authControllerProvider).accessToken;
    final organisationId = '${widget.organisation?['id'] ?? ''}';
    if (token == null ||
        organisationId.isEmpty ||
        _code.text.trim().isEmpty ||
        _name.text.trim().isEmpty) {
      setState(() => _error = 'Code and name are required.');
      return;
    }

    setState(() {
      _busy = true;
      _error = null;
    });

    try {
      await ref.read(apiClientProvider).createAccount(
            token,
            organisationId: organisationId,
            code: _code.text.trim(),
            name: _name.text.trim(),
            type: _type,
          );
      if (mounted) {
        Navigator.of(context).pop(true);
      }
    } catch (error) {
      setState(() => _error = 'Account could not be saved: $error');
    } finally {
      if (mounted) {
        setState(() => _busy = false);
      }
    }
  }
}

class _CreateJournalDialog extends ConsumerStatefulWidget {
  const _CreateJournalDialog(
      {required this.organisation, required this.accounts});

  final Map<String, dynamic>? organisation;
  final List<Map<String, dynamic>> accounts;

  @override
  ConsumerState<_CreateJournalDialog> createState() =>
      _CreateJournalDialogState();
}

class _CreateJournalDialogState extends ConsumerState<_CreateJournalDialog> {
  String? _debitAccountId;
  String? _creditAccountId;
  final _amount = TextEditingController(text: '1000');
  final _description = TextEditingController(text: 'Manual journal');
  bool _busy = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _debitAccountId =
        widget.accounts.isNotEmpty ? '${widget.accounts.first['id']}' : null;
    _creditAccountId = widget.accounts.length > 1
        ? '${widget.accounts[1]['id']}'
        : _debitAccountId;
  }

  @override
  void dispose() {
    _amount.dispose();
    _description.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final accountItems = [
      for (final account in widget.accounts)
        DropdownMenuItem<String>(
          value: '${account['id']}',
          child: Text('${account['code']} - ${account['name']}'),
        )
    ];

    return AlertDialog(
      title: const Text('New journal'),
      content: SizedBox(
        width: 560,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            DropdownButtonFormField<String>(
              initialValue: _debitAccountId,
              items: accountItems,
              onChanged: (value) => setState(() => _debitAccountId = value),
              decoration: const InputDecoration(labelText: 'Debit account'),
            ),
            const SizedBox(height: AppSpacing.md),
            DropdownButtonFormField<String>(
              initialValue: _creditAccountId,
              items: accountItems,
              onChanged: (value) => setState(() => _creditAccountId = value),
              decoration: const InputDecoration(labelText: 'Credit account'),
            ),
            const SizedBox(height: AppSpacing.md),
            TextField(
                controller: _amount,
                keyboardType: TextInputType.number,
                decoration: const InputDecoration(labelText: 'Amount')),
            const SizedBox(height: AppSpacing.md),
            TextField(
                controller: _description,
                decoration: const InputDecoration(labelText: 'Description')),
            if (_error != null) ...[
              const SizedBox(height: AppSpacing.md),
              Text(_error!, style: const TextStyle(color: AppColors.danger)),
            ],
          ],
        ),
      ),
      actions: [
        TextButton(
            onPressed: _busy ? null : () => Navigator.of(context).pop(false),
            child: const Text('Cancel')),
        FilledButton.icon(
            onPressed: _busy ? null : _save,
            icon: const Icon(Icons.save_outlined),
            label: const Text('Post')),
      ],
    );
  }

  Future<void> _save() async {
    final token = ref.read(authControllerProvider).accessToken;
    final organisationId = '${widget.organisation?['id'] ?? ''}';
    final amount = double.tryParse(_amount.text);
    if (token == null ||
        organisationId.isEmpty ||
        _debitAccountId == null ||
        _creditAccountId == null ||
        _debitAccountId == _creditAccountId ||
        amount == null ||
        amount <= 0) {
      setState(() =>
          _error = 'Choose two different accounts and a positive amount.');
      return;
    }

    setState(() {
      _busy = true;
      _error = null;
    });

    try {
      await ref.read(apiClientProvider).createJournal(
            token,
            organisationId: organisationId,
            debitAccountId: _debitAccountId!,
            creditAccountId: _creditAccountId!,
            description: _description.text.trim().isEmpty
                ? 'Manual journal'
                : _description.text.trim(),
            amount: amount,
          );
      if (mounted) {
        Navigator.of(context).pop(true);
      }
    } catch (error) {
      setState(() => _error = 'Journal could not be posted: $error');
    } finally {
      if (mounted) {
        setState(() => _busy = false);
      }
    }
  }
}

class _CreateAccessUserDialog extends ConsumerStatefulWidget {
  const _CreateAccessUserDialog({required this.permissions});

  final List<Map<String, dynamic>> permissions;

  @override
  ConsumerState<_CreateAccessUserDialog> createState() =>
      _CreateAccessUserDialogState();
}

class _CreateAccessUserDialogState
    extends ConsumerState<_CreateAccessUserDialog> {
  final _name = TextEditingController();
  final _email = TextEditingController();
  final _password = TextEditingController(text: 'Manager123!');
  final Set<String> _selected = {'customers.manage', 'sales.manage'};
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _name.dispose();
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('New manager'),
      content: SizedBox(
        width: 560,
        child: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                  controller: _name,
                  decoration: const InputDecoration(
                      labelText: 'Display name',
                      prefixIcon: Icon(Icons.badge_outlined))),
              const SizedBox(height: AppSpacing.md),
              TextField(
                  controller: _email,
                  decoration: const InputDecoration(
                      labelText: 'Email',
                      prefixIcon: Icon(Icons.mail_outline))),
              const SizedBox(height: AppSpacing.md),
              TextField(
                  controller: _password,
                  obscureText: true,
                  decoration: const InputDecoration(
                      labelText: 'Temporary password',
                      prefixIcon: Icon(Icons.password))),
              const SizedBox(height: AppSpacing.md),
              _PermissionChecklist(
                  permissions: widget.permissions, selected: _selected),
              if (_error != null) ...[
                const SizedBox(height: AppSpacing.md),
                Text(_error!, style: const TextStyle(color: AppColors.danger)),
              ],
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
            onPressed: _busy ? null : () => Navigator.of(context).pop(false),
            child: const Text('Cancel')),
        FilledButton.icon(
            onPressed: _busy ? null : _create,
            icon: const Icon(Icons.person_add_alt_1),
            label: const Text('Create')),
      ],
    );
  }

  Future<void> _create() async {
    final token = ref.read(authControllerProvider).accessToken;
    if (token == null ||
        _name.text.trim().isEmpty ||
        _email.text.trim().isEmpty ||
        _password.text.isEmpty) {
      setState(() => _error = 'Name, email, and password are required.');
      return;
    }

    setState(() {
      _busy = true;
      _error = null;
    });

    try {
      await ref.read(apiClientProvider).createAccessUser(
            token,
            email: _email.text.trim(),
            displayName: _name.text.trim(),
            password: _password.text,
            permissions: _selected.toList(),
          );
      if (mounted) {
        Navigator.of(context).pop(true);
      }
    } catch (error) {
      setState(() => _error = 'User could not be created: $error');
    } finally {
      if (mounted) {
        setState(() => _busy = false);
      }
    }
  }
}

class _EditPermissionsDialog extends ConsumerStatefulWidget {
  const _EditPermissionsDialog({required this.user, required this.permissions});

  final Map<String, dynamic> user;
  final List<Map<String, dynamic>> permissions;

  @override
  ConsumerState<_EditPermissionsDialog> createState() =>
      _EditPermissionsDialogState();
}

class _EditPermissionsDialogState
    extends ConsumerState<_EditPermissionsDialog> {
  late final Set<String> _selected =
      _readPermissions(widget.user['permissions']).toSet();
  bool _busy = false;
  String? _error;

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text('Permissions: ${widget.user['displayName'] ?? ''}'),
      content: SizedBox(
        width: 560,
        child: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              _PermissionChecklist(
                  permissions: widget.permissions, selected: _selected),
              if (_error != null) ...[
                const SizedBox(height: AppSpacing.md),
                Text(_error!, style: const TextStyle(color: AppColors.danger)),
              ],
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
            onPressed: _busy ? null : () => Navigator.of(context).pop(false),
            child: const Text('Cancel')),
        FilledButton.icon(
            onPressed: _busy ? null : _save,
            icon: const Icon(Icons.save_outlined),
            label: const Text('Save')),
      ],
    );
  }

  Future<void> _save() async {
    final token = ref.read(authControllerProvider).accessToken;
    if (token == null) {
      return;
    }

    setState(() {
      _busy = true;
      _error = null;
    });

    try {
      await ref.read(apiClientProvider).updateAccessUserPermissions(
            token,
            userId: '${widget.user['id']}',
            permissions: _selected.toList(),
          );
      if (mounted) {
        Navigator.of(context).pop(true);
      }
    } catch (error) {
      setState(() => _error = 'Permissions could not be saved: $error');
    } finally {
      if (mounted) {
        setState(() => _busy = false);
      }
    }
  }
}

class _PermissionChecklist extends StatefulWidget {
  const _PermissionChecklist(
      {required this.permissions, required this.selected});

  final List<Map<String, dynamic>> permissions;
  final Set<String> selected;

  @override
  State<_PermissionChecklist> createState() => _PermissionChecklistState();
}

class _PermissionChecklistState extends State<_PermissionChecklist> {
  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        for (final permission in widget.permissions)
          CheckboxListTile(
            value: widget.selected.contains('${permission['key']}'),
            onChanged: (value) {
              setState(() {
                if (value == true) {
                  widget.selected.add('${permission['key']}');
                } else {
                  widget.selected.remove('${permission['key']}');
                }
              });
            },
            title: Text('${permission['key']}'),
            subtitle: Text('${permission['description']}'),
            controlAffinity: ListTileControlAffinity.leading,
            contentPadding: EdgeInsets.zero,
          ),
      ],
    );
  }
}

double _asDouble(Object? value) {
  if (value is num) {
    return value.toDouble();
  }

  return double.tryParse('$value') ?? 0;
}

String _permissionSummary(Object? value) {
  final permissions = _readPermissions(value);
  if (permissions.isEmpty) {
    return 'No permissions';
  }

  return permissions.join(', ');
}

List<String> _readPermissions(Object? value) {
  if (value is List) {
    return value.map((item) => '$item').toList();
  }

  return [];
}

String _nameFor(List<Map<String, dynamic>> records, String id, String field) {
  return '${records.where((item) => '${item['id']}' == id).firstOrNull?[field] ?? id}';
}

Future<Map<String, dynamic>?> _selectOperationalOrganisation(
  List<Map<String, dynamic>> organisations,
  Future<List<Map<String, dynamic>>> Function(String organisationId)
      loadContext,
) async {
  final mainOrganisation = organisations.where((organisation) {
    final displayName = '${organisation['displayName'] ?? ''}'.toLowerCase();
    final legalName = '${organisation['legalName'] ?? ''}'.toLowerCase();
    return displayName == 'main organisation' ||
        legalName == 'unify demo trading llc';
  }).firstOrNull;
  if (mainOrganisation != null) {
    return mainOrganisation;
  }

  Map<String, dynamic>? first;

  for (final organisation in organisations) {
    first ??= organisation;
    final organisationId = '${organisation['id'] ?? ''}';
    if (organisationId.isEmpty) {
      continue;
    }

    final contextItems = await loadContext(organisationId);
    if (contextItems.isNotEmpty) {
      return organisation;
    }
  }

  return first;
}

String _money(Object? value) {
  final amount = _asDouble(value);
  return 'PKR ${amount.toStringAsFixed(0)}';
}

String _shortDate(Object? value) {
  final parsed = DateTime.tryParse('$value')?.toLocal();
  if (parsed == null) {
    return '-';
  }

  return '${parsed.year}-${parsed.month.toString().padLeft(2, '0')}-${parsed.day.toString().padLeft(2, '0')}';
}

class ModuleDefinition {
  const ModuleDefinition({
    required this.id,
    required this.title,
    required this.description,
    required this.primaryAction,
    required this.icon,
    required this.color,
    required this.stages,
    required this.stageCounts,
    required this.health,
    required this.exceptions,
    required this.progress,
    required this.records,
  });

  final String id;
  final String title;
  final String description;
  final String primaryAction;
  final IconData icon;
  final Color color;
  final List<String> stages;
  final List<int> stageCounts;
  final String health;
  final String exceptions;
  final double progress;
  final List<ModuleRecord> records;
}

class ModuleRecord {
  const ModuleRecord(
      this.reference, this.name, this.owner, this.status, this.value);

  final String reference;
  final String name;
  final String owner;
  final String status;
  final String value;
}

class ModuleCatalog {
  static ModuleDefinition byId(String id) {
    return modules.firstWhere(
      (module) => module.id == id,
      orElse: () => modules.first,
    );
  }

  static const modules = [
    ModuleDefinition(
      id: 'customers',
      title: 'Customers',
      description:
          'Customer profiles, credit control, branch ownership, and receivable visibility.',
      primaryAction: 'New customer',
      icon: Icons.people_alt,
      color: AppColors.royalPurple,
      stages: [
        'Capture profile',
        'Validate credit',
        'Manage status',
        'Review ledger'
      ],
      stageCounts: [42, 8, 5, 27],
      health: 'Strong',
      exceptions: '5 holds',
      progress: 0.82,
      records: [
        ModuleRecord('C-1001', 'North Retail', 'Main', 'Active', 'PKR 420K'),
        ModuleRecord('C-1002', 'Al Noor Market', 'North', 'On hold', 'PKR 86K'),
        ModuleRecord('C-1003', 'City Wholesale', 'Main', 'Active', 'PKR 1.2M'),
      ],
    ),
    ModuleDefinition(
      id: 'sales',
      title: 'Sales',
      description:
          'Invoices, stock movement, customer ledger posting, and sales operations.',
      primaryAction: 'New sale',
      icon: Icons.point_of_sale,
      color: AppColors.cobalt,
      stages: [
        'Draft invoice',
        'Reserve stock',
        'Post sale',
        'Collect payment'
      ],
      stageCounts: [18, 11, 34, 23],
      health: 'On target',
      exceptions: '3 stock gaps',
      progress: 0.74,
      records: [
        ModuleRecord(
            'INV-1042', 'Walk-in sale', 'Counter', 'Posted', 'PKR 84K'),
        ModuleRecord('INV-1043', 'North Retail', 'Waqar', 'Draft', 'PKR 112K'),
        ModuleRecord(
            'INV-1044', 'City Wholesale', 'Sana', 'Posted', 'PKR 320K'),
      ],
    ),
    ModuleDefinition(
      id: 'inventory',
      title: 'Inventory',
      description:
          'Warehouse balances, stock movements, transfers, adjustments, and reorder checks.',
      primaryAction: 'Adjust stock',
      icon: Icons.inventory_2,
      color: AppColors.success,
      stages: ['Receive stock', 'Transfer', 'Adjust', 'Reconcile'],
      stageCounts: [14, 7, 3, 5],
      health: 'Stable',
      exceptions: '9 low items',
      progress: 0.68,
      records: [
        ModuleRecord('STK-881', 'Regulator 12kg', 'Main', 'Low', '44 units'),
        ModuleRecord(
            'STK-882', 'Cylinder 45kg', 'Warehouse', 'Healthy', '290 units'),
        ModuleRecord(
            'TRF-210', 'Main to North', 'Inventory', 'Pending', '36 units'),
      ],
    ),
    ModuleDefinition(
      id: 'purchasing',
      title: 'Purchasing',
      description:
          'Purchase orders, goods receipts, supplier invoices, and inbound stock flow.',
      primaryAction: 'New PO',
      icon: Icons.shopping_cart,
      color: AppColors.coral,
      stages: ['Request', 'Approve', 'Receive goods', 'Invoice'],
      stageCounts: [9, 6, 4, 12],
      health: 'Needs review',
      exceptions: '6 delayed',
      progress: 0.57,
      records: [
        ModuleRecord(
            'PO-3301', 'Gas Supply Co', 'Procurement', 'Open', 'PKR 900K'),
        ModuleRecord('GRN-821', 'Cylinder receipt', 'Warehouse', 'Received',
            '120 units'),
        ModuleRecord(
            'PIN-442', 'Supplier invoice', 'Finance', 'Draft', 'PKR 240K'),
      ],
    ),
    ModuleDefinition(
      id: 'accounting',
      title: 'Accounting',
      description:
          'Chart of accounts, fiscal periods, balanced journals, and financial controls.',
      primaryAction: 'New journal',
      icon: Icons.account_balance,
      color: AppColors.metallicGold,
      stages: ['Prepare', 'Validate period', 'Balance', 'Post'],
      stageCounts: [7, 2, 4, 15],
      health: 'Controlled',
      exceptions: '2 drafts',
      progress: 0.79,
      records: [
        ModuleRecord(
            'JE-104', 'Sales posting', 'Finance', 'Posted', 'PKR 842K'),
        ModuleRecord(
            'JE-105', 'Inventory valuation', 'Finance', 'Draft', 'PKR 190K'),
        ModuleRecord('ACC-220', 'Cash account', 'Finance', 'Active', 'PKR 0'),
      ],
    ),
    ModuleDefinition(
      id: 'reports',
      title: 'Reports',
      description:
          'Operational exports, financial packs, inventory reports, and audit review.',
      primaryAction: 'Generate report',
      icon: Icons.bar_chart,
      color: AppColors.mint,
      stages: ['Select report', 'Apply filters', 'Generate', 'Export'],
      stageCounts: [24, 13, 5, 19],
      health: 'Ready',
      exceptions: '0 failed',
      progress: 0.91,
      records: [
        ModuleRecord('RPT-001', 'Daily sales', 'Operations', 'Ready', 'PDF'),
        ModuleRecord(
            'RPT-002', 'Stock valuation', 'Inventory', 'Ready', 'Excel'),
        ModuleRecord('RPT-003', 'Audit trail', 'Security', 'Ready', 'CSV'),
      ],
    ),
    ModuleDefinition(
      id: 'settings',
      title: 'Settings',
      description: 'Account security and workspace settings.',
      primaryAction: 'Save',
      icon: Icons.settings,
      color: AppColors.royalPurple,
      stages: [],
      stageCounts: [],
      health: 'Secure',
      exceptions: '0',
      progress: 1,
      records: [],
    ),
  ];
}
