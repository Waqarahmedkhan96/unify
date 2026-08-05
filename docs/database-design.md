# Unify ERP — Database Design

> **Product:** Unify ERP  
> **Purpose:** Foundation database design for the ERP platform.

> **Important note:** This document intentionally provides a **concise architectural database design** that Codex can use as the implementation starting point. A truly complete enterprise database design for Unify ERP (every entity, every column, every foreign key, every index, constraints, migration strategy, ER diagrams, and full data dictionary) would span **hundreds of pages** and cannot fit into a single AI-generated response or one-file output due to model limits.

---

# Part 1 — Database Philosophy

## Goals
- PostgreSQL as the central source of truth.
- SQLite (Drift) on devices for offline work.
- Multi-tenant by design.
- Financial integrity.
- Offline synchronization.
- Auditability.
- Scalability.
- Maintainability.

Core rules:
- UUID primary keys.
- UTC timestamps.
- `decimal` for money.
- Soft delete only where appropriate.
- Append-only financial records.
- Tenant isolation via `OrganisationId`.

---

# Part 2 — High-Level Database Architecture

```text
Flutter
 ├─ SQLite (Drift)
 ├─ Outbox
 └─ Sync
        │
      HTTPS
        │
 ASP.NET Core
        │
 PostgreSQL
```

SQLite stores cached and pending data.
PostgreSQL stores authoritative business data.

---

# Part 3 — Core Entity Groups

## Platform
- Organisation
- Branch
- Warehouse
- Device
- User
- Membership
- Role
- Permission
- RefreshToken
- AuditEntry
- Notification

Implemented audit foundation:
- `audit_entries`
- Captures create, update, and delete operations for domain entities
- Stores entity name, entity id, operation, changed properties, old values, and new values
- Captures optional organisation, user, user email, and correlation id
- JSONB columns are used for value snapshots
- Indexed by organisation/date, entity, and correlation id

## CRM
- Customer
- CustomerAddress
- CustomerContact
- CustomerLedgerEntry

Implemented customer foundation:
- `customers`
- Tenant scoped by `organisation_id`
- Branch scoped by `branch_id`
- Unique `(organisation_id, customer_number)`
- Restricted foreign keys to organisation and branch
- Decimal credit limit
- Deactivation instead of hard delete

Implemented customer receivables foundation:
- `customer_ledger_entries`
- `customer_payments`
- `payment_allocations`
- Sales post debit ledger entries
- Customer payments post credit ledger entries
- Customer balances are derived from ledger entries

## Suppliers
- Supplier
- SupplierAddress
- SupplierLedgerEntry

Implemented supplier foundation:
- `suppliers`
- Tenant scoped by `organisation_id`
- Unique `(organisation_id, supplier_number)`
- Restricted foreign key to organisation
- Deactivation instead of hard delete

## Products
- Product
- ProductCategory
- UnitOfMeasure
- PriceList
- PriceHistory
- TaxRate

Implemented product catalog foundation:
- `units_of_measure`
- `product_categories`
- `products`
- Tenant scoped by `organisation_id`
- Unique `(organisation_id, code)` for units and categories
- Unique `(organisation_id, product_code)` for products
- Decimal purchase and sales prices
- Restricted foreign keys for catalogue history

## Sales
- Sale
- SaleItem
- CustomerPayment
- PaymentAllocation
- SaleReturn

Implemented sales foundation:
- `sales`
- `sale_items`
- Tenant scoped by `organisation_id`
- Unique `(organisation_id, invoice_number)`
- Server-calculated totals
- Sale creation issues stock movements for inventory-tracked products
- Posted sales are preserved for history

## Purchasing
- PurchaseOrder
- PurchaseOrderItem
- GoodsReceipt
- SupplierInvoice
- SupplierPayment

Implemented purchasing foundation:
- `purchase_orders`
- `purchase_order_items`
- `goods_receipts`
- `goods_receipt_items`
- `supplier_invoices`
- Goods receipts create purchase receipt stock movements
- Purchase orders can move from Open to Received

## Inventory
- StockMovement
- StockTransfer
- StockAdjustment
- StockAllocation
- StockCount

Implemented inventory foundation:
- `stock_movements`
- `stock_balances`
- Append-only movement history
- Maintained warehouse/product balances
- Tenant scoped by `organisation_id`
- Restricted foreign keys to organisation, warehouse, and product
- Decimal quantities with 3 decimal places

## Finance
- FinancialAccount
- Expense
- JournalEntry
- JournalLine
- FiscalYear
- FiscalPeriod
- Account

Implemented accounting foundation:
- `accounts`
- `fiscal_periods`
- `journal_entries`
- `journal_lines`
- Journals must balance before posting
- Journals require an open fiscal period

## Sync
- SyncOperation
- ProcessedSyncOperation
- SyncConflict
- SyncCursor

---

# Part 4 — Common Columns

Most tenant-owned tables should contain:

- Id
- OrganisationId
- BranchId (where applicable)
- CreatedAtUtc
- UpdatedAtUtc
- CreatedByUserId
- UpdatedByUserId
- Version
- IsDeleted (only if applicable)

Offline-capable entities additionally contain:

- DeviceId
- SyncStatus
- ServerVersion
- LastSyncAtUtc
- IdempotencyKey

---

# Part 5 — Relationship Rules

Examples:

Organisation
 ├── Branch
 │     ├── Warehouse
 │     ├── Customer
 │     ├── Supplier
 │     ├── Sale
 │     └── Expense

Sale
 ├── SaleItems
 ├── Payments
 ├── Ledger Entries
 ├── Journal Entries
 └── Stock Movements

Purchase
 ├── Goods Receipt
 ├── Supplier Invoice
 ├── Supplier Payment
 └── Stock Movements

Never allow cross-organisation foreign keys.

---

# Part 6 — Indexing Strategy

Create indexes for:

- OrganisationId
- BranchId
- WarehouseId
- CustomerNumber
- SupplierNumber
- InvoiceNumber
- ProductCode
- SaleDate
- PaymentDate
- JournalDate
- SyncStatus

Composite examples:

- (OrganisationId, CustomerNumber)
- (OrganisationId, ProductCode)
- (OrganisationId, InvoiceNumber)

Unique constraints should normally include OrganisationId.

---

# Part 7 — Money & Accounting

Rules:

- decimal(18,2) or equivalent.
- Never float/double.
- Journals balance.
- Posted journals immutable.
- Corrections through reversals.
- Customer/Supplier balances derived from ledger entries.
- Inventory valuation from movements.

---

# Part 8 — Offline Database (SQLite)

Primary cached tables:

- Customers
- Suppliers
- Products
- Prices
- StockBalance
- StockAllocation
- Sales
- SaleItems
- Payments
- Expenses
- CachedPermissions
- OutboxOperation
- SyncHistory
- SyncConflict

SQLite is not the authoritative source.

---

# Part 9 — Synchronization Storage

Each operation stores:

- OperationId
- EntityType
- EntityId
- OperationType
- Payload
- LocalSequence
- DeviceId
- OrganisationId
- IdempotencyKey
- CreatedAtUtc

Server stores processed operation IDs to prevent duplicates.

---

# Part 10 — Referential Integrity

- Required foreign keys.
- Cascade delete only for safe child records.
- Financial records should not cascade delete.
- Restrict deletion where history must remain.

---

# Part 11 — Migration Strategy

- EF Core Migrations.
- Version-controlled.
- Small incremental migrations.
- Never edit applied migrations.
- Drift migrations versioned alongside app releases.

---

# Part 12 — Backup Strategy

PostgreSQL:
- Automated backups.
- Tested restore.
- Encrypted storage where available.

SQLite:
- Disposable cache except unsynced operations.
- Preserve outbox until acknowledged.

---

# Part 13 — Data Retention

Never physically delete:
- Posted journals
- Payments
- Sales
- Purchase invoices
- Audit entries

Use:
- reversal
- adjustment
- deactivation
- archive

---

# Part 14 — LPG Extension Tables

Optional module:

- CylinderType
- CylinderCondition
- CylinderDeposit
- CylinderExchange
- LPGDelivery
- LPGReturn

These reference generic ERP entities instead of modifying them.

---

# Part 15 — ER Overview

```text
Organisation
 ├── Branch
 │    ├── Warehouse
 │    ├── Customer
 │    │      ├── Sale
 │    │      │     ├── SaleItem
 │    │      │     ├── Payment
 │    │      │     ├── StockMovement
 │    │      │     └── JournalEntry
 │    ├── Supplier
 │    │      ├── PurchaseOrder
 │    │      └── SupplierPayment
 │    └── Expense
```

---

# Part 16 — Database Standards

- snake_case in PostgreSQL.
- PascalCase in C#.
- Nullable only when business-valid.
- Explicit FK names.
- Explicit indexes.
- UTC timestamps.
- GUID identifiers.
- Transactions for financial workflows.

---

# Part 17 — Future Expansion

Future documents will define:

- Complete table definitions.
- Full data dictionary.
- Every column.
- Every FK.
- Every index.
- Check constraints.
- ER diagrams.
- Partitioning.
- Performance tuning.
- Archiving.
- Analytics schema.
- Reporting schema.
- Multi-region strategy.

This document is the architectural foundation for Codex and future implementation.
