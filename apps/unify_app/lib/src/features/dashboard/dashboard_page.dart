import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../core/layout/app_breakpoints.dart';
import '../../core/theme/app_theme.dart';

class DashboardPage extends StatelessWidget {
  const DashboardPage({super.key});

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final width = constraints.maxWidth;
        final padding = AppBreakpoints.pagePadding(width);
        final columns = AppBreakpoints.gridColumns(width);

        return SingleChildScrollView(
          padding: EdgeInsets.all(padding),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _DashboardHeader(compact: AppBreakpoints.isMobile(width)),
              const SizedBox(height: AppSpacing.lg),
              GridView.count(
                crossAxisCount: columns,
                crossAxisSpacing: AppSpacing.md,
                mainAxisSpacing: AppSpacing.md,
                childAspectRatio: width < AppBreakpoints.mobile ? 1.9 : 1.55,
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                children: const [
                  _MetricCard(title: 'Today sales', value: 'PKR 842K', delta: '+18.4%', icon: Icons.point_of_sale, color: AppColors.royalPurple),
                  _MetricCard(title: 'Stock value', value: 'PKR 12.8M', delta: '+4.1%', icon: Icons.inventory_2, color: AppColors.cobalt),
                  _MetricCard(title: 'Receivables', value: 'PKR 2.4M', delta: '-6.2%', icon: Icons.account_balance_wallet, color: AppColors.coral),
                  _MetricCard(title: 'Open orders', value: '128', delta: '+9', icon: Icons.shopping_cart, color: AppColors.success),
                ],
              ),
              const SizedBox(height: AppSpacing.lg),
              Flex(
                direction: width < 980 ? Axis.vertical : Axis.horizontal,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Expanded(
                    flex: width < 980 ? 0 : 7,
                    child: const _RevenuePanel(),
                  ),
                  SizedBox(width: width < 980 ? 0 : AppSpacing.md, height: width < 980 ? AppSpacing.md : 0),
                  Expanded(
                    flex: width < 980 ? 0 : 4,
                    child: const _ActivityPanel(),
                  ),
                ],
              ),
              const SizedBox(height: AppSpacing.lg),
              const _OperationsTable(),
            ],
          ),
        );
      },
    );
  }
}

class _DashboardHeader extends StatelessWidget {
  const _DashboardHeader({required this.compact});

  final bool compact;

  @override
  Widget build(BuildContext context) {
    return Flex(
      direction: compact ? Axis.vertical : Axis.horizontal,
      crossAxisAlignment: compact ? CrossAxisAlignment.start : CrossAxisAlignment.center,
      children: [
        Expanded(
          flex: compact ? 0 : 1,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Operations dashboard', style: Theme.of(context).textTheme.headlineLarge),
              const SizedBox(height: AppSpacing.xs),
              Text(
                DateFormat('EEEE, MMM d').format(DateTime.now()),
                style: Theme.of(context).textTheme.bodyMedium,
              ),
            ],
          ),
        ),
        SizedBox(height: compact ? AppSpacing.md : 0),
        Wrap(
          spacing: AppSpacing.sm,
          runSpacing: AppSpacing.sm,
          children: [
            FilledButton.icon(onPressed: () {}, icon: const Icon(Icons.add), label: const Text('New sale')),
            OutlinedButton.icon(onPressed: () {}, icon: const Icon(Icons.download_outlined), label: const Text('Export')),
          ],
        ),
      ],
    );
  }
}

class _MetricCard extends StatelessWidget {
  const _MetricCard({required this.title, required this.value, required this.delta, required this.icon, required this.color});

  final String title;
  final String value;
  final String delta;
  final IconData icon;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: const Duration(milliseconds: 520),
      curve: Curves.easeOutCubic,
      builder: (context, value, child) => Opacity(
        opacity: value,
        child: Transform.translate(offset: Offset(0, 18 * (1 - value)), child: child),
      ),
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.md),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Row(
                children: [
                  Container(
                    width: 38,
                    height: 38,
                    decoration: BoxDecoration(
                      color: color.withValues(alpha: 0.12),
                      borderRadius: BorderRadius.circular(AppRadius.md),
                    ),
                    child: Icon(icon, color: color),
                  ),
                  const Spacer(),
                  Chip(label: Text(delta)),
                ],
              ),
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(value, style: Theme.of(context).textTheme.headlineMedium),
                  const SizedBox(height: AppSpacing.xs),
                  Text(title, style: Theme.of(context).textTheme.bodyMedium),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _RevenuePanel extends StatefulWidget {
  const _RevenuePanel();

  @override
  State<_RevenuePanel> createState() => _RevenuePanelState();
}

class _RevenuePanelState extends State<_RevenuePanel> {
  String _range = '30d';

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(child: Text('Revenue rhythm', style: Theme.of(context).textTheme.titleLarge)),
                SegmentedButton<String>(
                  segments: const [
                    ButtonSegment(value: '7d', label: Text('7d')),
                    ButtonSegment(value: '30d', label: Text('30d')),
                    ButtonSegment(value: '90d', label: Text('90d')),
                  ],
                  selected: {_range},
                  onSelectionChanged: (value) => setState(() => _range = value.first),
                ),
              ],
            ),
            const SizedBox(height: AppSpacing.lg),
            SizedBox(
              height: 260,
              child: CustomPaint(
                painter: _RevenueChartPainter(),
                child: const SizedBox.expand(),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _RevenueChartPainter extends CustomPainter {
  final values = const [36, 44, 39, 58, 53, 69, 72, 66, 81, 88, 84, 96];

  @override
  void paint(Canvas canvas, Size size) {
    final gridPaint = Paint()
      ..color = AppColors.line
      ..strokeWidth = 1;
    final fillPaint = Paint()
      ..shader = const LinearGradient(
        colors: [Color(0x334B0082), Color(0x1134D399)],
        begin: Alignment.topCenter,
        end: Alignment.bottomCenter,
      ).createShader(Offset.zero & size);
    final linePaint = Paint()
      ..color = AppColors.royalPurple
      ..strokeCap = StrokeCap.round
      ..strokeWidth = 3
      ..style = PaintingStyle.stroke;

    for (var i = 0; i < 5; i++) {
      final y = size.height * i / 4;
      canvas.drawLine(Offset(0, y), Offset(size.width, y), gridPaint);
    }

    final maxValue = values.reduce((a, b) => a > b ? a : b).toDouble();
    final step = size.width / (values.length - 1);
    final path = Path();
    final fill = Path();

    for (var i = 0; i < values.length; i++) {
      final point = Offset(step * i, size.height - (values[i] / maxValue) * size.height * 0.88);
      if (i == 0) {
        path.moveTo(point.dx, point.dy);
        fill.moveTo(point.dx, size.height);
        fill.lineTo(point.dx, point.dy);
      } else {
        path.lineTo(point.dx, point.dy);
        fill.lineTo(point.dx, point.dy);
      }
    }

    fill.lineTo(size.width, size.height);
    fill.close();
    canvas.drawPath(fill, fillPaint);
    canvas.drawPath(path, linePaint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}

class _ActivityPanel extends StatelessWidget {
  const _ActivityPanel();

  @override
  Widget build(BuildContext context) {
    const activities = [
      ('Sale posted', 'Invoice INV-1042 moved stock and ledger', AppColors.success),
      ('Low stock', 'Cylinder regulator below reorder point', AppColors.warning),
      ('Payment received', 'Customer balance updated', AppColors.cobalt),
      ('Audit event', 'Password security setting reviewed', AppColors.royalPurple),
    ];

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Live activity', style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: AppSpacing.md),
            for (final activity in activities) ...[
              _ActivityTile(title: activity.$1, subtitle: activity.$2, color: activity.$3),
              const Divider(height: AppSpacing.lg),
            ],
          ],
        ),
      ),
    );
  }
}

class _ActivityTile extends StatelessWidget {
  const _ActivityTile({required this.title, required this.subtitle, required this.color});

  final String title;
  final String subtitle;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          width: 10,
          height: 10,
          margin: const EdgeInsets.only(top: 6),
          decoration: BoxDecoration(color: color, shape: BoxShape.circle),
        ),
        const SizedBox(width: AppSpacing.md),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(title, style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: AppSpacing.xs),
              Text(subtitle, style: Theme.of(context).textTheme.bodyMedium),
            ],
          ),
        ),
      ],
    );
  }
}

class _OperationsTable extends StatelessWidget {
  const _OperationsTable();

  @override
  Widget build(BuildContext context) {
    final rows = [
      ('Sales', 'Invoice review', '12 pending', AppColors.royalPurple),
      ('Inventory', 'Transfer approval', '5 waiting', AppColors.cobalt),
      ('Purchasing', 'Goods receipts', '9 due today', AppColors.coral),
      ('Accounting', 'Journal posting', '3 drafts', AppColors.success),
    ];

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Operational focus', style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: AppSpacing.md),
            SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: DataTable(
                columns: const [
                  DataColumn(label: Text('Module')),
                  DataColumn(label: Text('Queue')),
                  DataColumn(label: Text('Status')),
                  DataColumn(label: Text('Action')),
                ],
                rows: [
                  for (final row in rows)
                    DataRow(
                      cells: [
                        DataCell(Row(children: [Icon(Icons.circle, color: row.$4, size: 10), const SizedBox(width: AppSpacing.sm), Text(row.$1)])),
                        DataCell(Text(row.$2)),
                        DataCell(Text(row.$3)),
                        DataCell(TextButton(onPressed: () {}, child: const Text('Open'))),
                      ],
                    ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
