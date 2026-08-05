# Unify ERP - API Design

> **Important note:** This is the foundation API design. A complete enterprise API specification (every endpoint, DTO, schema, version, error, example request/response, OpenAPI detail, pagination model, and webhook/event contract) would span hundreds of pages and cannot fit into one generated file.

# Part 1 - API Principles
- RESTful HTTPS API
- ASP.NET Core Web API
- JSON
- Versioned (`/api/v1`)
- Stateless
- JWT protected
- Problem Details errors
- Idempotent where required

# Part 2 - Authentication
Endpoints:
- POST /api/v1/auth/login
- POST /api/v1/auth/refresh
- POST /api/v1/auth/logout
- POST /api/v1/auth/logout-all
- GET /api/v1/auth/sessions
- GET /api/v1/auth/me

JWT + rotating refresh tokens.

# Part 2.1 - System
Endpoints:
- GET /api/v1/system/health
- GET /health/live
- GET /health/ready

# Part 3 - Core Modules
Endpoints grouped by:
- GET /api/v1/platform/organisations
- POST /api/v1/platform/organisations
- GET /api/v1/platform/organisations/{organisationId}/branches
- POST /api/v1/platform/branches
- GET /api/v1/platform/organisations/{organisationId}/warehouses
- POST /api/v1/platform/warehouses
- users
- roles
- permissions
- devices
- settings

# Part 4 - ERP Modules
Resources:
- GET /api/v1/customers?organisationId={organisationId}&branchId={branchId}&search={search}
- POST /api/v1/customers
- GET /api/v1/customers/{customerId}?organisationId={organisationId}
- POST /api/v1/customers/{customerId}/deactivate?organisationId={organisationId}
- GET /api/v1/suppliers?organisationId={organisationId}&search={search}
- POST /api/v1/suppliers
- GET /api/v1/suppliers/{supplierId}?organisationId={organisationId}
- POST /api/v1/suppliers/{supplierId}/deactivate?organisationId={organisationId}
- GET /api/v1/products/units?organisationId={organisationId}
- POST /api/v1/products/units
- GET /api/v1/products/categories?organisationId={organisationId}
- POST /api/v1/products/categories
- GET /api/v1/products?organisationId={organisationId}&categoryId={categoryId}&search={search}
- POST /api/v1/products
- GET /api/v1/products/{productId}?organisationId={organisationId}
- POST /api/v1/products/{productId}/deactivate?organisationId={organisationId}
- price-lists
- quotations
- POST /api/v1/sales
- GET /api/v1/sales?organisationId={organisationId}&customerId={customerId}
- GET /api/v1/sales/{saleId}?organisationId={organisationId}
- POST /api/v1/payments/customers
- GET /api/v1/payments/customers/{customerId}/balance?organisationId={organisationId}
- GET /api/v1/payments/customers/{customerId}/ledger?organisationId={organisationId}
- POST /api/v1/purchasing/orders
- GET /api/v1/purchasing/orders?organisationId={organisationId}&supplierId={supplierId}
- POST /api/v1/purchasing/goods-receipts
- POST /api/v1/purchasing/supplier-invoices
- POST /api/v1/inventory/adjustments
- POST /api/v1/inventory/transfers
- GET /api/v1/inventory/balances?organisationId={organisationId}&warehouseId={warehouseId}
- GET /api/v1/inventory/movements?organisationId={organisationId}&warehouseId={warehouseId}&productId={productId}
- expenses
- POST /api/v1/accounting/accounts
- GET /api/v1/accounting/accounts?organisationId={organisationId}
- POST /api/v1/accounting/fiscal-periods
- GET /api/v1/accounting/fiscal-periods?organisationId={organisationId}
- POST /api/v1/accounting/journals
- reports
- deliveries

Standard CRUD plus domain-specific actions.

# Part 5 - API Standards
- Pagination
- Filtering
- Sorting
- Searching
- Validation
- Correlation ID
- UTC timestamps
- UUID identifiers

List endpoints return a paged response envelope:
- items
- pageNumber
- pageSize
- totalCount
- totalPages

Validation failures use RFC7807 validation Problem Details with:
- code
- correlationId
- errors

Every API response includes `X-Correlation-ID`. Clients may provide this header to trace a request across logs.

# Part 6 - Security
- HTTPS only
- Authorization policies
- Tenant validation
- Branch validation
- Rate limiting
- Request size limits

Implemented module permission policies use JWT `permission` claims. Each protected module group requires its matching permission claim.

Implemented global rate limiting uses a fixed window limiter partitioned by authenticated user name when present, otherwise by remote IP. Defaults are 120 requests per 60 seconds and can be changed with `RateLimiting__PermitLimit` and `RateLimiting__WindowSeconds`.

Platform audit endpoint:
- `GET /api/v1/platform/audit-entries`
- Requires `platform.manage`
- Supports `organisationId`, `entityName`, `entityId`, `fromUtc`, `toUtc`, `pageNumber`, and `pageSize`
- Returns paged audit entries for operational traceability

# Part 7 - Reporting
Endpoints generate:
- Excel
- PDF
- CSV where appropriate

# Part 8 - Sync
- POST /sync/push
- POST /sync/pull
- GET /sync/status
- POST /sync/retry

# Part 9 - Errors
Use RFC7807 Problem Details with stable machine-readable error codes.

# Part 10 - Future
Future document will define every endpoint, DTO, request/response model, OpenAPI examples, status codes, versioning, and deprecation policy.
