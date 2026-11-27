# Database Migrations Guide

Crisis Aid Platform applies schema changes using **ordered, idempotent T-SQL scripts** targeting **SQL Server 2022**.

---

## Directory Layout

```text
database/
├── migrations/   # Numbered schema, index, constraint, and trigger scripts
├── seeds/        # Idempotent reference-data MERGE scripts
└── scripts/      # Operational and verification scripts (audit immutability, reference check)
```

---

## Naming Convention

Migration files follow `<NNN>_<short_snake_case_description>.sql`:

- `NNN` is a zero-padded 3-digit sequence number (`001`, `002`, `003`, ...).
- Numbers are strictly monotonic; never renumber or edit a migration that has already been applied to a shared environment.
- Every script must be **re-runnable (idempotent)** using `IF OBJECT_ID(...)` and `IF NOT EXISTS (...)` guards against `sys.indexes`, `sys.check_constraints`, `sys.triggers`, or `sys.foreign_keys`.

### Current Migration Order

| Order | File | Purpose |
|---:|---|---|
| 001 | `001_requests_status_district_created_indexes.sql` | Filtered queue & listing indexes on `dbo.AidRequests` |
| 002 | `002_conversations_messages_indexes.sql` | Conversation thread & chronological message pagination indexes |
| 003 | `003_unique_indexes_nic_and_idempotency.sql` | Unique indexes on `Users.NicHash`, `AidRequests.IdempotencyKey`, and `OutboxMessages.IdempotencyKey` |
| 004 | `004_check_constraints_occupancy_quantity.sql` | `CHECK` constraints for shelter occupancy, resource quantity, and request urgency |
| 005 | `005_four_eyes_check_constraints.sql` | Four-eyes `CHECK` constraints on `AiRecommendations` and `RoleChangeRequests` |

---

## How to Apply

### 1. Local Docker Compose (`db` service)

Start SQL Server from the repository root:

```bash
docker compose up -d db
```

Apply all migrations in numerical order followed by reference-data seeds:

```bash
for f in database/migrations/*.sql database/seeds/*.sql; do
  echo "Applying $f ..."
  docker compose exec -T db /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$DB_SA_PASSWORD" -d CrisisPlatform -C -b -i - < "$f"
done
```

### 2. Local SQL Server / `sqlcmd`

```powershell
Get-ChildItem database/migrations/*.sql, database/seeds/*.sql | Sort-Object FullName | ForEach-Object {
    Write-Host "Applying $($_.Name)..."
    sqlcmd -S localhost -d CrisisPlatform -U sa -P $env:DB_SA_PASSWORD -C -b -i $_.FullName
}
```

> [!IMPORTANT]
> Always pass `-b` to `sqlcmd` so execution halts immediately on the first T-SQL error.

---

## Post-Apply Verification

1. Verify reference data integrity locally:
   ```powershell
   powershell -ExecutionPolicy Bypass -File database/scripts/verify-reference-data.ps1
   ```
2. Verify audit table immutability on SQL Server:
   ```bash
   sqlcmd -S localhost -d CrisisPlatform -U "$DB_APP_USER" -P "$DB_APP_PASSWORD" -C -b -i database/scripts/verify-audit-immutability.sql
   ```
