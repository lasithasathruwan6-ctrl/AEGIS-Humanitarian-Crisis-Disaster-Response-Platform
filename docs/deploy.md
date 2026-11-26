# Deployment Guide — Crisis Aid Platform

**Last updated:** 2026-11-26  
**Maintained by:** Member 6 (QA / DevOps)

---

## Overview

The platform consists of two independently deployable units:

| Unit | Technology | Default port |
|------|-----------|-------------|
| API  | ASP.NET Core 10 / Docker | 8080 (HTTP inside container) |
| Frontend | Next.js 15 / Node.js | 3000 |
| Database | SQL Server 2022 | 1433 |

HTTPS termination is handled by a reverse proxy (nginx, Caddy, or a cloud load balancer). The API never binds to port 443 directly.

---

## Prerequisites

| Tool | Version |
|------|---------|
| Docker + Docker Compose | 24.x + |
| .NET SDK | 10.0.x (for local `dotnet run`) |
| Node.js | 20 LTS (for local `npm run build`) |
| SQL Server | 2022 (or use the Docker Compose service) |

---

## 1 — Local Development

### 1.1 Clone and configure

```bash
git clone https://github.com/lasithasathruwan6-ctrl/crisis-aid-platform.git
cd crisis-aid-platform
cp .env.example .env        # then fill in every variable — see docs/env-reference.md
```

### 1.2 Start with Docker Compose

```bash
docker compose up --build
```

| Service | URL |
|---------|-----|
| API     | http://localhost:8080 |
| Swagger | http://localhost:8080/openapi |
| SQL Server | localhost:1433 |

Stop and remove volumes:

```bash
docker compose down -v
```

### 1.3 Run without Docker

**Backend**

```bash
# Set required secrets with dotnet user-secrets (see docs/security/secrets.md)
dotnet user-secrets init --project backend/CrisisPlatform.Api
dotnet user-secrets set "Auth:Tokens:SigningKey" "<48+ chars>" --project backend/CrisisPlatform.Api
# ... set remaining secrets

dotnet run --project backend/CrisisPlatform.Api
```

**Frontend**

```bash
cd frontend
npm ci
npm run dev      # development server on http://localhost:3000
```

---

## 2 — Building for Production

### 2.1 API Docker image

```bash
# From the repo root
docker build -f backend/Dockerfile -t crisis-aid-api:latest backend/
```

The image:
- Uses a multi-stage build (SDK → runtime only).
- Runs as a non-root user (`appuser`, UID 1001).
- Listens on port 8080 (HTTP). Set `ASPNETCORE_URLS=http://+:8080`.
- Expects all secrets as environment variables (see `docs/env-reference.md`).

### 2.2 Frontend production build

```bash
cd frontend
npm ci
npm run build      # output in .next/
npm run start      # production server on port 3000
```

Or build a Docker image for the frontend using a standard Next.js Dockerfile pattern
(not provided here — add one if the team chooses containerised frontend deployment).

---

## 3 — Database

### 3.1 Applying the schema

```bash
# Using sqlcmd (adjust connection details)
sqlcmd -S <host> -U <user> -P <password> -d <database> \
       -i database/schema.sql
```

There is currently no migration framework; the schema SQL is in `database/`.
When a migration framework is adopted, this section must be updated.

### 3.2 Connection string format

```
Server=<host>,<port>;Database=CrisisPlatform;User Id=<user>;Password=<password>;TrustServerCertificate=True;
```

> [!CAUTION]
> Use a least-privilege SQL login — not `sa`. Grant only `db_datareader` + `db_datawriter` on the application database.

---

## 4 — Environment Variables

All runtime configuration is injected via environment variables. No secrets are stored in `appsettings.json` or committed to Git.

See **[docs/env-reference.md](env-reference.md)** for the full list, purpose and format of every variable.

---

## 5 — Reverse Proxy (nginx example)

```nginx
server {
    listen 443 ssl;
    server_name api.example.org;

    ssl_certificate     /etc/ssl/certs/api.example.org.crt;
    ssl_certificate_key /etc/ssl/private/api.example.org.key;

    location / {
        proxy_pass         http://127.0.0.1:8080;
        proxy_set_header   Host              $host;
        proxy_set_header   X-Real-IP         $remote_addr;
        proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

---

## 6 — Health Check

The API exposes a health endpoint (once wired) at `GET /healthz`.
The Docker Compose `healthcheck` for the SQL Server container uses `sqlcmd`.

Add an ASP.NET Core health check (`app.MapHealthChecks("/healthz")`) when the full module layer is integrated.

---

## 7 — CI/CD Pipeline

| Trigger | Workflow | What it does |
|---------|----------|-------------|
| Push / PR to `main` (backend path) | `.github/workflows/backend.yml` | Restore → build → `dotnet test` → upload TRX |
| Push / PR to `main` (frontend path) | `.github/workflows/frontend.yml` | `npm ci` → lint → typecheck → Vitest → `next build` |
| Weekly (Monday) | Dependabot | Opens PRs for NuGet, npm and Actions updates |

Deployment to staging/production is currently manual (run `docker compose` or push the image to your registry). A CD pipeline should be added before the first production release.

---

## 8 — Rollback

1. Identify the last good image tag or Git commit.
2. `docker pull crisis-aid-api:<previous-tag>` and restart the container.
3. If a database schema change was applied, restore from the last backup before restarting.

---

## 9 — Known Limitations

- No automated CD pipeline yet (manual Docker push).
- No database migration framework — schema changes are applied by running SQL scripts manually.
- OTP delivery requires a live SMTP server; CI runs skip OTP-gated tests.
- Frontend Docker image not yet provided.
