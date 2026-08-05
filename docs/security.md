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

## Offline
- Device stock allocation
- Idempotent sync
- Conflict handling
- Signed-in device validation

## Future
Pen testing, dependency scanning, vulnerability management, encryption review.
