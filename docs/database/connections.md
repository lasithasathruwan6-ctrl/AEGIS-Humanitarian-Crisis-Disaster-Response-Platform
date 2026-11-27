# Database Connections & Pooling

This guide covers SQL Server 2022 connection configuration, connection pooling, local development setup, and environment variables for `CrisisPlatform`.

---

## 1. Connection String Configuration

The backend reads the database connection string from `ConnectionStrings:Default` (environment variable `ConnectionStrings__Default`).

### Local Docker Compose (`db` service)

Configured in `docker-compose.yml`:

```text
Server=db,1433;Database=CrisisPlatform;User Id=sa;Password=${DB_SA_PASSWORD};TrustServerCertificate=True;
```

### Local SQL Server / Developer Instance

```text
Server=localhost,1433;Database=CrisisPlatform;User Id=<app_login>;Password=<password>;Encrypt=True;TrustServerCertificate=True;Min Pool Size=5;Max Pool Size=100;Connect Timeout=15;
```

### Production / Staging

```text
Server=<db-host>,1433;Database=CrisisPlatform;User Id=crisis_app;Password=<secret>;Encrypt=True;TrustServerCertificate=False;Min Pool Size=10;Max Pool Size=200;MultipleActiveResultSets=False;
```

> [!CAUTION]
> Never use `sa` in staging or production. Create a least-privilege SQL login (`crisis_app`) with `SELECT`, `INSERT`, `UPDATE` (excluding `AuditEntries`), `DELETE` (only where soft-delete is not used, e.g. `RefreshTokens`, `OtpChallenges`, `OutboxMessages` cleanup), and **no** `ALTER` or `CONTROL` permissions.

---

## 2. Connection Pooling (`Microsoft.Data.SqlClient`)

ADO.NET / `SqlClient` enables connection pooling by default (`Pooling=true`):

| Parameter | Recommended Value | Purpose |
|---|---|---|
| `Pooling` | `true` (default) | Reuses open TDS connections across HTTP requests |
| `Min Pool Size` | `5` (dev) / `10` (prod) | Pre-warms connections to avoid cold-start latency |
| `Max Pool Size` | `100` (dev) / `200` (prod) | Caps concurrent connections per API instance |
| `Connect Timeout` | `15` | Bounds connection acquisition wait time (seconds) |
| `Connection Lifetime` | `300` (when behind LB/DNS) | Forces periodic connection recycling across failovers |

---

## 3. Setting Secrets Locally

Never commit connection strings or passwords. Use `.env` (git-ignored) for Docker Compose or `dotnet user-secrets` when running the API directly:

```powershell
dotnet user-secrets init --project backend/CrisisPlatform.Api
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=CrisisPlatform;User Id=crisis_app;Password=<password>;Encrypt=True;TrustServerCertificate=True;" --project backend/CrisisPlatform.Api
```
