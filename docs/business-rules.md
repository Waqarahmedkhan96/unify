# Unify ERP — Business Rules

> **Important note:** This is the foundation business-rules document. A complete enterprise rule catalogue (every validation, workflow, approval, exception, accounting rule, inventory rule, and module-specific policy) would be hundreds of pages and cannot fit into one generated file.

# Part 1 – Global Rules
- Tenant isolation
- Server authoritative
- UTC timestamps
- Decimal money
- Audit important actions

# Part 2 – Customer Rules
- Unique customer number per organisation
- Preserve history
- Credit limit enforcement
- Statements from ledger
- Customer creation requires a valid branch in the same organisation
- Customer deactivation preserves the record for sales, payments, and ledger history

# Part 3 – Supplier Rules
- Preserve purchase history
- Supplier balance from ledger
- No duplicate payments
- Unique supplier number per organisation
- Supplier deactivation preserves the record for purchases, payments, and ledger history

# Part 4 – Sales Rules
- At least one item
- Server calculates totals
- Historical prices immutable
- Discounts require permissions
- Corrections through returns/reversals
- Invoice number must be unique per organisation
- Sales require active customers in the sale branch
- Sales require sufficient stock for inventory-tracked products

# Part 4.1 - Product Rules
- Unique product code per organisation
- Products require a valid unit of measure in the same organisation
- Product categories are optional but must belong to the same organisation when provided
- Purchase and sales prices cannot be negative
- Product deactivation preserves historical sales, purchasing, and inventory records

# Part 5 – Payment Rules
- Allocations cannot exceed payment
- Allocations cannot exceed outstanding balance
- Advances supported
- Customer payments require an active customer in the payment branch
- Receipt number must be unique per organisation
- Customer balance is derived from ledger debits minus credits

# Part 6 – Inventory Rules
- Every change creates StockMovement
- No direct balance edits
- Offline allocation enforced
- Counts create adjustments
- Transfers require source/destination
- Stock cannot go negative
- Transfers create paired outbound and inbound movements
- Inventory movements require an active inventory-tracked product

# Part 7 – Purchasing Rules
- Goods receipt before invoice where configured
- Returns preserve history
- Supplier liabilities auditable
- Purchase orders require at least one item
- Purchase order and goods receipt numbers are unique per organisation
- Supplier invoice numbers are unique per supplier within an organisation
- Goods receipts increase inventory through stock movements

# Part 8 – Accounting Rules
- Double-entry
- Debits equal credits
- Posted journals immutable
- Closed periods reject posting
- Reversals only

# Part 9 – Security Rules
- Disabled users cannot log in
- Disabled devices cannot sync
- Permission required for sensitive actions
- Audit security changes

# Part 10 – Future
Future document will define every module's detailed workflows, approvals, validation matrices, edge cases, LPG rules, payroll rules, and reporting logic.
