# SQL Server Provider Mapping Notes & Known Differences

This document records SQL Server 2022 (`Microsoft.Data.SqlClient` / T-SQL) type mappings, behavioural differences from PostgreSQL/SQLite, and design decisions for `CrisisPlatform`.

---

## 1. CLR to SQL Server Type Mappings

| .NET / Domain Type | SQL Server 2022 Type | Notes |
|---|---|---|
| `Guid` | `UNIQUEIDENTIFIER` | Primary keys and foreign keys. Stored in 16 bytes; use `NEWSEQUENTIALID()` for DB-generated clustered keys. |
| `DateTimeOffset` | `DATETIMEOFFSET(7)` | Preserves UTC offset (`+00:00`). Always normalize to UTC before persisting. |
| `byte[]` (concurrency token) | `ROWVERSION` | 8-byte auto-incrementing binary value updated on every `INSERT`/`UPDATE`. Mapped as optimistic concurrency token. |
| `string` (fixed hash) | `CHAR(64)` | SHA-256 / HMAC-SHA256 hex strings (`NicHash`, `TokenHash`, `CodeHash`, `PrevHash`, `EntryHash`). |
| `string` (text) | `NVARCHAR(n)` / `NVARCHAR(MAX)` | Unicode-safe for English, Sinhala (`si`), and Tamil (`ta`) text. |
| `byte[]` (ciphertext) | `VARBINARY(MAX)` | AES-256-GCM encrypted NIC payload (`EncryptedNic`). |
| `decimal` (currency/confidence) | `DECIMAL(18, 2)` / `DECIMAL(5, 4)` | Exact fixed-point arithmetic for `Donations.Amount` and `AiRecommendations.Confidence`. |
| `bool` | `BIT` | `IsLocked`, `IsRevoked`, `IsAvailable`, `IsActive`. |

---

## 2. Known Behavioural Differences

### Multiple Cascade Paths
SQL Server rejects schemas where two or more `ON DELETE CASCADE` paths reach the same table (error `1785`).
- **Mitigation:** All cross-aggregate foreign keys (`AidRequests.CitizenId`, `Donations.DonorId`, `RoleChangeRequests`, `AiRecommendations`) use `ON DELETE NO ACTION` and rely on soft-delete (`DeletedAt`).

### Filtered Unique Indexes
SQL Server allows only **one** `NULL` in a standard `UNIQUE` index.
- **Mitigation:** Nullable unique columns (`AidRequests.IdempotencyKey`, `OutboxMessages.IdempotencyKey`, soft-deleted `Users.NicHash`) use filtered unique indexes (`WHERE IdempotencyKey IS NOT NULL AND DeletedAt IS NULL`).

### Audit Table Immutability (`INSTEAD OF` Triggers)
SQL Server does not support PostgreSQL `CREATE RULE ... DO INSTEAD NOTHING`.
- **Mitigation:** `dbo.AuditEntries` uses `INSTEAD OF UPDATE` and `INSTEAD OF DELETE` triggers that `THROW 51000` and roll back the transaction, paired with revoking `ALTER` / `CONTROL` so `TRUNCATE TABLE` is also blocked for the application login.

### Case Sensitivity
Default SQL Server collations (`SQL_Latin1_General_CP1_CI_AS`) are case-insensitive.
- **Mitigation:** Hex hashes (`NicHash`, `TokenHash`) and email lookups behave consistently, while application-level `Idempotency-Key` comparisons use `StringComparer.Ordinal` / `CS` collation where exact byte matching is required.

### Optimistic Concurrency
SQLite and in-memory test doubles do not auto-generate `ROWVERSION` values.
- **Mitigation:** Concurrency tests use explicit version checks (`expectedRowVersion == actualRowVersion`) or SQL Server container integration tests to verify stale-update rejection.
