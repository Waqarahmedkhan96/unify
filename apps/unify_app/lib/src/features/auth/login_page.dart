import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../core/layout/app_breakpoints.dart';
import '../../core/theme/app_theme.dart';
import 'auth_controller.dart';

class LoginPage extends ConsumerStatefulWidget {
  const LoginPage({super.key});

  @override
  ConsumerState<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends ConsumerState<LoginPage> {
  final _emailController = TextEditingController(text: 'owner@unify.local');
  final _passwordController = TextEditingController(text: 'ChangeMe123!');
  final _formKey = GlobalKey<FormState>();
  bool _obscurePassword = true;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final auth = ref.watch(authControllerProvider);

    return Scaffold(
      body: LayoutBuilder(
        builder: (context, constraints) {
          final compact = AppBreakpoints.isMobile(constraints.maxWidth);

          return Container(
            decoration: const BoxDecoration(
              gradient: LinearGradient(
                colors: [AppColors.deepPurple, AppColors.royalPurple],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
            ),
            child: Center(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(AppSpacing.lg),
                child: ConstrainedBox(
                  constraints: BoxConstraints(maxWidth: compact ? 460 : 980),
                  child: Flex(
                    direction: compact ? Axis.vertical : Axis.horizontal,
                    children: [
                      Expanded(
                        flex: compact ? 0 : 5,
                        child: _BrandPanel(compact: compact),
                      ),
                      SizedBox(width: compact ? 0 : AppSpacing.xl, height: compact ? AppSpacing.lg : 0),
                      Expanded(
                        flex: compact ? 0 : 4,
                        child: AnimatedContainer(
                          duration: const Duration(milliseconds: 450),
                          curve: Curves.easeOutCubic,
                          padding: const EdgeInsets.all(AppSpacing.lg),
                          decoration: BoxDecoration(
                            color: Colors.white,
                            borderRadius: BorderRadius.circular(AppRadius.lg),
                            boxShadow: const [
                              BoxShadow(
                                color: Color(0x33000000),
                                blurRadius: 30,
                                offset: Offset(0, 18),
                              ),
                            ],
                          ),
                          child: Form(
                            key: _formKey,
                            child: Column(
                              mainAxisSize: MainAxisSize.min,
                              crossAxisAlignment: CrossAxisAlignment.stretch,
                              children: [
                                Text('Secure sign in', style: Theme.of(context).textTheme.headlineMedium),
                                const SizedBox(height: AppSpacing.sm),
                                Text(
                                  'Access operations, inventory, sales, purchasing, and accounting from one workspace.',
                                  style: Theme.of(context).textTheme.bodyMedium,
                                ),
                                const SizedBox(height: AppSpacing.lg),
                                TextFormField(
                                  controller: _emailController,
                                  keyboardType: TextInputType.emailAddress,
                                  decoration: const InputDecoration(
                                    labelText: 'Email',
                                    prefixIcon: Icon(Icons.mail_outline),
                                  ),
                                  validator: (value) => value == null || value.trim().isEmpty ? 'Email is required' : null,
                                ),
                                const SizedBox(height: AppSpacing.md),
                                TextFormField(
                                  controller: _passwordController,
                                  obscureText: _obscurePassword,
                                  decoration: InputDecoration(
                                    labelText: 'Password',
                                    prefixIcon: const Icon(Icons.lock_outline),
                                    suffixIcon: IconButton(
                                      tooltip: _obscurePassword ? 'Show password' : 'Hide password',
                                      onPressed: () => setState(() => _obscurePassword = !_obscurePassword),
                                      icon: Icon(_obscurePassword ? Icons.visibility_outlined : Icons.visibility_off_outlined),
                                    ),
                                  ),
                                  validator: (value) => value == null || value.isEmpty ? 'Password is required' : null,
                                ),
                                const SizedBox(height: AppSpacing.md),
                                Align(
                                  alignment: Alignment.centerRight,
                                  child: TextButton(
                                    onPressed: auth.isBusy ? null : _forgotPassword,
                                    child: const Text('Forgot password?'),
                                  ),
                                ),
                                if (auth.error != null) ...[
                                  const SizedBox(height: AppSpacing.sm),
                                  _InlineError(message: auth.error!),
                                ],
                                const SizedBox(height: AppSpacing.lg),
                                FilledButton.icon(
                                  onPressed: auth.isBusy ? null : _submit,
                                  icon: AnimatedSwitcher(
                                    duration: const Duration(milliseconds: 200),
                                    child: auth.isBusy
                                        ? const SizedBox(
                                            key: ValueKey('loading'),
                                            width: 18,
                                            height: 18,
                                            child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                                          )
                                        : const Icon(Icons.login, key: ValueKey('icon')),
                                  ),
                                  label: const Text('Sign in'),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          );
        },
      ),
    );
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final ok = await ref.read(authControllerProvider).login(
          email: _emailController.text.trim(),
          password: _passwordController.text,
        );

    if (ok && mounted) {
      context.go('/dashboard');
    }
  }

  Future<void> _forgotPassword() async {
    final email = _emailController.text.trim();
    if (email.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Enter your email first.')));
      return;
    }

    final ok = await ref.read(authControllerProvider).forgotPassword(email);
    if (ok && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('If the email exists, a reset message will be sent.')),
      );
    }
  }
}

class _BrandPanel extends StatelessWidget {
  const _BrandPanel({required this.compact});

  final bool compact;

  @override
  Widget build(BuildContext context) {
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: const Duration(milliseconds: 650),
      curve: Curves.easeOutCubic,
      builder: (context, value, child) => Opacity(
        opacity: value,
        child: Transform.translate(
          offset: Offset(0, 16 * (1 - value)),
          child: child,
        ),
      ),
      child: Column(
        crossAxisAlignment: compact ? CrossAxisAlignment.center : CrossAxisAlignment.start,
        children: [
          Container(
            width: 64,
            height: 64,
            decoration: BoxDecoration(
              color: AppColors.metallicGold,
              borderRadius: BorderRadius.circular(AppRadius.lg),
            ),
            child: const Icon(Icons.hub_outlined, color: AppColors.deepPurple, size: 34),
          ),
          const SizedBox(height: AppSpacing.lg),
          Text(
            'Unify ERP',
            textAlign: compact ? TextAlign.center : TextAlign.left,
            style: Theme.of(context).textTheme.headlineLarge?.copyWith(color: Colors.white, fontSize: compact ? 34 : 48),
          ),
          const SizedBox(height: AppSpacing.md),
          Text(
            'A secure command center for multi-branch operations, inventory, sales, purchasing, accounting, and audit visibility.',
            textAlign: compact ? TextAlign.center : TextAlign.left,
            style: Theme.of(context).textTheme.titleMedium?.copyWith(color: Colors.white.withValues(alpha: 0.86), height: 1.5),
          ),
        ],
      ),
    );
  }
}

class _InlineError extends StatelessWidget {
  const _InlineError({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: AppColors.danger.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(AppRadius.md),
        border: Border.all(color: AppColors.danger.withValues(alpha: 0.2)),
      ),
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Row(
          children: [
            const Icon(Icons.error_outline, color: AppColors.danger),
            const SizedBox(width: AppSpacing.sm),
            Expanded(child: Text(message)),
          ],
        ),
      ),
    );
  }
}
