import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/layout/app_breakpoints.dart';
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
                  SizedBox(width: width < 980 ? 0 : AppSpacing.md, height: width < 980 ? AppSpacing.md : 0),
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

class _ModuleHeader extends StatelessWidget {
  const _ModuleHeader({required this.module, required this.compact});

  final ModuleDefinition module;
  final bool compact;

  @override
  Widget build(BuildContext context) {
    return Flex(
      direction: compact ? Axis.vertical : Axis.horizontal,
      crossAxisAlignment: compact ? CrossAxisAlignment.start : CrossAxisAlignment.center,
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
              Text(module.title, style: Theme.of(context).textTheme.headlineLarge),
              const SizedBox(height: AppSpacing.xs),
              Text(module.description, style: Theme.of(context).textTheme.bodyMedium),
            ],
          ),
        ),
        SizedBox(height: compact ? AppSpacing.md : 0),
        FilledButton.icon(onPressed: () {}, icon: const Icon(Icons.add), label: Text(module.primaryAction)),
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
                  DropdownMenuItem(value: 'All branches', child: Text('All branches')),
                  DropdownMenuItem(value: 'Main', child: Text('Main')),
                  DropdownMenuItem(value: 'North', child: Text('North')),
                ],
                onChanged: (value) => setState(() => _branch = value ?? _branch),
              ),
            ),
            SegmentedButton<String>(
              segments: const [
                ButtonSegment(value: 'Active', label: Text('Active')),
                ButtonSegment(value: 'Draft', label: Text('Draft')),
                ButtonSegment(value: 'Archived', label: Text('Archived')),
              ],
              selected: {_view},
              onSelectionChanged: (value) => setState(() => _view = value.first),
            ),
            OutlinedButton.icon(onPressed: () {}, icon: const Icon(Icons.tune), label: const Text('Filters')),
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
            Text('${module.title} workflow', style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: AppSpacing.md),
            for (var i = 0; i < module.stages.length; i++) ...[
              _StageTile(
                index: i + 1,
                title: module.stages[i],
                count: module.stageCounts[i],
                color: module.color,
              ),
              if (i != module.stages.length - 1) const SizedBox(height: AppSpacing.sm),
            ],
          ],
        ),
      ),
    );
  }
}

class _StageTile extends StatelessWidget {
  const _StageTile({required this.index, required this.title, required this.count, required this.color});

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
              child: Text('$index', style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w700)),
            ),
            const SizedBox(width: AppSpacing.md),
            Expanded(child: Text(title, style: Theme.of(context).textTheme.titleMedium)),
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
            _InsightRow(label: 'Cycle health', value: module.health, icon: Icons.speed),
            _InsightRow(label: 'Exceptions', value: module.exceptions, icon: Icons.warning_amber),
            _InsightRow(label: 'Audit coverage', value: 'Enabled', icon: Icons.verified_user_outlined),
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
  const _InsightRow({required this.label, required this.value, required this.icon});

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
            Text('Recent records', style: Theme.of(context).textTheme.titleLarge),
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

class _SettingsWorkspaceState extends ConsumerState<_SettingsWorkspace> {
  final _currentPassword = TextEditingController();
  final _newPassword = TextEditingController();
  bool _obscure = true;

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
              Text('Security settings', style: Theme.of(context).textTheme.headlineLarge),
              const SizedBox(height: AppSpacing.xs),
              Text('Manage account access, sessions, password hygiene, and protected operations.', style: Theme.of(context).textTheme.bodyMedium),
              const SizedBox(height: AppSpacing.lg),
              ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 720),
                child: Card(
                  child: Padding(
                    padding: const EdgeInsets.all(AppSpacing.lg),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        Text('Change password', style: Theme.of(context).textTheme.titleLarge),
                        const SizedBox(height: AppSpacing.md),
                        TextField(
                          controller: _currentPassword,
                          obscureText: _obscure,
                          decoration: const InputDecoration(labelText: 'Current password', prefixIcon: Icon(Icons.lock_outline)),
                        ),
                        const SizedBox(height: AppSpacing.md),
                        TextField(
                          controller: _newPassword,
                          obscureText: _obscure,
                          decoration: InputDecoration(
                            labelText: 'New password',
                            prefixIcon: const Icon(Icons.password),
                            suffixIcon: IconButton(
                              tooltip: _obscure ? 'Show password' : 'Hide password',
                              onPressed: () => setState(() => _obscure = !_obscure),
                              icon: Icon(_obscure ? Icons.visibility_outlined : Icons.visibility_off_outlined),
                            ),
                          ),
                        ),
                        if (auth.error != null) ...[
                          const SizedBox(height: AppSpacing.md),
                          Text(auth.error!, style: const TextStyle(color: AppColors.danger)),
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
  const ModuleRecord(this.reference, this.name, this.owner, this.status, this.value);

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
      description: 'Customer profiles, credit control, branch ownership, and receivable visibility.',
      primaryAction: 'New customer',
      icon: Icons.people_alt,
      color: AppColors.royalPurple,
      stages: ['Capture profile', 'Validate credit', 'Manage status', 'Review ledger'],
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
      description: 'Invoices, stock movement, customer ledger posting, and sales operations.',
      primaryAction: 'New sale',
      icon: Icons.point_of_sale,
      color: AppColors.cobalt,
      stages: ['Draft invoice', 'Reserve stock', 'Post sale', 'Collect payment'],
      stageCounts: [18, 11, 34, 23],
      health: 'On target',
      exceptions: '3 stock gaps',
      progress: 0.74,
      records: [
        ModuleRecord('INV-1042', 'Walk-in sale', 'Counter', 'Posted', 'PKR 84K'),
        ModuleRecord('INV-1043', 'North Retail', 'Waqar', 'Draft', 'PKR 112K'),
        ModuleRecord('INV-1044', 'City Wholesale', 'Sana', 'Posted', 'PKR 320K'),
      ],
    ),
    ModuleDefinition(
      id: 'inventory',
      title: 'Inventory',
      description: 'Warehouse balances, stock movements, transfers, adjustments, and reorder checks.',
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
        ModuleRecord('STK-882', 'Cylinder 45kg', 'Warehouse', 'Healthy', '290 units'),
        ModuleRecord('TRF-210', 'Main to North', 'Inventory', 'Pending', '36 units'),
      ],
    ),
    ModuleDefinition(
      id: 'purchasing',
      title: 'Purchasing',
      description: 'Purchase orders, goods receipts, supplier invoices, and inbound stock flow.',
      primaryAction: 'New PO',
      icon: Icons.shopping_cart,
      color: AppColors.coral,
      stages: ['Request', 'Approve', 'Receive goods', 'Invoice'],
      stageCounts: [9, 6, 4, 12],
      health: 'Needs review',
      exceptions: '6 delayed',
      progress: 0.57,
      records: [
        ModuleRecord('PO-3301', 'Gas Supply Co', 'Procurement', 'Open', 'PKR 900K'),
        ModuleRecord('GRN-821', 'Cylinder receipt', 'Warehouse', 'Received', '120 units'),
        ModuleRecord('PIN-442', 'Supplier invoice', 'Finance', 'Draft', 'PKR 240K'),
      ],
    ),
    ModuleDefinition(
      id: 'accounting',
      title: 'Accounting',
      description: 'Chart of accounts, fiscal periods, balanced journals, and financial controls.',
      primaryAction: 'New journal',
      icon: Icons.account_balance,
      color: AppColors.metallicGold,
      stages: ['Prepare', 'Validate period', 'Balance', 'Post'],
      stageCounts: [7, 2, 4, 15],
      health: 'Controlled',
      exceptions: '2 drafts',
      progress: 0.79,
      records: [
        ModuleRecord('JE-104', 'Sales posting', 'Finance', 'Posted', 'PKR 842K'),
        ModuleRecord('JE-105', 'Inventory valuation', 'Finance', 'Draft', 'PKR 190K'),
        ModuleRecord('ACC-220', 'Cash account', 'Finance', 'Active', 'PKR 0'),
      ],
    ),
    ModuleDefinition(
      id: 'reports',
      title: 'Reports',
      description: 'Operational exports, financial packs, inventory reports, and audit review.',
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
        ModuleRecord('RPT-002', 'Stock valuation', 'Inventory', 'Ready', 'Excel'),
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
