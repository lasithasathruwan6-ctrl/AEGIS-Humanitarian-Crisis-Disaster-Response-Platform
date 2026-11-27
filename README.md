# AEGIS — Humanitarian Crisis & Disaster Response Platform

**SE205.3 – Software Architecture | Group 15 | NSBM Green University**

**AEGIS** is a full-stack, trilingual (English, Sinhala, Tamil) humanitarian crisis coordination platform built for Sri Lanka's 25 districts. It connects citizens, emergency coordinators, shelter administrators, logistics teams, volunteers, and donors under one core principle:

> **AI only recommends, scores, drafts, or flags. Named, authenticated human staff always decide, approve, send, and publish — with every critical action sealed in a SHA-256 hash-chained audit ledger.**

---

## Live Deployment & Submission Links

| Resource | URL |
| --- | --- |
| **Live Web Application (Vercel)** | https://aegis-platform-seven.vercel.app/ |


---

## System Architecture & Tech Stack

| Layer | Technology | Location |
| --- | --- | --- |
| **Frontend** | Next.js 15 (App Router), React 19, TypeScript, Leaflet, OpenStreetMap | `./src` |
| **Backend API** | ASP.NET Core Web API (.NET 10, C#), FluentValidation, Serilog | `./aegis-api` |
| **Database** | Supabase PostgreSQL (Npgsql, `aegis` schema, 44 tables) & local SQL Server | `AegisDbContext` |
| **AI & NLU** | Rule-based explainable heuristic engine + Groq Cloud API (facts-only, PII-redacted) | `RuleBasedAiProvider`, `GroqMultilingualNluService`, `AiCopilotService` |
| **Weather & Flood Data** | Open-Meteo Forecast & Flood APIs | `PredictionService`, `WeatherAlertService` |
| **Route Optimisation** | Google OR-Tools (`9.15.6755`) + OSRM / Google Routes (Haversine fallback) | `OrToolsDispatchOptimiser`, `OsrmRoutingService` |
| **Notifications** | Transactional Outbox + Brevo / Resend HTTPS email API | `NotificationOutboxWorker`, `SmtpEmailSender` |
| **Hosting** | Vercel (Frontend), Railway Docker (API + Workers), Supabase (PostgreSQL) | `aegis-api/Dockerfile` |

### Clean Architecture (`aegis-api/src`)
- **`Aegis.Domain`** — `BaseEntity`, 30+ domain entities, enums (`AegisRoles`, `RequestStatus`, `RequestPriority`, `DecisionAction`), `Result<T>`, domain events, and repository/UoW interfaces.
- **`Aegis.Application`** — Service interfaces, DTOs, FluentValidation validators, Strategy implementations (`MaxUrgencyStrategy`, `FairShareStrategy`, `FastestEtaStrategy`), and Factories (`NotificationFactory`, `RecommendationFactory`).
- **`Aegis.Infrastructure`** — EF Core persistence (`AegisDbContext`), PostgreSQL migrations, `AuditService` (SHA-256 hash chain), `NicCryptoService` (AES-256-GCM + HMAC-SHA256), `PiiRedactor`, `LocalFileStorage`, and background workers (`NotificationOutboxWorker`, `WeatherAlertEvaluationWorker`).
- **`Aegis.Api`** — Thin REST controllers under `/api/v1`, JWT authentication, policy-based RBAC (`StaffOnly`, `CanDecide`, `AuditAccess`, `AdminOnly`), rate limiting, security headers, and Swagger.

---

## Installation & Configuration

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20 LTS](https://nodejs.org/) & npm
- Supabase PostgreSQL connection string (Session Pooler, port `5432`, TLS required) or local SQL Server

### 1. Backend API (`aegis-api`)
Configure secrets with `dotnet user-secrets` (never commit real secrets to Git):
```powershell
cd aegis-api/src/Aegis.Api
dotnet user-secrets set "ConnectionStrings:Default" "<supabase-session-pooler-connection-string>"
dotnet user-secrets set "Jwt:Key" "<strong-32+-char-jwt-secret>"
dotnet user-secrets set "NicCrypto:EncryptionKey" "<base64-32-byte-aes-key>"
dotnet user-secrets set "NicCrypto:HmacKey" "<base64-32-byte-hmac-key>"
dotnet user-secrets set "Groq:ApiKey" "<optional-groq-api-key>"
dotnet user-secrets set "GoogleMaps:ApiKey" "<optional-google-routes-key>"
```

Apply EF Core migrations (or run `supabase/migrations/20261116000000_aegis_schema.sql`), bootstrap the first `SystemAdmin`, and start the API:
```powershell
dotnet ef database update --project aegis-api/src/Aegis.Infrastructure --startup-project aegis-api/src/Aegis.Api
dotnet run --project aegis-api/src/Aegis.Api -- bootstrap-admin --email admin@aegis.lk --full-name "System Admin"
dotnet run --project aegis-api/src/Aegis.Api
```

### 2. Frontend (`./src`)
Create `.env.local` in the repository root (see `.env.example`):
```env
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000/api/v1
```
Install dependencies and run:
```powershell
npm install
npm run dev
```
Open `http://localhost:3000`.

### 3. Environment Variables Reference
- **Backend (`aegis-api`):** `ConnectionStrings__Default`, `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, `NicCrypto__EncryptionKey`, `NicCrypto__HmacKey`, `Groq__ApiKey`, `Groq__Model`, `GoogleMaps__ApiKey`, `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password`, `Smtp__FromEmail`, `Channel__Secret`, `Cors__FrontendOrigin`.
- **Frontend:** `NEXT_PUBLIC_API_BASE_URL`, `NEXT_PUBLIC_GOOGLE_MAPS_API_KEY` (optional).

---

## Test Credentials (For Evaluators)

> **Note:** Use these demo accounts on the live deployment (`https://aegis-platform-seven.vercel.app/`). Staff sign in via `/staff/login`; citizens sign in via `/login`.

| Role | Login Page | Email | Password |
| --- | --- | --- | --- |
| **System Admin / Staff** | `/staff/login` | `[ENTER DEMO STAFF EMAIL]` | `[ENTER DEMO STAFF PASSWORD]` |
| **Citizen** | `/login` | `[ENTER DEMO CITIZEN EMAIL]` | `[ENTER DEMO CITIZEN PASSWORD]` |

*(New citizens can also register at `/register` using a valid Sri Lankan NIC format and a real email address to receive the 6-digit OTP.)*

---

## API Information

- **Base Path:** `/api/v1`
- **Swagger / OpenAPI (Development):** `/swagger`
- **Health Check:** `GET /api/v1/health` and `GET /health`
- **Authentication:** Short-lived JWT Bearer access tokens (15 min) + rotating SHA-256 hashed refresh tokens. Next.js proxies tokens in an `httpOnly`, `SameSite=Lax` cookie (`aegis_token`).

### Main Endpoint Groups
| Module | Base Route | Key Operations |
| --- | --- | --- |
| **Auth & Profile** | `/api/v1/auth`, `/api/v1/account` | Register (NIC + OTP), verify email, citizen/staff login, refresh, logout, profile |
| **Requests & Cases** | `/api/v1/requests`, `/api/v1/cases` | Create (idempotent), track, chat, upload evidence, cancel, feedback, duplicate merge |
| **AI & Governance** | `/api/v1/ai`, `/api/v1/admin/ai` | Recommendations, human decision (`Approve/Modify/Reject/Defer`), mode (`Active/Shadow/Off`), analytics, copilot, stock forecast, district risk |
| **Alerts** | `/api/v1/alerts` | Official alerts, automated weather drafts, approve/retract/false-alarm, dry-run, signed unsubscribe |
| **Logistics** | `/api/v1/shelters`, `/api/v1/resources`, `/api/v1/scenarios` | Shelter Overpass import & occupancy (`409 Conflict` guard), stock dispatch, shortage simulation |
| **Dispatch** | `/api/v1/dispatch` | Vehicle fleet management, OR-Tools route preview, human approval (2-person rule for critical stops) |
| **Volunteers & Donations** | `/api/v1/volunteers`, `/api/v1/campaigns`, `/api/v1/donations` | Volunteer registration, verification, skill/distance matching, campaign & anomaly review |
| **Audit Ledger** | `/api/v1/audit` | Paginated audit entries, `GET /api/v1/audit/verify` (SHA-256 chain integrity check) |

### Postman Collection
- Collection file: [`docs/postman/aegis.postman_collection.json`](docs/postman/aegis.postman_collection.json)
- Verified against the live Railway deployment with **29 requests across 8 folders** — **35 / 35 assertions passed** (mean response time `987 ms`).

---

## Testing & Verification

```powershell
# Backend unit tests (71 / 71 passed)
dotnet test aegis-api/tests/Aegis.UnitTests/Aegis.UnitTests.csproj

# Backend integration tests (1 active passed; Testcontainers test skipped without local Docker)
dotnet test aegis-api/tests/Aegis.IntegrationTests/Aegis.IntegrationTests.csproj

# Frontend typecheck and production build (48 / 48 routes, zero errors)
npm run typecheck
npm run build
```

---

## Known Limitations & Future Work

- **Heuristic Triage Scoring:** Priority scoring uses deterministic developer-defined rules rather than a trained ML model; no formal accuracy benchmark was conducted on historical disaster datasets.
- **External Service Dependence:** Weather advisories and NLU depend on Open-Meteo and Groq availability (mitigated with stale-data guards, numeric validation, and fixed trilingual template fallbacks).
- **Single-Instance Realtime Signals:** `InMemoryRealtimeSignalPublisher` (SSE) and `AiRateLimiter` run in-process; horizontal multi-replica scaling requires a shared Redis backplane.
- **Ephemeral Evidence Storage:** `LocalFileStorage` stores uploaded evidence under `App_Data/evidence`, requiring a persistent volume or object storage (e.g., S3 / Supabase Storage) in stateless container hosts.
- **Excluded Channels (Future Work):** Native mobile apps, Sri Lankan SMS gateway integration, Web Push notifications, and offline-first volunteer sync are planned for future iterations.

---

## Documentation Index
- [`docs/architecture.md`](docs/architecture.md) — Clean Architecture layers, design patterns, and event flow
- [`docs/database.md`](docs/database.md) — Supabase PostgreSQL schema `aegis`, constraints, concurrency, and RLS
- [`docs/security.md`](docs/security.md) — JWT/OTP auth, 9-role RBAC matrix, NIC encryption, and AI safety controls
- [`docs/deploy.md`](docs/deploy.md) — Vercel, Docker (OR-Tools `libgomp1`), Supabase pooler, and hardening checklist

---

## Team — Group 15

| # | Name | Student ID | GitHub | Key Area |
| --- | --- | --- | --- | --- |
| 1 | R.P.L Sathruwan | 39132 | `@lasithasathruwan6-ctrl` | Team lead, architecture, security, JWT/RBAC, audit ledger |
| 2 | H.B.H Indusara | 39327 | `@hasindu-indusara` | Database design, requests, case management, human-in-the-loop |
| 3 | Sasmitha Thejan | 39660 | `@bandara331` | AI scoring, governance, duplicates, anomaly detection, resources |
| 4 | E.W.S.P.D Dewmini | 39160 | `@dimalsha-dewmini` | Notifications, outbox/workers, mapping, routing, report |
| 5 | E.M.M.P.S Ekanayaka | 39318 | `@Madhusha-Prageeth` | Shelters (concurrency safety), volunteers, UI evidence |
| 6 | G.R.G Perera | 39055 | `@gimhanarasidu38-create` | Citizen & donor portals, campaigns, API & QA testing |
