# Bootstrap Admin Guide

Initialising the first `Admin` account on a fresh `CrisisPlatform` database must be explicit, auditable, and idempotent. No default or demo admin accounts are ever seeded in migrations or application startup.

---

## Preconditions

1. All database migrations (`database/migrations/*.sql`) and reference-data seeds (`database/seeds/001_reference_data.sql`) have been applied.
2. No active (`DeletedAt IS NULL`) user with `Role = 'Admin'` exists in `dbo.Users`.
3. Credentials are supplied via environment variables or `dotnet user-secrets`—never hardcoded in scripts or committed to source control.

---

## Environment Variables

| Variable | Required | Format | Purpose |
|---|---:|---|---|
| `BOOTSTRAP_ADMIN_EMAIL` | Yes | Valid email address | Login email for the initial administrator |
| `BOOTSTRAP_ADMIN_FULL_NAME` | Yes | Non-empty string | Display name |
| `BOOTSTRAP_ADMIN_PASSWORD` | Yes | Passes `PasswordPolicy` (>= 12 chars, mixed classes, not common) | Initial password (hashed with Argon2id/PBKDF2 before storage) |

---

## Command Usage

```bash
# Supply secrets via environment (cleared after execution)
export BOOTSTRAP_ADMIN_EMAIL="<admin-email>"
export BOOTSTRAP_ADMIN_FULL_NAME="<admin-name>"
export BOOTSTRAP_ADMIN_PASSWORD="<strong-passphrase>"

dotnet run --project backend/CrisisPlatform.Api -- bootstrap-admin
```

### Guardrails

- **Single-use:** If an active `Admin` already exists in `dbo.Users`, the command exits without modifying any rows.
- **Password Policy:** Enforces `PasswordPolicy.Validate(password, email)` before creating the account.
- **Audit Trail:** Writes an `AdminBootstrapped` entry to `dbo.AuditEntries` linked into the SHA-256 hash chain.
- **Subsequent Admins:** Must be onboarded via `StaffInvitationService` and promoted through the two-person `RoleChangeService` approval flow.
