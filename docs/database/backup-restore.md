# Database Backup and Restore Runbook

This runbook covers backup, point-in-time recovery, and post-restore verification for `CrisisPlatform` on **SQL Server 2022**.

---

## 1. Recovery Objectives

| Metric | Target | Strategy |
|---|---|---|
| **RPO** (Recovery Point Objective) | <= 15 minutes | `FULL` recovery model with 15-minute transaction log backups |
| **RTO** (Recovery Time Objective) | <= 30 minutes | Automated daily `FULL` + 4-hour `DIFFERENTIAL` + log chain restore |

---

## 2. Backup Commands (T-SQL)

### Full Database Backup (Daily)

```sql
BACKUP DATABASE [CrisisPlatform]
TO DISK = N'/var/opt/mssql/backup/CrisisPlatform_full.bak'
WITH COMPRESSION, CHECKSUM, STATS = 10, INIT;
GO
```

### Differential Backup (Every 4 Hours)

```sql
BACKUP DATABASE [CrisisPlatform]
TO DISK = N'/var/opt/mssql/backup/CrisisPlatform_diff.bak'
WITH DIFFERENTIAL, COMPRESSION, CHECKSUM, STATS = 10, INIT;
GO
```

### Transaction Log Backup (Every 15 Minutes)

```sql
BACKUP LOG [CrisisPlatform]
TO DISK = N'/var/opt/mssql/backup/CrisisPlatform_log.trn'
WITH COMPRESSION, CHECKSUM;
GO
```

### Verify Backup Integrity

```sql
RESTORE VERIFYONLY
FROM DISK = N'/var/opt/mssql/backup/CrisisPlatform_full.bak'
WITH CHECKSUM;
GO
```

---

## 3. Restore Procedure

1. Put the database into `SINGLE_USER` mode (or restore to a staging database name first to avoid overwriting active connections):

```sql
USE master;
GO
ALTER DATABASE [CrisisPlatform] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

RESTORE DATABASE [CrisisPlatform]
FROM DISK = N'/var/opt/mssql/backup/CrisisPlatform_full.bak'
WITH NORECOVERY, REPLACE, STATS = 10;

RESTORE DATABASE [CrisisPlatform]
FROM DISK = N'/var/opt/mssql/backup/CrisisPlatform_diff.bak'
WITH NORECOVERY, STATS = 10;

RESTORE LOG [CrisisPlatform]
FROM DISK = N'/var/opt/mssql/backup/CrisisPlatform_log.trn'
WITH RECOVERY;
GO

ALTER DATABASE [CrisisPlatform] SET MULTI_USER;
GO
```

---

## 4. Docker Compose Volume Backup (Local / Dev)

```bash
# Stop API to flush writes, keep SQL Server or stop cleanly
docker compose stop api db

# Archive the sqldata volume
docker run --rm -v crisis-aid-platform_sqldata:/data -v "$(pwd)":/backup alpine \
  tar czf /backup/sqldata-backup.tar.gz -C /data .

docker compose start db api
```

---

## 5. Post-Restore Verification

1. Run `DBCC CHECKDB (N'CrisisPlatform') WITH NO_INFOMSGS;` to verify physical page integrity.
2. Run `database/scripts/verify-audit-immutability.sql` to confirm `INSTEAD OF` triggers on `dbo.AuditEntries` are active.
3. Verify the `AuditEntries` SHA-256 hash chain (`AuditHashChain.Verify`) from the genesis entry to the latest restored row.
4. Confirm `/health/services` reports `database: Healthy`.
