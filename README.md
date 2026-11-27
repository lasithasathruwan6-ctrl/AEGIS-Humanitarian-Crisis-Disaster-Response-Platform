AEGIS — Humanitarian Crisis & Disaster Response Platform
**SE205.3 – Software Architecture Coursework 2026 | Group 15 | NSBM Green University**  
**AI-Powered Humanitarian Crisis Management and Aid Coordination Platform**

A full-stack, trilingual (**English, Sinhala, Tamil**) multi-organisation platform built for coordinating emergency assistance requests, shelters, volunteers, relief resources, donations, and AI-assisted decision-making during disasters across Sri Lanka's 25 districts.

> **Core Governance Principle:** AI only scores, drafts, or recommends (with deterministic heuristics, PII redaction, and counterfactual explanations). Authenticated human coordinators and administrators always review, approve, and act — with every critical action recorded in an append-only, SHA-256 hash-chained audit ledger.

---

## Repository Structure (Git Tree)

```text
crisis-aid-platform/
├── .github/
│   ├── dependabot.yml                    # Weekly NuGet, npm & GitHub Actions security updates
│   └── workflows/
│       ├── backend.yml                   # .NET 10 build & xUnit test CI pipeline
│       └── frontend.yml                  # Next.js 15 lint, typecheck, test & build CI pipeline
├── backend/
│   ├── Dockerfile                        # Multi-stage ASP.NET Core 10 container image
│   ├── CrisisPlatform.Api/               # Modular Monolith Web API (/api/v1)
│   │   ├── Common/                       # Auditing, Errors, Events, Health, OpenAPI, Pagination, Resilience, Security
│   │   └── Modules/                      # Admin, AI, Auth, Crises, Donations, Mapping, Metrics, Notifications,
│   │                                     # Partners, Requests, Resources, Shelters, Volunteers
│   └── CrisisPlatform.Tests/             # xUnit unit, security, database & integration test suites
├── database/
│   ├── migrations/                       # Ordered idempotent T-SQL migrations (001–007: indexes, constraints, triggers)
│   ├── seeds/                            # Reference data seed scripts (001_reference_data.sql)
│   └── scripts/                          # Audit immutability verification & outbox retention cleanup scripts
├── frontend/
│   ├── src/
│   │   ├── app/                          # Next.js 15 App Router pages (/, /login, /register, /shelters, /citizen, /staff, /admin)
│   │   ├── components/                   # RoleGuard, Navbar, ShelterMap, RequestChat, SessionExpiredModal, StatusState, LoadingSkeleton
│   │   ├── hooks/                        # usePolling (exponential back-off & AbortController cleanup)
│   │   ├── i18n/                         # Trilingual dictionaries (en, si, ta) & localStorage-persisted I18nProvider
│   │   ├── lib/                          # API client (401 handling), auth session helpers, NIC/phone/email/upload validation
│   │   └── styles/                       # Unified WCAG AA design tokens (tokens.css)
│   └── tests/                            # Vitest + Testing Library accessibility, i18n, validation & component tests
├── docs/
│   ├── ai/                               # AI architecture, prompt guardrails & evaluation/limitations
│   ├── backend/                          # Modular monolith layers, resilience & API endpoint reference
│   ├── database/                         # Schema overview, ER diagram, data dictionary, migrations, PDPA & backup guides
│   ├── diagrams/                         # Mermaid class, sequence & deployment diagrams as code
│   ├── frontend/                         # Frontend route/guard structure & UI/accessibility guide
│   ├── postman/                          # Postman collection & environment for automated API verification
│   ├── security/                         # Threat model, role-endpoint matrix, secrets policy, security report & audit
│   └── testing/                          # Test plan, coverage matrix, Newman CLI guide & UAT templates
├── scripts/
│   └── run-newman.sh                     # Automated Postman/Newman API test runner
├── .env.example                          # Template of required environment variables (no secrets)
├── CONTRIBUTING.md                       # Conventional commits, PR workflow, code review & folder ownership
├── CrisisPlatform.slnx                   # .NET 10 solution file
└── docker-compose.yml                    # One-command local stack (API + SQL Server 2022)
```

---

## System Architecture & Tech Stack

| Layer | Technology | Location |
|-------|------------|----------|
| **Architecture** | Modular Monolith + Transactional Outbox + Human-in-the-Loop AI | `backend/CrisisPlatform.Api` |
| **Backend API** | ASP.NET Core 10 Web API (C#, .NET 10), OpenAPI / Swagger | `backend/CrisisPlatform.Api` |
| **Frontend Web** | Next.js 15 (App Router), React 19, TypeScript, CSS Custom Properties | `frontend/` |
| **Database** | Microsoft SQL Server 2022 (EF Core + T-SQL migrations, triggers & `ROWVERSION`) | `database/` |
| **AI & Triage** | Deterministic explainable scoring + PII-redacted LLM fallback (Groq) + Copilot drafts | `Modules/AI` |
| **Async & Resilience** | Transactional Outbox (`OutboxProcessor`), idempotency keys, exponential back-off, dead-lettering | `Common/Events`, `Common/Resilience` |
| **Testing** | xUnit (.NET), Vitest + React Testing Library (Frontend), Postman / Newman (API) | `backend/CrisisPlatform.Tests`, `frontend/tests`, `docs/postman` |
| **DevOps & CI** | Docker Compose, GitHub Actions (`backend.yml`, `frontend.yml`), Dependabot | `.github/`, `docker-compose.yml` |

---

## Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20 LTS](https://nodejs.org/) & npm
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) *(recommended for SQL Server 2022 + API compose setup)*
- Git

### Option A — Docker Compose (API + SQL Server 2022)

```bash
git clone https://github.com/lasithasathruwan6-ctrl/crisis-aid-platform.git
cd crisis-aid-platform

# Copy the template and populate required values (see docs/env-reference.md)
cp .env.example .env

# Build and start SQL Server 2022 + ASP.NET Core API
docker compose up --build
```

| Service | Default URL |
|---------|-------------|
| **Backend API** | `http://localhost:8080/api/v1` |
| **OpenAPI / Swagger** | `http://localhost:8080/openapi/v1.json` |
| **Health Checks** | `http://localhost:8080/health` and `/health/services` |

### Option B — Local Development (Backend + Frontend)

#### 1. Backend (`backend/CrisisPlatform.Api`)
Secrets are never committed to Git. Configure local secrets with `dotnet user-secrets` (see [`docs/env-reference.md`](docs/env-reference.md) and [`docs/security/secrets.md`](docs/security/secrets.md)):

```bash
dotnet user-secrets init --project backend/CrisisPlatform.Api
dotnet user-secrets set "Auth:Tokens:SigningKey" "<strong-48+-char-signing-key>" --project backend/CrisisPlatform.Api

dotnet run --project backend/CrisisPlatform.Api
```

#### 2. Frontend (`frontend/`)
```bash
cd frontend
npm ci
npm run dev
```
Open `http://localhost:3000` (set `NEXT_PUBLIC_API_BASE_URL` in `frontend/.env.local` if the API runs on a custom port).

---

## Key Engineering Highlights

### 1. Security & Access Control
- **Authentication:** Password policy enforcement, hashed OTP verification with cooldown, short-lived JWT access tokens, rotating/revocable refresh tokens, and brute-force account lockout.
- **Authorisation:** **Deny-by-default** role policies across 6 roles (`Anon`, `Citizen`, `Volunteer`, `Staff`, `Coordinator`, `Admin`) verified against [`docs/security/role-endpoint-matrix.md`](docs/security/role-endpoint-matrix.md), plus query-level ownership scoping for citizen records.
- **Data Protection:** AES-256-GCM NIC encryption at rest, masked NIC responses, keyed HMAC uniqueness hashing, log PII/secret redaction, magic-byte file upload validation, 24-hour hashed staff invitations, and **four-eyes (two-admin)** role-change approvals.

### 2. Database Integrity & Audit Immutability
- **Constraints & Concurrency:** Filtered unique indexes (`NicHash`, `IdempotencyKey`), `CHECK` constraints (`CurrentOccupancy <= Capacity`, `Quantity >= 0`, distinct requester/approver IDs), soft-delete (`DeletedAt`), and `ROWVERSION` optimistic concurrency on shelter capacity and inventory.
- **Tamper-Evident Audit Log:** Append-only `AuditEntries` protected by SQL Server `INSTEAD OF UPDATE/DELETE` triggers and a SHA-256 hash chain.

### 3. Explainable AI & Human-in-the-Loop Governance
- **Deterministic First:** Rule-based urgency scoring with deterministic counterfactual explanations, duplicate request detection (token similarity + distance + time window), district weather risk calculation (with stale-data guard), volunteer skill/distance matching, and donation anomaly flagging.
- **Strict Safety:** PII is redacted before any optional external LLM call; AI copilot drafts and recommendations **never auto-execute or auto-send** without human coordinator approval.

### 4. Accessible, Trilingual & Resilient Frontend
- **Accessibility:** Unified design tokens verified to WCAG 2.1 AA (`>= 4.5:1`), visible `:focus-visible` outlines, skip-to-content navigation, enforced `aria-label`s on icon-only buttons, and `@media (prefers-reduced-motion: reduce)` support.
- **Sri Lankan Context:** Complete **English (`en`), Sinhala (`සිංහල`), and Tamil (`தமிழ்`)** citizen flows with `localStorage` persistence, client-side Sri Lankan NIC (`9-digit+V/X` & `12-digit`), `+94`/local phone, email, and 5 MB evidence upload validation.
- **Resilience & Performance:** Global 401 `<SessionExpiredModal />`, `usePolling` hook with exponential back-off, `<RequestChat />` with optimistic send & retry, `<ShelterMap />` tile-failure fallback alert, and `next/dynamic` lazy-loading.

---

## Running Tests & Quality Checks

```bash
# 1. Backend unit, security, resilience, AI & integration tests (xUnit)
dotnet test backend/CrisisPlatform.Tests/CrisisPlatform.Tests.csproj

# 2. Frontend lint (zero warnings), strict TypeScript check, Vitest suite & production build
cd frontend
npm run check
npm test
npm run build

# 3. API end-to-end collection via Newman (against running API on localhost:8080)
bash scripts/run-newman.sh
```

---

## CI/CD Workflows

| Workflow | File | Trigger | Checks |
|----------|------|---------|--------|
| **Backend CI** | [`.github/workflows/backend.yml`](.github/workflows/backend.yml) | Push / PR touching `backend/**` | `.NET 10` restore, build, and `dotnet test` |
| **Frontend CI** | [`.github/workflows/frontend.yml`](.github/workflows/frontend.yml) | Push / PR touching `frontend/**` | `npm ci`, ESLint, `tsc --noEmit`, Vitest, `next build` |
| **Dependabot** | [`.github/dependabot.yml`](.github/dependabot.yml) | Weekly (Monday) | Automated NuGet, npm, and GitHub Actions security updates |

---

## Complete Documentation Index

| Category | Document | Description |
|----------|----------|-------------|
| **Operations** | [`docs/deploy.md`](docs/deploy.md) | Local, Docker Compose, production deployment & rollback guide |
| | [`docs/env-reference.md`](docs/env-reference.md) | Complete environment variable reference (names, scopes, formats) |
| | [`docs/release-checklist.md`](docs/release-checklist.md) | Pre-release verification checklist |
| | [`CONTRIBUTING.md`](CONTRIBUTING.md) | Conventional commit rules, PR workflow, review policy & folder ownership |
| **Architecture & API** | [`docs/backend/architecture-layers.md`](docs/backend/architecture-layers.md) | Modular monolith layers, domain events, outbox & resilience patterns |
| | [`docs/backend/api-endpoints.md`](docs/backend/api-endpoints.md) | `/api/v1` REST endpoint catalogue, pagination & error contracts |
| | [`docs/diagrams/diagrams.md`](docs/diagrams/diagrams.md) | Mermaid class, authentication/request sequence & deployment diagrams |
| | [`docs/postman/crisis-aid-platform.postman_collection.json`](docs/postman/crisis-aid-platform.postman_collection.json) | Postman collection (& [`crisis-aid-platform.postman_environment.json`](docs/postman/crisis-aid-platform.postman_environment.json)) |
| **Security** | [`docs/security/threat-model.md`](docs/security/threat-model.md) | STRIDE threat model & mitigations |
| | [`docs/security/role-endpoint-matrix.md`](docs/security/role-endpoint-matrix.md) | Deny-by-default Role × Endpoint authorisation matrix |
| | [`docs/security/secrets.md`](docs/security/secrets.md) | Secret management policy & rotation procedures |
| | [`docs/security/security-testing-report.md`](docs/security/security-testing-report.md) | Security verification report (271 passing security tests) |
| | [`docs/security/dependency-audit.md`](docs/security/dependency-audit.md) | NuGet & npm dependency vulnerability audit |
| **Database** | [`docs/database/schema-overview.md`](docs/database/schema-overview.md) | Core tables, relationships, concurrency & soft-delete design |
| | [`docs/database/er-diagram.md`](docs/database/er-diagram.md) | Entity-Relationship diagram |
| | [`docs/database/data-dictionary.md`](docs/database/data-dictionary.md) | Column-level types, constraints & sensitivity classification |
| | [`docs/database/migrations.md`](docs/database/migrations.md) | Ordered SQL migration & reference seed execution guide |
| | [`docs/database/connections.md`](docs/database/connections.md) | Connection strings, pooling & SQL Server provider notes |
| | [`docs/database/privacy-data-map.md`](docs/database/privacy-data-map.md) | Sri Lanka PDPA personal data map & retention policy |
| | [`docs/database/outbox-retention.md`](docs/database/outbox-retention.md) | Transactional outbox cleanup & retention schedule |
| | [`docs/database/bootstrap-admin.md`](docs/database/bootstrap-admin.md) | Initial administrator bootstrap procedure |
| | [`docs/database/backup-restore.md`](docs/database/backup-restore.md) | SQL Server backup & point-in-time recovery guide |
| **AI Governance** | [`docs/ai/overview.md`](docs/ai/overview.md) | Deterministic triage, PII redaction, Groq fallback & governance modes |
| | [`docs/ai/prompts.md`](docs/ai/prompts.md) | Prompt templates, schema validation & safety guardrails |
| | [`docs/ai/limitations-and-evaluation.md`](docs/ai/limitations-and-evaluation.md) | AI evaluation criteria, failure modes & human-override boundaries |
| **Frontend** | [`docs/frontend/structure.md`](docs/frontend/structure.md) | Next.js App Router areas, routes & `<RoleGuard />` behaviour |
| | [`docs/frontend/ui-guide.md`](docs/frontend/ui-guide.md) | Design tokens, WCAG AA accessibility, trilingual i18n & async UI states |
| **QA & Testing** | [`docs/testing/test-plan.md`](docs/testing/test-plan.md) | End-to-end test strategy, environments & defect triage |
| | [`docs/testing/coverage-table.md`](docs/testing/coverage-table.md) | Feature × test-layer coverage matrix |
| | [`docs/testing/newman-guide.md`](docs/testing/newman-guide.md) | Running automated API tests locally and in CI with Newman |
| | [`docs/testing/uat-form.md`](docs/testing/uat-form.md) | User Acceptance Testing scenarios (& [`uat-results-template.md`](docs/testing/uat-results-template.md)) |

---

## Team — Group 15

| # | Name | Student ID | GitHub | Area & Key Contributions |
|---|------|------------|--------|--------------------------|
| 1 | R.P.L Sathruwan | 39132 | [`@lasithasathruwan6-ctrl`](https://github.com/lasithasathruwan6-ctrl) | Architecture, Security, JWT/OTP Auth, RBAC Policy Matrix, NIC Encryption, Staff Invitations & Two-Admin Approvals |
| 2 | H.B.H Indusara | 39327 | [`@hasindu-indusara`](https://github.com/hasindu-indusara) | SQL Server Database Schema & Migrations, Requests & Case Management, Audit Triggers, Concurrency & PDPA Docs |
| 3 | Sasmitha Thejan | 39660 | [`@bandara331`](https://github.com/bandara331) | Explainable AI Triage, Counterfactuals, Copilot Drafts, Volunteer Matcher, Donation Anomaly Detection & Resources |
| 4 | E.W.S.P.D Dewmini | 39160 | [`@dimalsha-dewmini`](https://github.com/dimalsha-dewmini) | Next.js 15 Frontend, Accessibility (WCAG AA), Trilingual i18n (EN/SI/TA), Client Validation, Polling, Chat & Map UI |
| 5 | E.M.M.P.S Ekanayaka | 39318 | [`@Madhusha-Prageeth`](https://github.com/Madhusha-Prageeth) | Shelters & Volunteers, Transactional Outbox Processor, Resilience, Risk/Duplicate Detectors, Health & OpenAPI Docs |
| 6 | G.R.G Perera | 39055 | [`@gimhanarasidu38-create`](https://github.com/gimhanarasidu38-create) | QA & Integration Testing, Postman/Newman Suite, Docker Compose, GitHub Actions CI/CD, Mermaid Diagrams & Release Docs |

---

## Known Limitations

- **Manual Production Deployment:** GitHub Actions validates builds, linting, typechecking, and tests on every push/PR, while production container deployment remains manually gated via [`docs/release-checklist.md`](docs/release-checklist.md).
- **Heuristic Priority Scoring:** Crisis triage scoring uses deterministic, auditable rules rather than a machine-learning model trained on historical disaster datasets.
- **External Provider Availability:** Optional LLM summaries and live map tiles depend on external network reachability; both degrade gracefully (deterministic fallback templates on the backend and an accessible alert + shelter list fallback on the frontend).
- **SMTP Dependency for Live OTP Delivery:** Production OTP emails require an external SMTP/email provider configured via environment variables.
