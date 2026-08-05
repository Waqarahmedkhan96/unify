# Unify ERP — Synchronization Design

> **Important note:** This is the foundation synchronization design. A complete synchronization protocol (every payload, sequence diagram, retry scenario, conflict matrix, cursor format, batch protocol, and timing diagram) would be far larger than one generated response.

# Part 1 – Objectives
- Offline-first
- Idempotent
- Reliable
- Incremental
- Conflict-aware

# Part 2 – Local Flow
User Action
→ SQLite Transaction
→ Outbox Entry
→ Pending Sync

# Part 3 – Push
Client sends ordered operations:
- UUID
- Entity
- Operation
- Payload
- Idempotency Key
- Local Sequence

Server validates:
- User
- Device
- Tenant
- Permission
- Business Rules

# Part 4 – Pull
Client sends cursor.
Server returns:
- Changed records
- Tombstones
- Updated permissions
- Stock allocations
- Next cursor

# Part 5 – Conflict Rules
Financial:
- Never overwrite
- Reverse or adjust

Customer:
- Optimistic concurrency
- Merge/manual review

Prices:
- Server authoritative
- Historical sales unchanged

Inventory:
- Movement based
- No balance overwrite

# Part 6 – Offline Stock
Allocate stock per device.
Offline sales cannot exceed allocation.

# Part 7 – Retry
Exponential backoff.
Persist retry state.
Never duplicate accepted operations.

# Part 8 – Statuses
- LocalOnly
- Pending
- Synchronizing
- Synchronized
- Conflict
- Rejected

# Part 9 – Integrity Rules
- Atomic local writes
- UUIDs
- Ordered dependencies
- Processed operation table
- Audit sync failures

# Part 10 – Future
Future document expands protocol, payload schemas, conflict matrices, Mermaid diagrams, retries, recovery and performance tuning.
