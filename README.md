# Crisis Aid Platform

**SE205.3 Software Architecture Coursework 2026**  
AI-Powered Humanitarian Crisis Management and Aid Coordination Platform

A multi-organisation platform for coordinating emergency assistance, shelters, volunteers, resources, donations and AI-assisted (human-approved) decision-making during disasters.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 10 Web API (C#, .NET 10) |
| Database | SQL Server 2022 |
| Frontend | Next.js 15 (App Router, React 19, TypeScript) |
| API Docs | OpenAPI / Swagger |
| Architecture | Modular Monolith |
| Containerisation | Docker + Docker Compose |
| CI | GitHub Actions |

---

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20 LTS](https://nodejs.org)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for the compose workflow)
- Git

### Option A — Docker Compose (recommended)

```bash
git clone https://github.com/lasithasathruwan6-ctrl/crisis-aid-platform.git
cd crisis-aid-platform

# Copy the example env file and fill in secrets (see docs/env-reference.md)
cp .env.example .env

# Start API + SQL Server together
docker compose up --build
```

| Service | URL |
|---------|-----|
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/openapi |

### Option B — Run directly

**Backend**

```bash
# Set secrets with dotnet user-secrets
dotnet user-secrets init --project backend/CrisisPlatform.Api
dotnet user-secrets set "Auth:Tokens:SigningKey" "<48+ chars>" --project backend/CrisisPlatform.Api
# ... set remaining secrets (see docs/env-reference.md)

dotnet run --project backend/CrisisPlatform.Api
dotnet test backend/CrisisPlatform.Tests
```
- **OpenAPI / Swagger:** `/openapi/v1.json` (JWT Bearer auth, DTO examples, 400/401/403/409 responses)
- **Health Checks:** `/health` and `/health/services` (`database`, `ai`, `email`)
- **Documentation:** [docs/backend/architecture-layers.md](docs/backend/architecture-layers.md) (layers, patterns, resilience, outbox) and [docs/backend/api-endpoints.md](docs/backend/api-endpoints.md) (endpoints, pagination, error codes)

**Frontend**

```bash
cd frontend
npm ci
npm run dev      # http://localhost:3000
```

---

## Running Tests

**Backend (xUnit)**

```bash
dotnet test backend/CrisisPlatform.Tests/CrisisPlatform.Tests.csproj
```

**Frontend (Vitest)**

```bash
cd frontend
npm test
```

**API (Newman / Postman)**

```bash
# Install Newman
npm install -g newman

# Run the full collection against localhost:8080
bash scripts/run-newman.sh
```

See [`docs/testing/newman-guide.md`](docs/testing/newman-guide.md) for detailed instructions.

---

## Documentation Index

| Document | Description |
|----------|-------------|
| [`docs/deploy.md`](docs/deploy.md) | Full deployment guide (local, Docker, production, rollback) |
| [`docs/env-reference.md`](docs/env-reference.md) | Every environment variable, its purpose and format |
| [`docs/release-checklist.md`](docs/release-checklist.md) | Pre-release checklist (code, security, testing, deployment) |
| [`CONTRIBUTING.md`](CONTRIBUTING.md) | Commit style, branch workflow, review process, folder ownership |
| **Testing** | |
| [`docs/testing/test-plan.md`](docs/testing/test-plan.md) | Test strategy, scope, environments and defect policy |
| [`docs/testing/coverage-table.md`](docs/testing/coverage-table.md) | Feature × test-type coverage matrix |
| [`docs/testing/newman-guide.md`](docs/testing/newman-guide.md) | How to run the Postman collection with Newman |
| [`docs/testing/uat-form.md`](docs/testing/uat-form.md) | UAT test cases (40 cases across all roles) |
| [`docs/testing/uat-results-template.md`](docs/testing/uat-results-template.md) | UAT results recording template |
| **Security** | |
| [`docs/security/secrets.md`](docs/security/secrets.md) | Secret handling rules and what counts as a secret |
| [`docs/security/role-endpoint-matrix.md`](docs/security/role-endpoint-matrix.md) | Role × endpoint permission matrix |
| [`docs/security/threat-model.md`](docs/security/threat-model.md) | Threat model |
| [`docs/security/security-testing-report.md`](docs/security/security-testing-report.md) | Security test results (271 passing tests) |
| [`docs/security/dependency-audit.md`](docs/security/dependency-audit.md) | Dependency vulnerability audit |
| **Frontend** | |
| [`docs/frontend/structure.md`](docs/frontend/structure.md) | Frontend folder structure, routes and role guards |
| [`docs/frontend/ui-guide.md`](docs/frontend/ui-guide.md) | Design tokens, accessibility, i18n, responsive patterns |
| **Diagrams** | |
| [`docs/diagrams/diagrams.md`](docs/diagrams/diagrams.md) | Class diagram, sequence diagrams (auth, request lifecycle), deployment diagram |
| **API** | |
| [`docs/postman/crisis-aid-platform.postman_collection.json`](docs/postman/crisis-aid-platform.postman_collection.json) | Postman collection (import into Postman or run with Newman) |
| [`docs/postman/crisis-aid-platform.postman_environment.json`](docs/postman/crisis-aid-platform.postman_environment.json) | Postman environment (no secrets) |

---

## Team

| Member | Name | GitHub | Area |
|--------|------|--------|------|
| 1 | Lasitha Sathruwan | @lasithasathruwan6-ctrl | Architecture, Security, Auth, Admin |
| 2 | Hasindu Indusara | @hasindu-indusara | Database, Requests, Human-in-the-loop |
| 3 | Sasmitha Thejan | @bandara331 | AI services, Resources |
| 4 | Dimalsha Dewmini | @dimalsha-dewmini | Notifications, Mapping, Async |
| 5 | Prageeth Ekanayake | @Madhusha-Prageeth | Shelters, Volunteers |
| 6 | Rasindu Gimhana | @gimhanarasidu38-create | Citizens, Donations, QA / Testing / DevOps |

---

## Security

- Security headers, strict single-origin CORS, auth rate limiting, password policy, hashed OTP with cooldown, short-lived JWTs + rotating/revokable refresh tokens, account lockout, AES-256-GCM NIC encryption + masking + keyed uniqueness hash, deny-by-default role policies + ownership scoping, single-use 24h staff invitations, two-person role change approval, log redaction, upload signature validation.
- **271 passing security tests.** See [`docs/security/security-testing-report.md`](docs/security/security-testing-report.md).
- Secrets are never committed. See [`docs/security/secrets.md`](docs/security/secrets.md).

---

## CI Status

| Workflow | Trigger |
|----------|---------|
| Backend build & test | Push / PR on `backend/**` |
| Frontend lint, typecheck & build | Push / PR on `frontend/**` |
| Dependabot | Weekly (Monday) for NuGet, npm, Actions |

---

## Known Limitations

- No automated CD pipeline — deployment is currently manual.
- No database migration framework — schema changes are applied via SQL scripts.
- OTP delivery requires a live SMTP server; CI skips OTP-gated flows.
- Frontend Docker image not yet provided.
