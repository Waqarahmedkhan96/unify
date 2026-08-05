# Unify ERP — Security

> Foundation security specification.

## Identity
- ASP.NET Core Identity
- JWT access tokens
- Rotating refresh tokens
- Device registration
- Session revocation
- Password change
- Password reset through signed, expiring Identity tokens
- First-admin bootstrap without public signup

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

Implemented rate limiting:
- Global fixed-window limiting is enabled by default.
- Authenticated users are partitioned independently.
- Anonymous clients are partitioned by remote IP address.
- Rejected requests return HTTP 429.

Implemented HTTPS production behavior:
- Production trusts reverse proxy forwarded headers.
- Production uses HSTS.
- Production redirects HTTP requests to HTTPS when `Https__RequireHttps=true`.
- TLS certificates are terminated at the reverse proxy.

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

Development may log password reset tokens only for local testing when SMTP is not configured. Production startup requires SMTP reset delivery settings.

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

## Implemented Account Recovery

Password endpoints:
- `POST /api/v1/auth/forgot-password`
- `POST /api/v1/auth/reset-password`
- `POST /api/v1/auth/change-password`

Forgot-password requests always return an accepted response after validation so attackers cannot discover registered emails. Successful reset and password change operations revoke existing refresh-token sessions.

## Implemented First Admin Bootstrap

There is no public signup endpoint. Production can create the first platform admin through `BootstrapAdmin` configuration only when the user table is empty. The bootstrap user receives all current module permissions. Keep `BootstrapAdmin__Enabled=false` after first deployment.
