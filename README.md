# Unify ERP

**Unify** is a cross-platform, offline-first ERP platform for Android and Windows.

## Technology Stack
- Flutter + Dart
- Riverpod
- GoRouter
- Drift + SQLite
- ASP.NET Core Web API (.NET LTS)
- C#
- Entity Framework Core
- PostgreSQL
- ASP.NET Core Identity
- JWT + Rotating Refresh Tokens
- Docker
- GitHub Actions

## Features
- Multi-tenant
- Multi-branch
- Multi-warehouse
- Offline-first synchronization
- Customers
- Suppliers
- Sales
- Purchases
- Inventory
- Accounting
- Expenses
- Cash & Bank
- Reports (Excel/PDF)
- Audit logs
- Device stock allocation
- LPG extension

## Repository
- apps/
- backend/
- modules/
- docs/
- deploy/
- .github/

## Setup
1. Clone repository.
2. Start PostgreSQL with Docker Compose.
3. Apply EF Core migrations.
4. Run backend.
5. Run Flutter app.
6. Log in with seeded development account.

## Documentation
See:
- MASTER_PROMPT.md
- PROJECT_SPECIFICATION.md
- docs/architecture.md
- docs/database-design.md
- docs/api-design.md
- docs/synchronization.md
- docs/business-rules.md
- docs/security.md
- docs/deployment.md
- docs/release-plan.md
- docs/coding-standards.md

## Vision
Build a secure, scalable, maintainable ERP suitable for commercial deployment while remaining offline-first and modular.
