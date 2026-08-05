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
2. Copy `.env.example` to `.env` and adjust local values if required.
3. Start PostgreSQL and the API with Docker Compose:

   ```powershell
   docker compose up --build
   ```

4. Or run the backend directly:

   ```powershell
   dotnet run --project backend/src/Unify.Erp.Api/Unify.Erp.Api.csproj
   ```

5. Verify the API:

   ```powershell
   Invoke-RestMethod http://localhost:5080/api/v1/system/health
   ```

6. Generate the Flutter app after Flutter is installed:

   ```powershell
   Set-Location apps/unify_app
   flutter create --platforms android,windows --org com.unifyerp .
   ```

## Backend Verification

```powershell
dotnet build backend/Unify.Erp.sln --configuration Release
dotnet test backend/Unify.Erp.sln --configuration Release --no-build
```

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
