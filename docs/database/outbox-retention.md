# Outbox Retention & Cleanup

`dbo.OutboxMessages` guarantees at-least-once delivery for asynchronous notifications and domain events. Once delivered, old rows must be pruned to prevent unbounded table growth.

---

## Retention Policy

| `Status` | Retention | Action |
|---|---|---|
| `Pending` | Until delivered or dead-lettered | Never pruned |
| `DeadLettered` | Indefinite (manual triage) | Retained for operator investigation |
| `Delivered` | **30 days** after `SentAt` | Deleted in batches of 1,000 rows |

---

## Running Cleanup

```bash
sqlcmd -S localhost -d CrisisPlatform -U "$DB_APP_USER" -P "$DB_APP_PASSWORD" -C -b \
  -i database/scripts/cleanup-old-outbox.sql
```

The script deletes in `TOP (1000)` batches to avoid SQL Server lock escalation and transaction-log spikes.
