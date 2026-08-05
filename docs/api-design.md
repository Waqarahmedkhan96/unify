# Unify ERP — API Design

> **Important note:** This is the foundation API design. A complete enterprise API specification (every endpoint, DTO, schema, version, error, example request/response, OpenAPI detail, pagination model, and webhook/event contract) would span hundreds of pages and cannot fit into one AI response.

# Part 1 – API Principles
- RESTful HTTPS API
- ASP.NET Core Web API
- JSON
- Versioned (`/api/v1`)
- Stateless
- JWT protected
- Problem Details errors
- Idempotent where required

# Part 2 – Authentication
Endpoints:
- POST /auth/login
- POST /auth/refresh
- POST /auth/logout
- POST /auth/logout-all
- GET /auth/sessions

JWT + rotating refresh tokens.

# Part 3 – Core Modules
Endpoints grouped by:
- organisations
- branches
- warehouses
- users
- roles
- permissions
- devices
- settings

# Part 4 – ERP Modules
Resources:
- customers
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

# Part 5 – API Standards
- Pagination
- Filtering
- Sorting
- Searching
- Validation
- Correlation ID
- UTC timestamps
- UUID identifiers

# Part 6 – Security
- HTTPS only
- Authorization policies
- Tenant validation
- Branch validation
- Rate limiting
- Request size limits

# Part 7 – Reporting
Endpoints generate:
- Excel
- PDF
- CSV where appropriate

# Part 8 – Sync
- POST /sync/push
- POST /sync/pull
- GET /sync/status
- POST /sync/retry

# Part 9 – Errors
Use RFC7807 Problem Details with stable machine-readable error codes.

# Part 10 – Future
Future document will define every endpoint, DTO, request/response model, OpenAPI examples, status codes, versioning, and deprecation policy.
