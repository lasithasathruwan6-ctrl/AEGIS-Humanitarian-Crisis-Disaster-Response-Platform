# Environment Variable Reference — Crisis Aid Platform

**Purpose:** One-stop reference for every environment variable the platform reads.
No real values are recorded here. See `docs/security/secrets.md` for how to set them.

> [!IMPORTANT]
> Variables marked **Secret** must never be committed to source control.
> Set them with `dotnet user-secrets` (local), Docker Compose `.env` file (local only),
> or the host's secret store (staging/production).

---

## Backend (ASP.NET Core)

In .NET configuration, `__` in an env-var name maps to `:` in the config key.
Example: `Auth__Tokens__SigningKey` → `Auth:Tokens:SigningKey`.

| Variable | Config key | Secret | Required | Format | Purpose |
|----------|-----------|:------:|:--------:|--------|---------|
| `Auth__Tokens__SigningKey` | `Auth:Tokens:SigningKey` | ✅ | ✅ | Base64 string ≥ 32 bytes | Signs JWT access tokens. The API refuses to start if this is absent or shorter than 32 bytes. |
| `Auth__Tokens__AccessTokenMinutes` | `Auth:Tokens:AccessTokenMinutes` | ☐ | ☐ | Integer 1–30, default `15` | Lifetime of access tokens in minutes. Must be ≤ 30. |
| `Auth__Tokens__RefreshTokenDays` | `Auth:Tokens:RefreshTokenDays` | ☐ | ☐ | Integer, default `7` | Lifetime of refresh tokens in days. |
| `Security__NicEncryptionKey` | `Security:NicEncryptionKey` | ✅ | ✅ | Base64 of exactly 32 bytes | AES-256-GCM key for NIC encryption at rest. |
| `Security__NicHashKey` | `Security:NicHashKey` | ✅ | ✅ | Base64 ≥ 32 bytes | Keyed HMAC for the NIC uniqueness hash. Must differ from `NicEncryptionKey`. |
| `Security__OtpHashKey` | `Security:OtpHashKey` | ✅ | ✅ | Base64 ≥ 32 bytes | Keyed HMAC for OTP codes. Must differ from the other two keys. |
| `ConnectionStrings__Default` | `ConnectionStrings:Default` | ✅ | ✅ | SQL Server connection string | Database connection. Use a least-privilege login, not `sa`. |
| `Cors__AllowedOrigin` | `Cors:AllowedOrigin` | ☐ | ✅ | URL (no trailing slash) | The single browser origin allowed by CORS. E.g. `https://app.example.org`. |
| `Smtp__Host` | `Smtp:Host` | ☐ | ✅ | Hostname | SMTP relay host for OTP and invitation emails. |
| `Smtp__Port` | `Smtp:Port` | ☐ | ☐ | Integer, default `587` | SMTP port. |
| `Smtp__Username` | `Smtp:Username` | ☐ | ✅ | String | SMTP authentication username. |
| `Smtp__Password` | `Smtp:Password` | ✅ | ✅ | String | SMTP authentication password. |
| `Smtp__FromAddress` | `Smtp:FromAddress` | ☐ | ✅ | Email address | Sender address for outgoing emails. |
| `RateLimiting__Auth__PermitLimit` | `RateLimiting:Auth:PermitLimit` | ☐ | ☐ | Integer, default `5` | Maximum auth requests per window. |
| `RateLimiting__Auth__WindowSeconds` | `RateLimiting:Auth:WindowSeconds` | ☐ | ☐ | Integer, default `60` | Rate-limit sliding window in seconds. |
| `ASPNETCORE_ENVIRONMENT` | — | ☐ | ☐ | `Development` / `Staging` / `Production` | Controls which `appsettings.<env>.json` is loaded and whether Swagger is exposed. |
| `ASPNETCORE_URLS` | — | ☐ | ☐ | URL list, default `http://+:8080` in Docker | Overrides the ports the API listens on. |

---

## Frontend (Next.js)

Next.js exposes variables to the browser only when prefixed with `NEXT_PUBLIC_`.
Variables without that prefix are server-side only.

| Variable | Side | Required | Format | Purpose |
|----------|------|:--------:|--------|---------|
| `NEXT_PUBLIC_API_URL` | Browser + Server | ✅ | URL (no trailing slash) | Base URL of the backend API. E.g. `https://api.example.org`. |
| `NEXT_TELEMETRY_DISABLED` | Server (CI) | ☐ | `1` | Set to `1` in CI to suppress Next.js telemetry. |

---

## Docker Compose (local development)

The `docker-compose.yml` reads these from a `.env` file in the repo root (git-ignored).
Copy `.env.example` to `.env` and fill in the values.

| Variable in `.env` | Consumed by | Notes |
|--------------------|-------------|-------|
| `DB_SA_PASSWORD` | `db` service (SQL Server `MSSQL_SA_PASSWORD`) | Used only locally. Never reuse this password in production. |
| `Auth__Tokens__SigningKey` | `api` service | Forwarded directly to the API container. |
| `Security__NicEncryptionKey` | `api` service | Forwarded directly to the API container. |
| `Security__NicHashKey` | `api` service | Forwarded directly to the API container. |
| `Security__OtpHashKey` | `api` service | Forwarded directly to the API container. |
| `Smtp__Host` | `api` service | Forwarded directly to the API container. |
| `Smtp__Username` | `api` service | Forwarded directly to the API container. |
| `Smtp__Password` | `api` service | Forwarded directly to the API container. |

---

## GitHub Actions (CI)

CI uses short fixed-value strings for the crypto keys. These are **not** production values and are safe to embed in the workflow YAML because:
- They sign nothing that leaves the CI runner.
- They encrypt no real data.
- They are distinct from any production or staging key.

| Secret / variable | Where set | Notes |
|-------------------|-----------|-------|
| `Auth__Tokens__SigningKey` | Inline in `backend.yml` env block | CI-only value; not a GitHub secret. |
| `Security__NicEncryptionKey` | Inline in `backend.yml` env block | CI-only value. |
| `Security__NicHashKey` | Inline in `backend.yml` env block | CI-only value. |
| `Security__OtpHashKey` | Inline in `backend.yml` env block | CI-only value. |

For staging/production deployments add GitHub Actions secrets (`Settings → Secrets and variables → Actions`) and reference them as `${{ secrets.MY_SECRET }}`.

---

## Generating Secret Values

```powershell
# PowerShell — 32-byte key
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))

# Or with openssl
openssl rand -base64 32    # NIC keys
openssl rand -base64 48    # signing key (extra margin)
```

Each key must be a **different random value**. Using the same value for two purposes defeats key separation.
