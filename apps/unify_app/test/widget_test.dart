import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:unify_app/src/app.dart';

void main() {
  testWidgets('renders secure login screen', (tester) async {
    await tester.pumpWidget(const ProviderScope(child: UnifyApp()));
    await tester.pumpAndSettle();

    expect(find.text('Unify ERP'), findsOneWidget);
    expect(find.text('Secure sign in'), findsOneWidget);
    expect(find.byIcon(Icons.login), findsOneWidget);
  });
}
