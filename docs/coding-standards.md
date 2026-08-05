# Unify ERP — Coding Standards

> Foundation coding standards. A complete enterprise engineering handbook would be significantly larger.

## Principles
- Readability over cleverness.
- Security before convenience.
- Consistency over personal preference.
- Test business rules.
- Document architectural decisions.

## General
- Follow SOLID where appropriate.
- Small focused classes and methods.
- Meaningful names.
- Nullable reference types enabled.
- Async I/O with CancellationToken.

## Backend
- Thin controllers.
- Business logic in Application/Domain.
- EF Core via DI.
- FluentValidation.
- Serilog.
- DTOs only across API boundary.
- Never expose EF entities.

## Flutter
- Feature-first folders.
- Riverpod only.
- GoRouter only.
- Drift as local source of truth.
- Material 3.
- Responsive Android/Windows.

## Git
- Feature branches.
- Conventional commits.
- PR reviews.
- No secrets committed.

## Testing
- Unit tests for business rules.
- Integration tests for API.
- Widget tests for critical UI.
- Sync and accounting scenarios are mandatory.
