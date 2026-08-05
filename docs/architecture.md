# Unify ERP — Architecture

> **Product:** Unify  
> **Tagline:** A cross-platform ERP solution focused on performance, scalability, and offline-first capabilities.

> **Note:** This is the **foundation architecture document** for Unify ERP. It defines the permanent architectural direction, major design decisions, technology choices, module boundaries, and implementation principles. It is **not** the final 70–100 page enterprise architecture reference. That larger document must be developed incrementally because it exceeds the response/output limits of current AI systems.

---

# 1. Purpose

This document defines the architectural foundation for Unify ERP.

Its objectives are to:

- Keep all developers following one architecture.
- Provide a reference for Codex.
- Explain why each major technology was selected.
- Prevent architectural drift.
- Keep modules loosely coupled.
- Support long-term maintainability.

---

# 2. Architectural Goals

Unify must be:

- Cross-platform
- Offline-first
- Secure by default
- Modular
- Scalable
- Testable
- Maintainable
- SaaS-ready
- Suitable for Android and Windows
- Able to evolve without major rewrites

---

# 3. High-Level Architecture

```text
Flutter (Android)
        │
Flutter (Windows)
        │
──────── HTTPS ────────
        │
ASP.NET Core Web API
        │
Business Layer
        │
Entity Framework Core
        │
PostgreSQL
```

Each Flutter application contains:

- SQLite
- Drift
- Offline Outbox
- Sync Engine
- Secure Storage

The server contains:

- Authentication
- Authorization
- Business Rules
- Reporting
- Accounting
- Synchronization
- Audit Logging

---

# 4. Architecture Style

## Modular Monolith

The first production version uses a Modular Monolith.

Reasons:

- Easier deployment
- Easier debugging
- Lower hosting cost
- Simpler transactions
- Easier testing
- Better for a small development team

Modules remain isolated internally so they can be extracted later if necessary.

---

# 5. Technology Stack

## Frontend

- Flutter
- Dart
- Riverpod
- GoRouter
- Drift
- SQLite
- Dio
- Material 3

## Backend

- ASP.NET Core Web API
- C#
- Entity Framework Core
- PostgreSQL
- ASP.NET Core Identity
- JWT
- Refresh Tokens
- FluentValidation
- Serilog

## Infrastructure

- Docker
- Docker Compose
- GitHub Actions
- GitHub
- CodeQL
- Dependabot

---

# 6. Repository Layout

```text
apps/
    unify_app/

backend/
    src/
    tests/

modules/
    industry/
        lpg/

docs/

deploy/

.github/
```

---

# 7. Clean Architecture

Backend layers:

```text
API
 ↓
Application
 ↓
Domain
 ↑
Infrastructure
```

Rules:

- Domain knows nothing about API.
- Domain knows nothing about Infrastructure.
- Controllers contain no business logic.
- UI never talks directly to PostgreSQL.

---

# 8. Flutter Architecture

Feature-first structure.

```text
features/
    auth/
    dashboard/
    customers/
    suppliers/
    products/
    sales/
    inventory/
    accounting/
    reports/
```

Every feature contains:

- presentation
- domain
- data

---

# 9. Offline-First Strategy

Each device stores local data in SQLite.

Workflow:

1. User performs action.
2. Save locally.
3. Add Outbox entry.
4. UI updates immediately.
5. Sync when internet returns.

Financial transactions are append-only.

---

# 10. Synchronization

Push:

```text
Client
 ↓
Outbox
 ↓
API
```

Pull:

```text
API
 ↓
Changed Records
 ↓
SQLite
```

Principles:

- Idempotent
- Ordered
- Retryable
- Incremental
- Conflict-aware

---

# 11. Multi-Tenancy

Every business record belongs to one Organisation.

Most operational records also belong to:

- Branch
- Warehouse (where applicable)

The server validates tenant ownership for every request.

---

# 12. Authentication

- ASP.NET Core Identity
- JWT Access Token
- Rotating Refresh Token
- Secure Storage
- Device Registration
- Session Management

---

# 13. Authorization

Permission-based authorization.

Examples:

- customers.view
- customers.create
- sales.create
- inventory.adjust
- accounting.post

Never trust client permissions.

---

# 14. Modules

Core:

- Organisations
- Branches
- Warehouses
- Users
- Roles
- Permissions
- Devices
- Audit
- Sync

ERP:

- CRM
- Sales
- Purchasing
- Inventory
- Accounting
- Cash & Bank
- Expenses
- Reports

Extensions:

- LPG
- Retail
- Pharmacy
- Manufacturing (future)

---

# 15. Reporting

Official:

Backend-generated:

- Excel
- PDF

Offline:

Generated from SQLite.

Clearly marked as:

```
PROVISIONAL OFFLINE REPORT
```

---

# 16. Security

- HTTPS
- JWT
- Refresh Token Rotation
- Rate Limiting
- Structured Logging
- Input Validation
- Secure Storage
- Audit Trail
- Tenant Isolation

---

# 17. Performance

Targets:

- Fast local UI
- Efficient SQL
- Indexed PostgreSQL
- Pagination
- Background report generation
- Async APIs

---

# 18. Testing

Backend:

- Unit
- Integration
- Architecture

Flutter:

- Unit
- Widget
- Integration

Critical flows:

- Login
- Sales
- Payments
- Sync
- Accounting
- Inventory

---

# 19. DevOps

- Docker
- Docker Compose
- GitHub Actions
- CodeQL
- Dependabot

CI must:

- Build
- Test
- Analyze
- Package

---

# 20. Architecture Decision Records

ADR-001: Flutter for Android and Windows.

ADR-002: ASP.NET Core Web API backend.

ADR-003: PostgreSQL as central database.

ADR-004: Drift + SQLite for offline.

ADR-005: Modular Monolith.

ADR-006: Offline-first synchronization.

ADR-007: JWT + Rotating Refresh Tokens.

ADR-008: Device Stock Allocation for offline inventory.

---

# 21. Guiding Principles

1. Security before convenience.
2. Data integrity before speed.
3. Offline capability without compromising correctness.
4. Financial records are immutable after posting.
5. Audit everything important.
6. Business rules belong on the server.
7. One source of truth.
8. Modular design.
9. Test before release.
10. Document architectural decisions.

---

# 22. Future Evolution

Future versions may introduce:

- SaaS subscriptions
- Plugin marketplace
- Multi-region deployment
- BI dashboards
- AI forecasting
- Event streaming
- Manufacturing
- Public APIs

These enhancements must not break the core architecture defined in this document.
