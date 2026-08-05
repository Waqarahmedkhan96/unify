import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../core/layout/app_breakpoints.dart';
import '../../core/theme/app_theme.dart';
import '../auth/auth_controller.dart';

class AppShell extends ConsumerWidget {
  const AppShell({required this.child, super.key});

  final Widget child;

  static const _items = [
    _NavItem('Dashboard', '/dashboard', Icons.dashboard_outlined, Icons.dashboard),
    _NavItem('Customers', '/customers', Icons.people_alt_outlined, Icons.people_alt),
    _NavItem('Sales', '/sales', Icons.point_of_sale_outlined, Icons.point_of_sale),
    _NavItem('Inventory', '/inventory', Icons.inventory_2_outlined, Icons.inventory_2),
    _NavItem('Purchasing', '/purchasing', Icons.shopping_cart_outlined, Icons.shopping_cart),
    _NavItem('Accounting', '/accounting', Icons.account_balance_outlined, Icons.account_balance),
    _NavItem('Reports', '/reports', Icons.bar_chart_outlined, Icons.bar_chart),
    _NavItem('Settings', '/settings', Icons.settings_outlined, Icons.settings),
  ];

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final mobile = AppBreakpoints.isMobile(constraints.maxWidth);
        final selectedIndex = _selectedIndex(context);

        if (mobile) {
          return Scaffold(
            appBar: AppBar(
              title: const Text('Unify ERP'),
              actions: [_AccountMenu(compact: true)],
            ),
            drawer: _MobileDrawer(items: _items, selectedIndex: selectedIndex),
            body: _AnimatedShellBody(child: child),
          );
        }

        return Scaffold(
          body: Row(
            children: [
              _DesktopNavigation(items: _items, selectedIndex: selectedIndex),
              Expanded(
                child: Column(
                  children: [
                    const _TopBar(),
                    Expanded(child: _AnimatedShellBody(child: child)),
                  ],
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  int _selectedIndex(BuildContext context) {
    final location = GoRouterState.of(context).uri.path;
    final index = _items.indexWhere((item) => location == item.route);

    return index < 0 ? 0 : index;
  }
}

class _DesktopNavigation extends StatelessWidget {
  const _DesktopNavigation({required this.items, required this.selectedIndex});

  final List<_NavItem> items;
  final int selectedIndex;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 264,
      decoration: const BoxDecoration(
        color: AppColors.surface,
        border: Border(right: BorderSide(color: AppColors.line)),
      ),
      child: SafeArea(
        child: Column(
          children: [
            const _BrandHeader(),
            Expanded(
              child: ListView.separated(
                padding: const EdgeInsets.all(AppSpacing.md),
                itemBuilder: (context, index) {
                  final item = items[index];
                  final selected = selectedIndex == index;

                  return _NavigationTile(
                    item: item,
                    selected: selected,
                    onTap: () => context.go(item.route),
                  );
                },
                separatorBuilder: (_, __) => const SizedBox(height: AppSpacing.xs),
                itemCount: items.length,
              ),
            ),
            const _SyncStatusPanel(),
          ],
        ),
      ),
    );
  }
}

class _MobileDrawer extends StatelessWidget {
  const _MobileDrawer({required this.items, required this.selectedIndex});

  final List<_NavItem> items;
  final int selectedIndex;

  @override
  Widget build(BuildContext context) {
    return Drawer(
      child: SafeArea(
        child: Column(
          children: [
            const _BrandHeader(),
            Expanded(
              child: ListView.builder(
                padding: const EdgeInsets.all(AppSpacing.md),
                itemCount: items.length,
                itemBuilder: (context, index) {
                  final item = items[index];

                  return _NavigationTile(
                    item: item,
                    selected: selectedIndex == index,
                    onTap: () {
                      Navigator.of(context).pop();
                      context.go(item.route);
                    },
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _BrandHeader extends StatelessWidget {
  const _BrandHeader();

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(AppSpacing.lg),
      child: Row(
        children: [
          Container(
            width: 42,
            height: 42,
            decoration: BoxDecoration(
              color: AppColors.royalPurple,
              borderRadius: BorderRadius.circular(AppRadius.md),
            ),
            child: const Icon(Icons.hub_outlined, color: AppColors.metallicGold),
          ),
          const SizedBox(width: AppSpacing.md),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Unify ERP', style: Theme.of(context).textTheme.titleLarge),
                Text('Operations Command', style: Theme.of(context).textTheme.bodyMedium),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _NavigationTile extends StatelessWidget {
  const _NavigationTile({required this.item, required this.selected, required this.onTap});

  final _NavItem item;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return AnimatedContainer(
      duration: const Duration(milliseconds: 180),
      curve: Curves.easeOut,
      decoration: BoxDecoration(
        color: selected ? AppColors.royalPurple.withValues(alpha: 0.09) : Colors.transparent,
        borderRadius: BorderRadius.circular(AppRadius.md),
      ),
      child: ListTile(
        dense: true,
        leading: Icon(selected ? item.selectedIcon : item.icon),
        title: Text(item.label),
        selected: selected,
        selectedColor: AppColors.royalPurple,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(AppRadius.md)),
        onTap: onTap,
      ),
    );
  }
}

class _TopBar extends ConsumerWidget {
  const _TopBar();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final auth = ref.watch(authControllerProvider);

    return Container(
      height: 72,
      padding: const EdgeInsets.symmetric(horizontal: AppSpacing.lg),
      decoration: const BoxDecoration(
        color: AppColors.surface,
        border: Border(bottom: BorderSide(color: AppColors.line)),
      ),
      child: Row(
        children: [
          Expanded(
            child: Text(
              'Live workspace',
              style: Theme.of(context).textTheme.titleLarge,
            ),
          ),
          const _OrganisationSelector(),
          const SizedBox(width: AppSpacing.md),
          Text(auth.displayEmail, style: Theme.of(context).textTheme.bodyMedium),
          const SizedBox(width: AppSpacing.sm),
          const _AccountMenu(compact: false),
        ],
      ),
    );
  }
}

class _OrganisationSelector extends StatelessWidget {
  const _OrganisationSelector();

  @override
  Widget build(BuildContext context) {
    return DropdownButtonHideUnderline(
      child: DropdownButton<String>(
        value: 'Main',
        borderRadius: BorderRadius.circular(AppRadius.md),
        items: const [
          DropdownMenuItem(value: 'Main', child: Text('Main Organisation')),
          DropdownMenuItem(value: 'North', child: Text('North Branch')),
          DropdownMenuItem(value: 'Wholesale', child: Text('Wholesale Desk')),
        ],
        onChanged: (_) {},
      ),
    );
  }
}

class _AccountMenu extends ConsumerWidget {
  const _AccountMenu({required this.compact});

  final bool compact;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return PopupMenuButton<String>(
      tooltip: 'Account menu',
      icon: const CircleAvatar(
        radius: 18,
        backgroundColor: AppColors.royalPurple,
        child: Icon(Icons.person_outline, color: Colors.white, size: 20),
      ),
      onSelected: (value) {
        if (value == 'settings') {
          context.go('/settings');
        }

        if (value == 'logout') {
          ref.read(authControllerProvider).logout();
          context.go('/login');
        }
      },
      itemBuilder: (context) => const [
        PopupMenuItem(value: 'settings', child: Text('Security settings')),
        PopupMenuDivider(),
        PopupMenuItem(value: 'logout', child: Text('Sign out')),
      ],
    );
  }
}

class _SyncStatusPanel extends StatelessWidget {
  const _SyncStatusPanel();

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(AppSpacing.md),
      child: DecoratedBox(
        decoration: BoxDecoration(
          color: AppColors.canvas,
          borderRadius: BorderRadius.circular(AppRadius.md),
          border: Border.all(color: AppColors.line),
        ),
        child: const Padding(
          padding: EdgeInsets.all(AppSpacing.md),
          child: Row(
            children: [
              Icon(Icons.cloud_done_outlined, color: AppColors.success),
              SizedBox(width: AppSpacing.sm),
              Expanded(child: Text('Online sync ready')),
            ],
          ),
        ),
      ),
    );
  }
}

class _AnimatedShellBody extends StatelessWidget {
  const _AnimatedShellBody({required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return AnimatedSwitcher(
      duration: const Duration(milliseconds: 260),
      switchInCurve: Curves.easeOutCubic,
      switchOutCurve: Curves.easeInCubic,
      transitionBuilder: (child, animation) {
        return FadeTransition(
          opacity: animation,
          child: SlideTransition(
            position: Tween<Offset>(
              begin: const Offset(0.02, 0),
              end: Offset.zero,
            ).animate(animation),
            child: child,
          ),
        );
      },
      child: KeyedSubtree(
        key: ValueKey(GoRouterState.of(context).uri.path),
        child: child,
      ),
    );
  }
}

class _NavItem {
  const _NavItem(this.label, this.route, this.icon, this.selectedIcon);

  final String label;
  final String route;
  final IconData icon;
  final IconData selectedIcon;
}
