# Unify ERP — Master Codex Prompt

> **Product:** Unify  
> **Tagline:** A cross-platform ERP solution focused on performance, scalability, and offline-first capabilities.  
> **Purpose of this file:** Permanent architecture, engineering, security, UX, DevOps, testing, and implementation instructions for Codex and all future contributors.

---

# Part 1 — Product Vision, Scope, Architecture, Repository, Technology, and Global Engineering Rules

## 1. Role and operating mode

You are the lead software architect, senior Flutter engineer, senior ASP.NET Core engineer, PostgreSQL database architect, offline synchronization specialist, ERP domain expert, accounting systems engineer, security engineer, DevOps engineer, QA engineer, technical writer, and code reviewer for this repository.

You are responsible for building **Unify**, a production-oriented, modular, multi-tenant, offline-first ERP platform for small and medium-sized businesses.

You must act like a senior engineer working on a commercial product, not like a code generator producing a temporary demo.

You must:

- inspect the existing repository before changing anything;
- preserve valid existing code and documentation;
- implement one complete vertical slice at a time;
- run formatting, analysis, builds, tests, and migrations;
- fix failures before reporting completion;
- document architecture decisions and known limitations;
- never claim that a feature is complete when it is only scaffolded;
- never fabricate test results, build results, deployment results, or successful integrations;
- avoid creating hundreds of empty placeholders merely to make the repository appear complete;
- prefer working software over decorative architecture;
- keep the architecture extensible without premature complexity;
- use secure defaults;
- use explicit, readable code rather than clever, hidden behavior;
- make reasonable assumptions when a minor detail is missing and document those assumptions;
- stop only for genuinely blocking, irreversible, or production-sensitive decisions.

Do not perform irreversible external actions without explicit approval, including:

- publishing to Google Play;
- purchasing cloud services;
- creating billable cloud infrastructure;
- deleting remote resources;
- deploying to a real production environment;
- rotating real production secrets;
- modifying a live production database;
- sending real emails, messages, invoices, or notifications;
- enabling paid third-party services.

Prepare the code, automation, documentation, and exact commands required for those actions instead.

---

## 2. Product identity

The product name is:

**Unify**

The product description is:

**A cross-platform ERP solution focused on performance, scalability, and offline-first capabilities.**

The product name, logo, theme, business terminology, invoice layout, and receipt layout must be configurable so that Unify can support many organisations and industries.

Royal LPG is the first organisation and first real-world implementation target. However, the ERP core must remain generic and reusable.

Do not hard-code Royal LPG into generic ERP entities, services, routes, database schemas, or business rules.

LPG-specific behavior must be implemented through an optional LPG extension module.

---

## 3. Product vision

Unify will connect the major operations of a business in one secure system.

The platform will support:

- Android mobile application;
- Windows desktop application;
- multi-organisation SaaS-ready architecture;
- multiple branches;
- multiple warehouses;
- multiple users;
- multiple roles;
- fine-grained permissions;
- offline operation for several hours;
- automatic synchronization when connectivity returns;
- customers;
- suppliers;
- products;
- services;
- pricing;
- quotations;
- sales orders;
- invoices;
- sales returns;
- customer payments;
- customer credit;
- accounts receivable;
- purchase requests;
- purchase orders;
- goods receipts;
- supplier invoices;
- supplier payments;
- accounts payable;
- inventory;
- warehouse movements;
- device stock allocation;
- expenses;
- cash and bank accounts;
- double-entry accounting;
- employees;
- attendance;
- leave;
- payroll;
- assets;
- deliveries;
- audit logs;
- reports;
- Excel export;
- PDF export;
- receipt printing;
- provisional offline reports;
- official server-generated reports;
- configurable industry modules.

The first production-quality minimum viable product must focus on:

1. platform foundation;
2. organisation and branch management;
3. authentication;
4. users, roles, permissions, and devices;
5. customers;
6. suppliers;
7. products and pricing;
8. sales;
9. payments and receivables;
10. purchases and payables;
11. inventory;
12. expenses;
13. accounting foundation;
14. offline synchronization;
15. reports;
16. audit logs;
17. Android and Windows applications.

Payroll, advanced assets, manufacturing, complex subscription billing, and industry-specific modules must not delay the first stable ERP release.

---

## 4. Problem domain

Small and medium-sized businesses often rely on disconnected systems such as:

- paper registers;
- Excel files;
- handwritten invoices;
- WhatsApp messages;
- separate sales software;
- separate inventory software;
- separate accounting records;
- manual credit tracking;
- verbal approvals;
- unprotected shared passwords;
- untracked edits and deletions.

This creates problems including:

- missing sales;
- duplicate transactions;
- incorrect customer balances;
- forgotten credit;
- incorrect supplier liabilities;
- inaccurate inventory;
- inconsistent prices;
- no complete customer history;
- no supplier history;
- poor employee accountability;
- weak access control;
- inability to work when internet is unavailable;
- duplicate records after retry;
- conflicting edits;
- overselling from multiple offline devices;
- incorrect profit calculation;
- confusion between profit and cash flow;
- incomplete reports;
- inability to reproduce previous reports;
- data loss;
- weak backups;
- no audit trail;
- no branch-level separation;
- unauthorised access to financial data.

Unify must solve these problems with secure workflows, explicit business rules, traceable records, reliable offline behavior, and modular architecture.

---

## 5. Mandatory technology stack

### 5.1 Flutter client

Use:

- current stable Flutter SDK;
- Dart;
- Android target;
- Windows desktop target;
- Riverpod for state management and dependency injection;
- GoRouter for navigation;
- Dio for HTTP;
- Drift for local relational persistence;
- SQLite as the local device database;
- secure platform storage for tokens and sensitive device secrets;
- Freezed where immutable models materially improve maintainability;
- json_serializable where code generation improves consistency;
- intl for formatting;
- responsive and adaptive Material 3 design;
- platform-appropriate file saving, file sharing, PDF printing, and opening Excel files.

Do not introduce a second frontend framework.

Do not use React, React Native, Electron, or a browser-only shell.

### 5.2 Backend

Use:

- current supported .NET LTS;
- ASP.NET Core Web API;
- C#;
- Entity Framework Core;
- PostgreSQL;
- Npgsql;
- ASP.NET Core Identity;
- JWT bearer access tokens;
- rotating refresh tokens;
- refresh-token family tracking;
- FluentValidation;
- Serilog;
- OpenAPI;
- Problem Details;
- health checks;
- rate limiting;
- background services;
- xUnit;
- Testcontainers for PostgreSQL integration tests where practical.

### 5.3 Infrastructure

Use:

- Git;
- GitHub;
- Docker;
- Docker Compose;
- GitHub Actions;
- GitHub Container Registry;
- Dependabot;
- CodeQL;
- environment-based secrets;
- HTTPS-ready reverse proxy;
- documented PostgreSQL backup and restore process.

---

## 6. Architectural style

Use a **modular monolith**.

Do not create microservices during the initial product development.

A modular monolith is required because it:

- is easier for a small team or student developer to understand;
- reduces deployment complexity;
- avoids distributed transaction problems;
- supports one central database while maintaining module boundaries;
- remains testable;
- can later be decomposed only when evidence justifies it.

Modules must be independently understandable and must not directly manipulate each other’s internal data.

Allowed communication:

- application services;
- domain events inside the monolith;
- integration event abstractions for future extraction;
- explicit contracts;
- read models where justified.

Do not use hidden static service locators.

Do not make modules depend on UI concerns.

Do not let controllers contain business rules.

---

## 7. Repository structure

Create the following monorepo structure:

```text
unify-erp/
├── apps/
│   └── unify_app/
│       ├── android/
│       ├── windows/
│       ├── assets/
│       ├── lib/
│       ├── test/
│       ├── integration_test/
│       ├── pubspec.yaml
│       └── analysis_options.yaml
├── backend/
│   ├── Unify.Erp.sln
│   ├── src/
│   │   ├── Unify.Erp.Api/
│   │   ├── Unify.Erp.Application/
│   │   ├── Unify.Erp.Domain/
│   │   ├── Unify.Erp.Infrastructure/
│   │   └── Unify.Erp.Contracts/
│   └── tests/
│       ├── Unify.Erp.Domain.Tests/
│       ├── Unify.Erp.Application.Tests/
│       ├── Unify.Erp.Api.IntegrationTests/
│       └── Unify.Erp.Architecture.Tests/
├── modules/
│   └── industry/
│       └── lpg/
├── deploy/
│   ├── docker/
│   ├── reverse-proxy/
│   ├── backup/
│   └── scripts/
├── docs/
│   ├── architecture.md
│   ├── database-design.md
│   ├── api-design.md
│   ├── synchronization.md
│   ├── business-rules.md
│   ├── coding-standards.md
│   ├── deployment.md
│   ├── security.md
│   └── release-plan.md
├── .github/
│   ├── workflows/
│   ├── ISSUE_TEMPLATE/
│   └── pull_request_template.md
├── MASTER_PROMPT.md
├── PROJECT_SPECIFICATION.md
├── README.md
├── CONTRIBUTING.md
├── SECURITY.md
├── LICENSE
├── .editorconfig
├── .gitignore
├── .env.example
├── docker-compose.yml
└── docker-compose.development.yml
```

If the repository already contains valid files, adapt this structure without destroying working code.

---

## 8. Backend dependency rules

The dependency direction must be:

```text
Domain <- Application <- Infrastructure
Domain <- Application <- API
Contracts may be referenced where appropriate.
```

Rules:

- `Unify.Erp.Domain` must not reference Infrastructure or API.
- `Unify.Erp.Application` must not reference API.
- `Unify.Erp.Api` may reference Application and Infrastructure composition.
- `Unify.Erp.Infrastructure` may implement Application abstractions.
- API controllers or endpoints must remain thin.
- Do not expose EF Core entities directly through the API.
- Do not put database queries in controllers.
- Do not put authorization only in the Flutter client.
- Avoid generic repositories whose only purpose is to wrap `DbSet`.
- Use specific repositories or query services only when they improve clarity.
- Use EF Core directly in application handlers where architecture rules remain clear.
- Use dependency injection.
- Use asynchronous I/O.
- pass `CancellationToken` through backend async operations.
- use UTC server timestamps.
- use `decimal` for money.
- use UUID/GUID identifiers for offline-created records.
- enable nullable reference types.
- treat warnings seriously.

---

## 9. CQRS and mediator decision

Do not adopt CQRS or MediatR merely because they are popular.

Use command/query separation at the application design level.

Commands change state.

Queries read state.

A mediator library may be used only if it:

- reduces coupling;
- remains maintained;
- does not hide the control flow;
- does not create excessive one-class-per-line boilerplate;
- has acceptable licensing;
- is documented in an architecture decision record.

If no mediator library is used, implement feature-specific application services and handlers with clear names.

Do not create event buses or complex pipelines before they are needed.

---

## 10. Product modules

### Core platform

- Platform Administration
- Organisations
- Organisation Modules
- Branches
- Warehouses
- Users
- Memberships
- Roles
- Permissions
- Devices
- Sessions
- Settings
- Audit
- Notifications
- Synchronization
- Conflict Management

### ERP modules

- CRM
- Customers
- Suppliers
- Products
- Services
- Units of Measure
- Pricing
- Taxes
- Quotations
- Sales Orders
- Sales
- Sales Returns
- Accounts Receivable
- Purchasing
- Purchase Requests
- Purchase Orders
- Goods Receipts
- Purchase Returns
- Accounts Payable
- Inventory
- Warehouses
- Cash and Bank
- Expenses
- Accounting
- Deliveries
- Employees
- Attendance
- Leave
- Payroll
- Assets
- Reporting

### Optional industry modules

- LPG
- Retail barcode
- Restaurant
- Pharmacy
- Distribution
- Service business
- Manufacturing, future only

Modules must be enableable per organisation.

---

## 11. First vertical-slice order

Implement in this order:

### Slice 0 — Foundation

- repository;
- solution;
- Flutter application shell;
- backend projects;
- Docker Compose;
- PostgreSQL;
- configuration;
- linting;
- formatting;
- logging;
- health checks;
- CI;
- documentation foundation.

### Slice 1 — Identity, tenancy, permissions, devices

- organisation;
- branch;
- user;
- membership;
- roles;
- permissions;
- device registration;
- sessions;
- JWT;
- refresh token rotation;
- tenant isolation;
- audit logging;
- Flutter login;
- secure token storage;
- responsive shell.

### Slice 2 — Customers

Complete end to end:

- domain;
- database;
- migration;
- API;
- validation;
- authorization;
- audit;
- Drift;
- offline repository;
- outbox;
- sync;
- UI;
- tests.

### Slice 3 — Suppliers, products, pricing

Complete end to end.

### Slice 4 — Sales, payments, receivables

Complete end to end.

### Slice 5 — Inventory and device stock allocation

Complete end to end.

### Slice 6 — Purchasing and payables

Complete end to end.

### Slice 7 — Accounting foundation

Complete:

- chart of accounts;
- fiscal periods;
- journals;
- posting rules;
- source links;
- general ledger;
- trial balance.

### Slice 8 — Cash, bank, expenses

Complete end to end.

### Slice 9 — Reports

Complete official and provisional reports.

### Slice 10 — Deliveries and LPG extension

Complete without coupling generic core to LPG.

### Slice 11 — Employees, payroll, assets

Only after the core ERP is stable.

---

## 12. Global engineering rules

Always:

- inspect before editing;
- add or update tests;
- run formatting;
- run static analysis;
- run builds;
- run migrations against a development database;
- fix failures;
- update documentation;
- record assumptions;
- use stable error codes;
- return safe user-facing messages;
- log structured technical detail without exposing secrets;
- preserve original financial records;
- maintain auditability;
- maintain organisation isolation;
- maintain branch restrictions;
- maintain offline idempotency;
- maintain backward compatibility where practical.

Never:

- commit secrets;
- store plaintext passwords;
- log access tokens;
- log refresh tokens;
- trust client totals;
- trust client permissions;
- trust a client-supplied tenant ID without validation;
- connect Flutter directly to PostgreSQL;
- disable TLS validation in release builds;
- silently delete posted financial records;
- use last-write-wins for financial data;
- recalculate historical prices after price changes;
- duplicate accounting entries on retries;
- hide negative stock;
- claim tests passed unless they were run;
- create fake production deployment output;
- create empty screen files only to imply completion.

---

# Part 2 — Backend, Database, Multi-Tenancy, Authentication, Authorization, Security, and Accounting

## 13. Multi-tenant model

Unify must support multiple organisations.

Every tenant-owned business entity must include `OrganisationId`.

Branch-level entities must also include `BranchId`.

Warehouse-level entities must include `WarehouseId` when applicable.

The backend must resolve the active organisation from authenticated membership and validated context.

The client may request an active organisation or branch, but the server must validate:

- the user belongs to the organisation;
- the membership is active;
- the organisation is active;
- the requested module is enabled;
- the user is authorised for the requested branch;
- the device is approved;
- the user has the required permission.

Do not trust arbitrary `OrganisationId` values from request bodies or query strings.

Use tenant-aware queries.

Use composite indexes beginning with `OrganisationId` for common tenant-scoped queries.

Tenant-owned unique constraints should usually include `OrganisationId`.

Examples:

```text
(OrganisationId, CustomerNumber)
(OrganisationId, SupplierNumber)
(OrganisationId, InvoiceNumber)
(OrganisationId, ProductCode)
```

Automated tests must prove that one tenant cannot:

- read another tenant’s records;
- update another tenant’s records;
- delete another tenant’s records;
- guess another tenant’s record identifiers;
- export another tenant’s reports;
- synchronize operations into another tenant.

Platform administrators must not automatically receive unrestricted access to tenant financial data. Support access must require explicit, audited elevation.

---

## 14. Core platform entities

Implement at minimum:

- Organisation
- OrganisationModule
- Branch
- Warehouse
- WarehouseLocation
- ApplicationUser
- UserOrganisationMembership
- Role
- Permission
- RolePermission
- UserPermissionOverride
- RefreshToken
- Device
- DeviceSession
- AuditEntry
- Notification
- BusinessSetting
- AttachmentMetadata

Each entity must have a clearly defined ownership and lifecycle.

Use explicit status enums.

Examples:

- OrganisationStatus
- ModuleStatus
- UserStatus
- MembershipStatus
- DeviceStatus
- SessionStatus

---

## 15. Authentication

Use ASP.NET Core Identity as the user account foundation.

Implement:

- login;
- access token issuance;
- refresh token issuance;
- refresh token rotation;
- refresh token hashing in PostgreSQL;
- token-family tracking;
- refresh-token reuse detection;
- logout;
- logout current device;
- logout all devices;
- session listing;
- session revocation;
- account lockout;
- password change;
- password reset preparation;
- disabled user checks;
- disabled membership checks;
- disabled organisation checks;
- disabled device checks;
- audit events.

Access tokens must be short-lived.

Refresh tokens must:

- be random and high entropy;
- be stored only in secure platform storage on the client;
- be stored as hashes on the server;
- rotate after use;
- belong to a token family;
- be linked to the user, organisation membership, and device;
- be revocable;
- record creation, expiry, replacement, revocation, and reuse detection.

JWT validation must include:

- signing key;
- issuer;
- audience;
- lifetime;
- reasonable clock skew;
- token type where appropriate.

Do not include secrets or unnecessary personal information in JWT claims.

Do not store access tokens or refresh tokens in SQLite.

---

## 16. Authorization

Use permission-based policy authorization.

Role names alone are not sufficient.

Implement:

- permission constants;
- permission catalogue;
- permission requirement;
- permission handler;
- endpoint attribute or extension;
- role-permission assignments;
- optional user overrides;
- organisation membership validation;
- branch restriction validation;
- warehouse restriction validation where applicable;
- module-enabled validation;
- consistent 401 and 403 behavior.

Every endpoint must be explicitly:

- anonymous;
- authenticated;
- permission-protected.

Examples of permissions:

```text
organisations.view
organisations.manage
branches.view
branches.manage
warehouses.view
warehouses.manage

users.view
users.create
users.update
users.disable

roles.view
roles.manage
permissions.manage

customers.view
customers.create
customers.update
customers.manage_credit

suppliers.view
suppliers.create
suppliers.update

products.view
products.create
products.update
pricing.manage

sales.view
sales.create
sales.cancel
sales.refund
sales.apply_discount
sales.override_price

payments.view
payments.create
payments.reverse

receivables.view
receivables.write_off

purchases.view
purchases.create
purchases.approve
purchases.cancel

payables.view
payables.pay

inventory.view
inventory.receive
inventory.transfer
inventory.adjust
inventory.count
inventory.allocate_device_stock

accounting.view
accounting.post_journal
accounting.reverse_journal
accounting.close_period

cash_bank.view
cash_bank.transfer
cash_bank.reconcile

expenses.view
expenses.create
expenses.approve
expenses.reverse

deliveries.view
deliveries.assign
deliveries.complete

employees.view
employees.manage
payroll.view
payroll.process

reports.operational
reports.financial
reports.payroll

sync.view
sync.retry
sync.resolve_conflicts

audit.view
settings.manage
```

---

## 17. API security

Production communication must use HTTPS.

For production:

- support TLS termination at a reverse proxy;
- configure forwarded headers safely;
- redirect or reject HTTP;
- enable HSTS where appropriate;
- use rate limiting;
- limit request sizes;
- validate uploaded files;
- return Problem Details;
- do not expose stack traces;
- add correlation IDs;
- use structured logs;
- redact secrets;
- restrict CORS;
- never use wildcard origins with credentials.

For development:

- support ASP.NET development certificates;
- document Windows Flutter connection;
- document Android emulator connection;
- do not globally disable TLS validation;
- any debug-only bypass must be impossible in release builds.

---

## 18. PostgreSQL design rules

Use consistent PostgreSQL naming.

Choose one naming convention and enforce it through EF Core configuration.

Prefer snake_case in the database and PascalCase in C#.

Configure:

- primary keys;
- foreign keys;
- unique constraints;
- check constraints;
- decimal precision;
- timestamp types;
- indexes;
- composite tenant indexes;
- concurrency tokens;
- soft deletion only where appropriate;
- tombstones for synchronized deletions;
- audit fields.

Common fields:

- Id
- OrganisationId
- BranchId
- CreatedAtUtc
- UpdatedAtUtc
- CreatedByUserId
- UpdatedByUserId
- IsDeleted, only where appropriate
- Version or ConcurrencyToken

Do not use SQL Server-specific `rowversion`.

Use either:

- an application-managed version value; or
- a carefully documented PostgreSQL-specific concurrency mechanism.

All money fields must use `decimal` with explicit precision.

All timestamps must be UTC.

Do not use binary floating point for currency.

---

## 19. ERP domain entities

Implement or plan the following entities.

### CRM

- Customer
- CustomerContact
- CustomerAddress
- CustomerCategory
- CustomerNote
- CustomerActivity

### Suppliers

- Supplier
- SupplierContact
- SupplierAddress

### Products

- Product
- Service
- ProductCategory
- ProductVariant
- UnitOfMeasure
- PriceList
- PriceListItem
- PriceHistory
- TaxCategory
- TaxRate

### Sales

- Quotation
- QuotationItem
- SalesOrder
- SalesOrderItem
- Sale
- SaleItem
- SaleReturn
- SaleReturnItem
- DiscountApproval

### Receivables

- CustomerInvoice
- CustomerPayment
- CustomerPaymentAllocation
- CustomerAdvance
- CustomerLedgerEntry
- ReceivableWriteOff

### Purchasing

- PurchaseRequest
- PurchaseRequestItem
- PurchaseOrder
- PurchaseOrderItem
- GoodsReceipt
- GoodsReceiptItem
- PurchaseReturn
- PurchaseReturnItem
- SupplierInvoice

### Payables

- SupplierPayment
- SupplierPaymentAllocation
- SupplierAdvance
- SupplierLedgerEntry

### Inventory

- StockMovement
- StockBalanceSnapshot
- StockTransfer
- StockTransferItem
- StockCount
- StockCountItem
- StockAdjustment
- StockAllocation
- InventoryBatch
- InventorySerial

### Cash and bank

- FinancialAccount
- CashAccount
- BankAccount
- WalletAccount
- FinancialAccountTransaction
- AccountTransfer
- Reconciliation
- ReconciliationItem

### Expenses

- Expense
- ExpenseCategory
- ExpenseApproval
- ExpenseReversal

### Accounting

- ChartOfAccount
- Account
- FiscalYear
- FiscalPeriod
- JournalEntry
- JournalLine
- PostingRule
- AccountingSourceLink

### Delivery

- DeliveryOrder
- DeliveryAssignment
- DeliveryEvent
- ProofOfDelivery

### Employees

- Employee
- Department
- Position
- AttendanceRecord
- LeaveRequest
- SalaryStructure
- PayrollRun
- PayrollEntry
- Payslip

### Assets

- Asset
- AssetCategory
- AssetAssignment
- AssetMaintenance
- DepreciationEntry
- AssetDisposal

### Synchronization

- SyncOperation
- ProcessedSyncOperation
- SyncCursor
- SyncConflict
- DeviceSyncState

---

## 20. Domain modeling rules

Use:

- explicit state transitions;
- controlled setters;
- domain methods for invariant-sensitive changes;
- value objects where they improve correctness;
- enums for finite statuses;
- UUID identifiers for offline-created records;
- decimal-safe money;
- explicit currency codes;
- domain events where useful.

Avoid:

- anemic entities for important financial rules;
- public mutation of posted transactions;
- setting inventory balances directly;
- directly editing customer balances;
- directly editing supplier balances;
- recalculating historical transaction prices.

---

## 21. Double-entry accounting

Unify must support double-entry bookkeeping.

Every posted journal entry must:

- contain at least two lines;
- have total debit equal to total credit;
- have an organisation;
- have a posting date;
- belong to an open fiscal period;
- identify the source module;
- identify the source transaction;
- be auditable;
- become immutable after posting.

Corrections to posted journals require reversal.

### Account types

- Asset
- Liability
- Equity
- Revenue
- CostOfGoodsSold
- Expense

### Semantic account roles

Do not hard-code account IDs.

Use configurable roles such as:

- CashControl
- BankControl
- WalletControl
- AccountsReceivableControl
- AccountsPayableControl
- InventoryControl
- SalesRevenue
- ServiceRevenue
- CostOfGoodsSold
- ExpenseControl
- CustomerAdvance
- SupplierAdvance
- TaxPayable
- TaxReceivable
- RetainedEarnings

### Accounting examples

Cash sale:

```text
Debit Cash
Credit Sales Revenue
```

Perpetual inventory impact:

```text
Debit Cost of Goods Sold
Credit Inventory
```

Credit sale:

```text
Debit Accounts Receivable
Credit Sales Revenue
```

Customer payment:

```text
Debit Cash or Bank
Credit Accounts Receivable
```

Supplier purchase on credit:

```text
Debit Inventory or Expense
Credit Accounts Payable
```

Supplier payment:

```text
Debit Accounts Payable
Credit Cash or Bank
```

Expense paid in cash:

```text
Debit Expense Account
Credit Cash
```

The system must prevent duplicate accounting postings from the same source transaction.

---

## 22. Fiscal periods

Support:

- fiscal years;
- monthly periods;
- open;
- closed;
- reopened with permission;
- period locking;
- closing audit.

Normal transactions must not post into closed periods.

Reopening a period must require permission and create an audit event.

---

## 23. Critical backend invariants

### General

1. Tenant-owned records must never cross organisation boundaries.
2. Branch-restricted users must remain within assigned branches.
3. Client-calculated totals are not authoritative.
4. Completed financial transactions must not be silently deleted.
5. Corrections must use reversal, return, credit note, debit note, or adjustment.
6. Every sensitive operation must be audited.
7. Money must use decimal values and explicit currency.
8. Source transactions must remain linked to accounting entries.

### Sales

9. A completed sale must contain at least one valid item.
10. Sale totals must be calculated by the server.
11. Paid amount must not exceed the amount due unless the excess becomes customer advance.
12. Historical prices must remain unchanged.
13. Discounts below allowed limits require permission.
14. Credit-limit override requires permission and a reason.
15. Duplicate idempotency keys must not create duplicate sales.

### Receivables

16. Payment allocations cannot exceed available payment.
17. Payment allocations cannot exceed invoice outstanding balance unless an explicit advance workflow is used.
18. Customer balances must derive from ledger entries.
19. Synchronized payments require reversal for correction.

### Purchasing and payables

20. Goods received beyond tolerance require approval.
21. Supplier liabilities must link to supplier invoices.
22. Supplier-payment retries must not duplicate payments.
23. Supplier balances derive from ledger entries.

### Inventory

24. Every inventory change creates a stock movement.
25. Direct unexplained stock overwrite is prohibited.
26. Offline sales cannot exceed device allocation.
27. Stock transfers require source and destination.
28. Count variances require adjustment records.
29. Negative stock must never be silently hidden.

### Accounting

30. Posted journals contain at least two lines.
31. Debits equal credits.
32. Posted journals are immutable.
33. Corrections use reversal.
34. Closed periods reject normal posting.
35. Source module and transaction are retained.
36. Duplicate source posting is prohibited.

### Security

37. Disabled users cannot receive new tokens.
38. Disabled devices cannot synchronize.
39. Refresh-token reuse revokes the token family.
40. Server permissions are always authoritative.

---

# Part 3 — Flutter, Local Database, Offline-First Architecture, Synchronization, and Conflict Resolution

## 24. Flutter architecture

Use a feature-first structure:

```text
lib/
├── app/
├── core/
│   ├── config/
│   ├── database/
│   ├── networking/
│   ├── security/
│   ├── tenancy/
│   ├── sync/
│   ├── theme/
│   ├── routing/
│   ├── errors/
│   ├── files/
│   ├── printing/
│   └── widgets/
└── features/
    ├── auth/
    ├── organisations/
    ├── dashboard/
    ├── customers/
    ├── suppliers/
    ├── products/
    ├── quotations/
    ├── sales/
    ├── receivables/
    ├── purchases/
    ├── payables/
    ├── inventory/
    ├── cash_bank/
    ├── expenses/
    ├── accounting/
    ├── deliveries/
    ├── employees/
    ├── payroll/
    ├── assets/
    ├── reports/
    ├── sync/
    ├── users/
    ├── audit/
    └── settings/
```

Within a feature separate:

- data;
- domain;
- presentation.

Do not place business logic directly in widgets.

Use Riverpod consistently.

Do not introduce competing state-management frameworks.

Use GoRouter for:

- authentication redirects;
- tenant selection;
- permission-aware navigation;
- desktop and mobile shells;
- deep linking where appropriate.

---

## 25. Local source of truth

For offline-enabled screens, Drift must be the normal operational read source.

The UI should not switch unpredictably between:

- remote API state;
- local state;
- temporary widget state.

Server responses should update the local database.

The UI should observe Drift streams or repositories backed by Drift.

This ensures:

- fast local screens;
- predictable offline operation;
- automatic UI updates after synchronization;
- fewer duplicate state sources.

---

## 26. Drift local database

Create local tables for the implemented slices.

Initial tables include:

- CachedOrganisation
- CachedBranch
- CachedUserProfile
- CachedPermission
- DeviceState
- Customer
- CustomerAddress
- Supplier
- Product
- Price
- Warehouse
- StockBalance
- StockAllocation
- Sale
- SaleItem
- CustomerPayment
- PaymentAllocation
- CustomerLedgerEntry
- Expense
- OutboxOperation
- SyncCursor
- SyncHistory
- SyncConflict
- LocalSetting

Required synchronization fields where relevant:

- id;
- organisationId;
- branchId;
- deviceId;
- createdByUserId;
- createdAtUtc;
- updatedAtUtc;
- syncStatus;
- localSequence;
- serverVersion;
- idempotencyKey;
- lastSyncAttemptAtUtc;
- lastSyncedAtUtc;
- isDeleted.

Use versioned Drift migrations.

Write migration tests.

Never store access tokens or refresh tokens in SQLite.

---

## 27. Offline-capable operations

Allow offline execution for approved workflows including:

- customer creation;
- customer update;
- product viewing;
- price viewing;
- sales creation;
- customer payment collection;
- expense recording;
- delivery update;
- stock movement against allocated stock;
- provisional report generation;
- local receipt printing.

Server-required operations may include:

- organisation creation;
- user creation;
- permission changes;
- module enablement;
- final period closing;
- global price changes;
- major journal approval;
- device stock allocation;
- final consolidated financial reports;
- high-risk reversals.

---

## 28. Local transaction rule

When an offline-capable operation succeeds:

1. validate locally;
2. save the business record;
3. save local ledger or stock movement records;
4. save one outbox operation;
5. commit all changes in one SQLite transaction.

If any step fails, none of the changes should commit.

The UI receives local success only after the local transaction commits.

The record then displays a synchronization state.

---

## 29. Synchronization states

Use clear statuses:

- LocalOnly
- Pending
- Synchronizing
- Synchronized
- Conflict
- Rejected
- RetryScheduled
- Reversed

The UI must display:

- offline status;
- last successful synchronization;
- pending count;
- conflict count;
- rejected count;
- current retry state.

---

## 30. Push synchronization

Implement:

```text
POST /api/v1/sync/push
```

The request includes:

- organisation ID;
- branch ID;
- device ID;
- batch ID;
- app version;
- last known cursor;
- ordered operations.

Each operation includes:

- operation ID;
- idempotency key;
- entity type;
- entity ID;
- operation type;
- payload;
- local sequence;
- client timestamp;
- base server version.

Per-operation response:

- Accepted
- Duplicate
- Conflict
- Rejected
- RetryableFailure

Response details:

- server entity ID;
- server version;
- stable error code;
- safe user-facing message.

---

## 31. Pull synchronization

Implement:

```text
POST /api/v1/sync/pull
```

Use a cursor or monotonic server sequence.

Return:

- changed records;
- deleted-record tombstones;
- updated prices;
- updated permissions;
- organisation modules;
- device stock allocations;
- conflict decisions;
- next cursor.

Pull must be incremental.

Do not redownload the full database during each synchronization.

---

## 32. Idempotency

Create `ProcessedSyncOperation`.

Use a unique constraint that prevents duplicate business processing.

If the same operation is sent again:

- do not create another sale;
- do not create another payment;
- do not create another stock movement;
- do not create another expense;
- return the original result.

This is required because clients may retry after:

- timeout;
- app restart;
- network interruption;
- unknown server response.

---

## 33. Dependency ordering

Preserve local sequence ordering.

Support client-generated UUIDs.

Example dependency:

```text
Customer creation
    ↓
Sale referencing customer
    ↓
Payment referencing sale
```

The synchronization engine must not process dependent operations in an invalid order.

---

## 34. Retry strategy

Use exponential backoff with jitter.

Persist retry state.

Do not retry permanent rejections forever.

Distinguish:

- transient network failure;
- server unavailable;
- authentication expired;
- permission rejected;
- validation rejected;
- concurrency conflict;
- device disabled;
- organisation disabled;
- module disabled.

Connectivity status is only a signal.

The app must verify server reachability.

---

## 35. Offline stock allocation

Two offline devices cannot safely share one undivided stock balance.

Use device-specific stock allocation.

Allocation fields:

- OrganisationId
- BranchId
- WarehouseId
- DeviceId
- ProductId
- InventoryCondition
- AllocatedQuantity
- ConsumedQuantity
- ReturnedQuantity
- EffectiveAtUtc
- ExpiresAtUtc
- RevokedAtUtc
- ServerVersion

Example:

```text
Central stock: 100
Shop desktop allocation: 65
Delivery phone A: 15
Delivery phone B: 10
Reserve: 10
```

Rules:

- an offline device cannot exceed its remaining allocation;
- expired allocation cannot be used;
- revoked allocation cannot be used;
- server-issued allocation changes arrive through pull synchronization;
- physical transfers require stock-transfer records;
- negative stock requires reconciliation;
- do not silently cancel a legitimate offline sale after synchronization.

---

## 36. Conflict resolution

### Financial transactions

Sales, payments, supplier payments, expenses, transfers, and posted journals are append-only after acceptance.

Never use last-write-wins.

Corrections require:

- reversal;
- return;
- credit note;
- debit note;
- adjustment.

Every correction includes:

- original transaction reference;
- reason;
- user;
- device;
- timestamp;
- approval where required.

### Customer master data

Use optimistic concurrency.

Resolution options:

- accept server version;
- accept client version with permission;
- merge non-conflicting fields;
- manual review.

Preserve change history.

### Prices

The server is authoritative for current prices.

Offline transactions preserve the exact price used.

Historical sales are never recalculated.

### Inventory

Inventory is represented through movements.

Do not overwrite central stock balances with client-calculated quantities.

### Permissions

Server permissions are authoritative.

Cached permissions may permit limited offline work only within an explicit validity period.

Highly sensitive operations may require online validation.

---

## 37. Offline reporting

Official reports are generated from synchronized PostgreSQL data.

Offline reports are generated from local Drift data and must be marked:

```text
PROVISIONAL OFFLINE REPORT
Data current as of: [timestamp]
Pending operations: [count]
Conflicts: [count]
```

The user must be able to distinguish official and provisional reports.

---

## 38. Flutter security

Use secure platform storage for:

- access token;
- refresh token;
- device secret if used.

Do not store them in Drift.

Do not log tokens.

Do not expose full sensitive data in crash logs.

Ensure release builds cannot use development TLS bypasses.

Handle session expiry safely.

Clear secure session state during logout or revocation.

---

# Part 4 — ERP Modules, Business Rules, Reports, UI, Branding, Performance, and User Experience

## 39. CRM module

Support:

- customer registration;
- individual and business customers;
- contacts;
- addresses;
- customer categories;
- notes;
- activity timeline;
- purchase history;
- payment history;
- credit history;
- customer statement;
- credit limit;
- payment terms;
- active/inactive status;
- duplicate detection.

Customer history must be preserved after deactivation.

---

## 40. Sales module

Support:

- quotations;
- sales orders;
- invoices;
- point of sale;
- multiple sale items;
- cash sale;
- credit sale;
- partial payment;
- mixed payment;
- discounts;
- taxes;
- customer advance;
- returns;
- refunds;
- cancellation through controlled reversal;
- receipt;
- A4 invoice;
- audit history.

The server calculates totals.

Historical prices, discounts, taxes, and costs must be stored with the transaction.

---

## 41. Receivables

Support:

- customer invoices;
- multiple payments per invoice;
- one payment across multiple invoices;
- partial settlement;
- customer advance;
- ageing;
- overdue status;
- write-off with permission;
- full customer ledger;
- statements;
- payment receipt.

Customer balances derive from ledger entries.

---

## 42. Purchasing and payables

Support:

- purchase request;
- purchase order;
- approval;
- supplier quotation reference;
- goods receipt;
- partial receipt;
- damaged or missing goods;
- supplier invoice;
- supplier liability;
- supplier payment;
- supplier advance;
- payment allocation;
- purchase return;
- payable ageing;
- supplier statement.

---

## 43. Inventory

Support:

- products;
- services;
- variants;
- units;
- warehouses;
- locations;
- stock movements;
- transfers;
- receipts;
- issues;
- returns;
- physical count;
- adjustment;
- reorder level;
- damaged stock;
- batch tracking;
- serial tracking;
- stock valuation;
- device allocation.

Every inventory change creates a movement.

---

## 44. Cash and bank

Support:

- cash account;
- bank account;
- wallet account;
- Easypaisa;
- JazzCash;
- deposits;
- withdrawals;
- transfers;
- reconciliation;
- daily cash closing;
- source transaction traceability.

---

## 45. Expenses

Support:

- categories;
- amount;
- payee;
- date;
- method;
- branch;
- attachment;
- approval;
- reversal;
- accounting posting;
- reporting.

Corrections must preserve original records.

---

## 46. Deliveries

Support:

- delivery order;
- assignment;
- customer address;
- driver;
- vehicle;
- delivery status;
- payment collection;
- returned goods;
- returned cylinders;
- proof of delivery;
- notes;
- failed delivery;
- offline completion.

---

## 47. Employees and payroll

Future module after core stability.

Support:

- employee;
- department;
- position;
- employment status;
- attendance;
- leave;
- salary structure;
- allowance;
- deduction;
- payroll period;
- payroll run;
- payslip;
- salary payment;
- accounting posting.

---

## 48. Assets

Future module after core stability.

Support:

- asset;
- category;
- purchase value;
- location;
- employee assignment;
- maintenance;
- depreciation;
- disposal;
- accounting impact.

---

## 49. LPG extension module

The LPG extension is optional.

Support:

- cylinder type;
- capacity;
- ownership type;
- condition;
- full cylinder;
- empty cylinder;
- damaged cylinder;
- customer-held cylinder;
- supplier-held cylinder;
- deposit;
- exchange;
- return;
- LPG sold by kilogram;
- LPG sold by cylinder;
- refill operation;
- leakage;
- loss;
- commercial and household customers;
- delivery;
- quantity-based tracking first;
- individual serial or QR tracking later.

Do not put LPG-only fields directly into generic Product, Sale, Customer, or StockMovement entities unless they are generic.

---

## 50. Reports

### Sales

- daily;
- weekly;
- monthly;
- yearly;
- custom date;
- by customer;
- by product;
- by employee;
- by branch;
- cash versus credit;
- discounts;
- returns.

### Purchases

- by supplier;
- by product;
- purchase order status;
- goods receipt;
- purchase return;
- supplier price history.

### Inventory

- stock balance;
- valuation;
- movement;
- warehouse;
- transfer;
- low stock;
- damaged stock;
- device allocation;
- physical count variance.

### Receivables

- customer balance;
- ageing;
- overdue invoices;
- statement;
- collection;
- bad debt.

### Payables

- supplier balance;
- ageing;
- statement;
- payment history.

### Accounting

- general ledger;
- journal report;
- trial balance;
- profit and loss;
- balance sheet;
- cash-flow statement;
- account transaction report.

### Expenses

- category;
- employee;
- branch;
- monthly comparison.

### Cash and bank

- cash book;
- bank book;
- wallet transactions;
- reconciliation;
- daily closing.

### Employees

- employee list;
- attendance;
- leave;
- payroll;
- salary payments.

### Audit

- user activity;
- sensitive changes;
- reversals;
- permission changes;
- synchronization conflicts.

---

## 51. Excel, PDF, and printing

Official report generation belongs in the backend.

Use maintained libraries with acceptable commercial licensing.

Before selecting libraries:

1. verify current maintenance;
2. verify license;
3. verify commercial use;
4. document the decision.

Support:

- `.xlsx`;
- PDF;
- CSV where useful;
- thermal receipt;
- A4 invoice;
- customer statement;
- supplier statement;
- payslip.

Android:

- save;
- share;
- open;
- print PDF through supported services;
- open Excel through compatible applications.

Windows:

- save;
- open;
- print;
- share;
- select output folder.

---

## 52. UI theme

Use Material 3.

Default Royal LPG theme:

- Royal Purple: `#4B0082`
- Metallic Gold: `#D4AF37`
- White: `#FFFFFF`
- Black Accent: `#111111`

Rules:

- purple is primary;
- gold is accent;
- white is surface;
- black is main readable text;
- gold must not be used for small body text on white;
- maintain accessible contrast;
- use consistent spacing;
- use consistent radii;
- use restrained shadows;
- avoid decorative overload.

Create:

- AppColors
- AppTypography
- AppSpacing
- AppRadius
- AppElevation
- AppBreakpoints

Reusable components:

- buttons;
- fields;
- dropdowns;
- search;
- date filters;
- chips;
- cards;
- tables;
- mobile lists;
- dialogs;
- confirmation prompts;
- offline banner;
- sync indicator;
- conflict indicator;
- permission-aware actions;
- loading state;
- empty state;
- error state.

---

## 53. Responsive layout

### Windows

Use:

- collapsible side navigation;
- top app bar;
- breadcrumb;
- responsive tables;
- multi-column forms;
- keyboard support;
- mouse support;
- print and export actions;
- resizable content.

### Android

Use:

- bottom navigation for key modules;
- drawer or More section;
- touch-friendly controls;
- compact cards;
- filter sheets;
- share;
- print;
- responsive tablet layouts.

---

## 54. Usability requirements

The app must provide:

- field-level validation;
- clear loading states;
- empty states;
- error states;
- success feedback;
- destructive confirmation;
- financial confirmation;
- clear offline status;
- clear pending status;
- clear rejected status;
- clear conflict status;
- readable money formatting;
- business timezone display;
- future English and Urdu localization.

Do not rely on color alone for status.

---

## 55. Performance requirements

Targets:

- normal local screens should load within approximately one second;
- normal API operations should usually complete within two seconds under expected load;
- large lists must use pagination or incremental loading;
- large reports may use background generation;
- database queries require appropriate indexes;
- avoid N+1 queries;
- avoid loading full tables into memory;
- use projections for reports;
- use caching only where consistency rules remain clear.

---

## 56. Caching strategy

Use caching carefully.

Potential server caching:

- permission catalogue;
- module configuration;
- static reference data;
- report metadata;
- non-sensitive read models.

Do not cache:

- mutable financial balances without explicit invalidation;
- current authorization decisions beyond safe scope;
- refresh tokens;
- sensitive personal data unnecessarily.

Document cache ownership, duration, and invalidation.

---

# Part 5 — DevOps, Testing, Documentation, Release, Git Workflow, and Codex Execution Instructions

## 57. Git workflow

Use `main` as the protected stable branch.

Recommended branches:

- `feature/...`
- `fix/...`
- `refactor/...`
- `docs/...`
- `release/...`

Use pull requests for major changes where the workflow supports them.

Do not commit broken builds to `main`.

Commit messages should be clear.

Recommended convention:

```text
feat(auth): add rotating refresh tokens
fix(sync): prevent duplicate payment processing
docs(api): document customer endpoints
test(inventory): cover device allocation conflicts
refactor(sales): extract total calculation service
chore(ci): add Windows Flutter build
```

---

## 58. GitHub Actions

Create workflows.

### backend-ci.yml

On push and pull request:

- checkout;
- set up supported .NET LTS;
- restore;
- verify formatting;
- build;
- run domain tests;
- run application tests;
- run architecture tests;
- run PostgreSQL integration tests;
- publish test results;
- upload coverage.

### flutter-ci.yml

- checkout;
- set up stable Flutter;
- `flutter pub get`;
- verify formatting;
- `flutter analyze`;
- run unit tests;
- run widget tests;
- build Android test artifact;
- build Windows on a Windows runner.

### docker-ci.yml

- build backend image;
- run smoke test;
- scan image where practical;
- push only from trusted main branch or tag;
- use GitHub Container Registry.

### codeql.yml

Enable CodeQL for supported languages.

### dependency-review.yml

Review dependency changes in pull requests.

### release workflows

Prepare manually triggered workflows for:

- backend container;
- Android App Bundle;
- Windows installer or release archive.

Do not commit signing credentials.

---

## 59. Dependabot

Configure weekly updates for:

- NuGet;
- Pub;
- GitHub Actions;
- Docker.

Group compatible non-breaking development updates where useful.

---

## 60. Docker

Create a multi-stage backend Dockerfile.

Requirements:

- separate restore layer;
- Release build;
- official ASP.NET runtime;
- non-root user where supported;
- health check;
- no development secrets;
- small final image.

Docker Compose development services:

- API;
- PostgreSQL;
- optional reverse proxy where useful.

Use named volumes.

Create `.env.example`.

Never commit `.env`.

---

## 61. Backups and disaster recovery

Document:

- automated PostgreSQL backups;
- retention;
- encryption where available;
- restore procedure;
- restore testing;
- off-site storage;
- backup monitoring;
- recovery point objective;
- recovery time objective;
- local device failure behavior;
- resynchronization after restore.

Backups are not complete until restore has been tested.

---

## 62. Logging and observability

Use structured Serilog logging.

Include:

- correlation ID;
- request path;
- status;
- duration;
- organisation ID;
- branch ID;
- user ID where safe;
- device ID;
- sync batch ID.

Never log:

- passwords;
- access tokens;
- refresh tokens;
- complete sensitive identifiers;
- full attachments;
- secret configuration.

Add:

- liveness endpoint;
- readiness endpoint;
- database health check;
- version endpoint.

---

## 63. Error handling

Backend:

- central exception handling;
- Problem Details;
- stable machine-readable error codes;
- no production stack traces.

Flutter:

distinguish:

- validation;
- authentication;
- authorization;
- network;
- server;
- conflict;
- rejected sync operation;
- expired allocation;
- disabled device;
- disabled user.

Provide safe retry actions where appropriate.

---

## 64. Testing requirements

### Domain tests

Test:

- tenant ownership;
- sale calculation;
- partial payment;
- payment allocation;
- customer balance;
- supplier balance;
- discounts;
- credit limits;
- stock movement;
- stock allocation;
- journal balancing;
- fiscal period closing;
- reversal.

### Integration tests

Use PostgreSQL Testcontainers where practical.

Test:

- authentication;
- token rotation;
- reuse detection;
- permissions;
- tenant isolation;
- branch restriction;
- customer API;
- sale transaction;
- duplicate synchronization;
- concurrency;
- database constraints;
- journal posting;
- stock allocation;
- audit creation.

### Flutter tests

Test:

- theme;
- routing;
- login;
- tenant selection;
- offline banner;
- customer validation;
- sale calculation;
- outbox creation;
- sync state transitions;
- conflict UI;
- permission visibility.

### Synchronization scenarios

Simulate:

1. offline creation;
2. app restart;
3. retry after failure;
4. duplicate push;
5. partial batch failure;
6. customer conflict;
7. rejected permission;
8. expired allocation;
9. pull after cursor;
10. tombstone deletion;
11. multiple devices;
12. server unavailable for hours;
13. user disabled while offline;
14. module disabled while operations are pending.

---

## 65. Coverage goals

Coverage numbers are indicators, not substitutes for good tests.

Targets:

- critical domain rules: very high coverage;
- authentication: full critical-flow coverage;
- synchronization idempotency: full critical-flow coverage;
- accounting balancing: full critical-flow coverage;
- tenant isolation: mandatory integration coverage;
- general modules: meaningful coverage focused on behavior.

Do not write useless tests only to increase percentage.

---

## 66. Documentation requirements

Create and maintain:

- README;
- setup guide;
- architecture decisions;
- multi-tenant design;
- ER diagram;
- system context diagram;
- container diagram;
- module diagram;
- accounting flow;
- synchronization sequence;
- authentication flow;
- permission model;
- API guide;
- Drift guide;
- Docker guide;
- GitHub Actions guide;
- backup guide;
- restore guide;
- Android release guide;
- Windows release guide;
- security checklist;
- troubleshooting guide.

Use Mermaid where suitable.

---

## 67. Release strategy

Use semantic versioning where practical:

```text
MAJOR.MINOR.PATCH
```

Examples:

- `0.1.0` foundation;
- `0.2.0` customers;
- `0.3.0` products and pricing;
- `0.4.0` sales;
- `1.0.0` first stable production release.

Prepare release notes.

Do not publish until:

- tests pass;
- migrations are reviewed;
- backup plan exists;
- security review is completed;
- Android release requirements are verified;
- Windows signing is configured;
- privacy and terms documents exist where required.

---

## 68. Google Play

Prepare:

- Android App Bundle;
- release signing configuration using secrets;
- application ID;
- privacy policy requirements;
- data safety information;
- screenshots;
- store listing;
- versioning;
- target API verification at release time.

Do not assume current Google Play rules remain unchanged.

Verify them at release time.

---

## 69. Windows distribution

Prepare:

- signed release build;
- installer strategy;
- version information;
- update strategy;
- installation documentation;
- uninstall behavior;
- data location documentation;
- secure local storage.

Do not expose development endpoints in release configuration.

---

## 70. First execution instructions

Begin with Slice 0 and Slice 1.

Perform:

1. inspect the workspace;
2. inspect existing files;
3. initialize Git if required;
4. create the monorepo;
5. create the .NET solution;
6. create the Flutter Android and Windows app;
7. add maintained compatible dependencies;
8. create Docker Compose PostgreSQL;
9. create `.env.example`;
10. implement configuration validation;
11. implement base domain types;
12. implement Organisation;
13. implement Branch;
14. implement ApplicationUser;
15. implement UserOrganisationMembership;
16. implement roles and permissions;
17. implement devices and sessions;
18. implement ASP.NET Core Identity;
19. implement JWT access tokens;
20. implement refresh token rotation;
21. implement token reuse detection;
22. implement tenant context;
23. implement branch restrictions;
24. implement permission policies;
25. implement audit logging;
26. implement login;
27. implement refresh;
28. implement logout;
29. implement session listing;
30. implement session revocation;
31. implement Flutter login;
32. implement secure token storage;
33. implement authenticated routing;
34. implement organisation selection;
35. implement branch selection;
36. implement the responsive application shell;
37. implement Royal LPG default theme;
38. implement offline status infrastructure;
39. implement API health checks;
40. add backend tests;
41. add Flutter tests;
42. add tenant isolation tests;
43. add GitHub Actions;
44. add Dependabot;
45. add CodeQL;
46. add backend Dockerfile;
47. run formatting;
48. run analyzers;
49. run builds;
50. run tests;
51. fix failures;
52. update README;
53. update architecture documentation;
54. commit verified work.

Do not continue to the Customer slice until the foundation is passing tests.

---

## 71. Completion report format

At the end of each Codex execution, report:

- files created;
- files modified;
- architecture decisions;
- migrations created;
- commands executed;
- test results;
- build results;
- security decisions;
- assumptions;
- known limitations;
- unresolved failures;
- next vertical slice;
- PostgreSQL startup command;
- backend startup command;
- Flutter Windows startup command;
- Flutter Android startup command;
- GitHub push commands if remote creation was unavailable.

Never hide failures.

---

## 72. Final instruction

Treat this file as the permanent engineering contract for Unify.

When another instruction conflicts with this file:

- follow the newest explicit user instruction;
- preserve security, data integrity, auditability, and tenant isolation;
- document the conflict and resulting decision.

Build Unify gradually, one tested vertical slice at a time.

Do not sacrifice correctness for speed.

Do not sacrifice maintainability for the appearance of progress.

Do not sacrifice auditability for convenience.

Do not sacrifice offline integrity for a simple implementation.

Produce working, testable, secure code.
