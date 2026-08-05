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

6. In Development, a local owner account is seeded automatically:

   ```text
   owner@unify.local
   ChangeMe123!
   ```

7. Generate the Flutter app after Flutter is installed:

   ```powershell
   Set-Location apps/unify_app
   flutter create --platforms android,windows --org com.unifyerp .
   ```

## Backend Verification

```powershell
dotnet build backend/Unify.Erp.sln --configuration Release
dotnet test backend/Unify.Erp.sln --configuration Release --no-build
```

## Open The UI

```powershell
docker compose up -d --build
cd apps/unify_app
..\..\flutter\bin\flutter.bat run -d chrome --dart-define=UNIFY_API_URL=http://localhost:5080
```

Or open the built web bundle after:

```powershell
..\..\flutter\bin\flutter.bat build web --dart-define=UNIFY_API_URL=http://localhost:5080
cd build/web
python -m http.server 5200 --bind 127.0.0.1
```

## Production Configuration

Production must provide the database connection string and JWT settings through environment variables or a secret manager. The API refuses to start in Production if required values are missing or development seed access is enabled. See [docs/deployment.md](docs/deployment.md).

Production HTTPS should terminate at the reverse proxy. The API trusts forwarded headers in Production, applies HSTS, and redirects HTTP to HTTPS when enabled.

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
