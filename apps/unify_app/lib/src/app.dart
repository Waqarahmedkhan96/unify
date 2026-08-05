import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'core/theme/app_theme.dart';
import 'features/auth/auth_controller.dart';
import 'features/auth/login_page.dart';
import 'features/auth/reset_password_page.dart';
import 'features/dashboard/dashboard_page.dart';
import 'features/modules/module_page.dart';
import 'features/shell/app_shell.dart';

final routerProvider = Provider<GoRouter>((ref) {
  final authState = ref.watch(authControllerProvider);

  return GoRouter(
    initialLocation: '/dashboard',
    refreshListenable: authState,
    redirect: (context, state) {
      final signedIn = authState.isAuthenticated;
      final loggingIn = state.uri.path == '/login';

      if (!signedIn && !loggingIn) {
        return '/login';
      }

      if (signedIn && loggingIn) {
        return '/dashboard';
      }

      return null;
    },
    routes: [
      GoRoute(
        path: '/login',
        builder: (context, state) => const LoginPage(),
      ),
      GoRoute(
        path: '/reset-password',
        builder: (context, state) => ResetPasswordPage(
          email: state.uri.queryParameters['email'],
          token: state.uri.queryParameters['token'],
        ),
      ),
      ShellRoute(
        builder: (context, state, child) => AppShell(child: child),
        routes: [
          GoRoute(
            path: '/dashboard',
            builder: (context, state) => const DashboardPage(),
          ),
          GoRoute(
            path: '/customers',
            builder: (context, state) => const ModulePage(moduleId: 'customers'),
          ),
          GoRoute(
            path: '/sales',
            builder: (context, state) => const ModulePage(moduleId: 'sales'),
          ),
          GoRoute(
            path: '/inventory',
            builder: (context, state) => const ModulePage(moduleId: 'inventory'),
          ),
          GoRoute(
            path: '/purchasing',
            builder: (context, state) => const ModulePage(moduleId: 'purchasing'),
          ),
          GoRoute(
            path: '/accounting',
            builder: (context, state) => const ModulePage(moduleId: 'accounting'),
          ),
          GoRoute(
            path: '/reports',
            builder: (context, state) => const ModulePage(moduleId: 'reports'),
          ),
          GoRoute(
            path: '/settings',
            builder: (context, state) => const ModulePage(moduleId: 'settings'),
          ),
        ],
      ),
    ],
  );
});

class UnifyApp extends ConsumerWidget {
  const UnifyApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final router = ref.watch(routerProvider);

    return MaterialApp.router(
      title: 'Unify ERP',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light(),
      routerConfig: router,
    );
  }
}
