# Unify ERP — Security

> Foundation security specification.

## Identity
- ASP.NET Core Identity
- JWT access tokens
- Rotating refresh tokens
- Device registration
- Session revocation

## Authorization
- Policy/permission based.
- Tenant validation.
- Branch validation.

## API
- HTTPS only
- Rate limiting
- Validation
- Problem Details
- Correlation IDs

## Data
- Decimal for money
- UTC timestamps
- UUID identifiers
- Secure platform storage for tokens
- No secrets in source control

## Logging
Never log:
- Passwords
- Access tokens
- Refresh tokens
- Secrets

## Audit
Audit:
- Logins
- Permission changes
- Financial changes
- Inventory adjustments
- Sync conflicts

Implemented audit trail:
- Domain entity create, update, and delete operations are persisted to `audit_entries`.
- Audit rows include request user metadata when available.
- Audit rows include the request correlation id for log-to-data tracing.
- Platform operators can read audit entries through the protected Platform API.

## Offline
- Device stock allocation
- Idempotent sync
- Conflict handling
- Signed-in device validation

## Future
Pen testing, dependency scanning, vulnerability management, encryption review.
## Implemented Authorization Foundation

Authenticated module APIs require permission claims in JWT access tokens.

Current permission names:
- `platform.manage`
- `customers.manage`
- `suppliers.manage`
- `products.manage`
- `inventory.manage`
- `sales.manage`
- `payments.manage`
- `purchasing.manage`
- `accounting.manage`

The development seed user receives all current permissions for local testing. Production deployments must assign permissions through role administration rather than broad seed access.
