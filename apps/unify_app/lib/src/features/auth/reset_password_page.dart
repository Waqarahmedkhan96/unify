import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../core/theme/app_theme.dart';
import 'auth_controller.dart';

class ResetPasswordPage extends ConsumerStatefulWidget {
  const ResetPasswordPage({this.email, this.token, super.key});

  final String? email;
  final String? token;

  @override
  ConsumerState<ResetPasswordPage> createState() => _ResetPasswordPageState();
}

class _ResetPasswordPageState extends ConsumerState<ResetPasswordPage> {
  late final TextEditingController _emailController;
  late final TextEditingController _tokenController;
  final _passwordController = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  bool _obscure = true;

  @override
  void initState() {
    super.initState();
    _emailController = TextEditingController(text: widget.email ?? '');
    _tokenController = TextEditingController(text: widget.token ?? '');
  }

  @override
  void dispose() {
    _emailController.dispose();
    _tokenController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final auth = ref.watch(authControllerProvider);

    return Scaffold(
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 520),
            child: Card(
              child: Padding(
                padding: const EdgeInsets.all(AppSpacing.lg),
                child: Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text('Reset password', style: Theme.of(context).textTheme.headlineMedium),
                      const SizedBox(height: AppSpacing.sm),
                      Text('Paste the secure reset token from your recovery email.', style: Theme.of(context).textTheme.bodyMedium),
                      const SizedBox(height: AppSpacing.lg),
                      TextFormField(
                        controller: _emailController,
                        decoration: const InputDecoration(labelText: 'Email', prefixIcon: Icon(Icons.mail_outline)),
                        validator: (value) => value == null || value.trim().isEmpty ? 'Email is required' : null,
                      ),
                      const SizedBox(height: AppSpacing.md),
                      TextFormField(
                        controller: _tokenController,
                        decoration: const InputDecoration(labelText: 'Reset token', prefixIcon: Icon(Icons.key_outlined)),
                        validator: (value) => value == null || value.trim().isEmpty ? 'Reset token is required' : null,
                      ),
                      const SizedBox(height: AppSpacing.md),
                      TextFormField(
                        controller: _passwordController,
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
                        validator: (value) => value == null || value.length < 12 ? 'Use at least 12 characters' : null,
                      ),
                      if (auth.error != null) ...[
                        const SizedBox(height: AppSpacing.md),
                        Text(auth.error!, style: const TextStyle(color: AppColors.danger)),
                      ],
                      const SizedBox(height: AppSpacing.lg),
                      FilledButton.icon(
                        onPressed: auth.isBusy ? null : _submit,
                        icon: const Icon(Icons.check_circle_outline),
                        label: const Text('Reset password'),
                      ),
                      const SizedBox(height: AppSpacing.sm),
                      TextButton(
                        onPressed: () => context.go('/login'),
                        child: const Text('Back to sign in'),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final ok = await ref.read(authControllerProvider).resetPassword(
          email: _emailController.text.trim(),
          resetToken: _tokenController.text.trim(),
          newPassword: _passwordController.text,
        );

    if (ok && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Password reset. Sign in with your new password.')));
      context.go('/login');
    }
  }
}
