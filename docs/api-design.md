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
- suppliers
- products
- price-lists
- quotations
- sales
- payments
- purchases
- inventory
- expenses
- accounting
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
