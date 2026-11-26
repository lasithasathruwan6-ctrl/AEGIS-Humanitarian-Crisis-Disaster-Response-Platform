# Backend Architecture Layers

**Project:** Crisis Aid Platform — ASP.NET Core Modular-Monolith API  
**Stack:** .NET 10, C#, SQL Server, xUnit

---

## Layer Overview

```
┌──────────────────────────────────────────────────────────────┐
│                   HTTP / TLS (port 443)                      │
├──────────────────────────────────────────────────────────────┤
│  API Layer  (CrisisPlatform.Api)                             │
│  • Middleware: Security headers, CORS, Rate-limiting         │
│  • Controllers / Minimal-API endpoints (Modules/)            │
│  • Request validation (FluentValidation)                     │
│  • Problem-details error handling                            │
│  • Swagger / OpenAPI documentation                           │
├──────────────────────────────────────────────────────────────┤
│  Application Layer  (services inside each Module)            │
│  • Use-case orchestration (command / query handlers)         │
│  • Allocation strategies (MaxUrgency, FairShare, FastestEta) │
│  • Notification factory (English / Sinhala / Tamil)          │
│  • Domain-event dispatcher                                   │
│  • Idempotency-key de-duplication                            │
│  • Outbox pattern with exponential-backoff retry             │
├──────────────────────────────────────────────────────────────┤
│  Domain Layer  (entities, value objects, domain events)      │
│  • Pure C# — zero infrastructure dependencies                │
│  • Entities: Request, Shelter, Resource, Volunteer …         │
│  • Domain events raised on aggregate state changes           │
│  • District risk-index calculation                           │
│  • Duplicate-detection rules                                 │
├──────────────────────────────────────────────────────────────┤
│  Infrastructure Layer  (adapters)                            │
│  • Repository implementations (EF Core + IRepository<T>)     │
│  • Unit of Work (IUnitOfWork wrapping DbContext.SaveChanges) │
│  • External HTTP clients: Open-Meteo, Routing, Email         │
│  • Resilience: Polly timeouts, retries, circuit-breaker      │
│  • Health checks: database, AI service, email feed           │
│  • Identity / JWT token services                             │
│  • Outbox table + background worker                          │
└──────────────────────────────────────────────────────────────┘
```

---

## Module Structure

Each feature is a self-contained module folder under `Modules/`:

```
Modules/
├── Auth/          → OTP, tokens, lockout
├── Admin/         → staff invitations, role-change approvals
├── Requests/      → citizen aid requests, idempotency, outbox
├── Shelters/      → shelter CRUD, capacity tracking
├── Resources/     → resource allocation strategies
├── Volunteers/    → volunteer matching
├── Notifications/ → notification factory, email sender
├── Mapping/       → routing client, ETA estimation
├── AI/            → AI-service client, district risk index
├── Crises/        → crisis lifecycle, duplicate detection
├── Donations/     → donation records
├── Partners/      → partner organisation management
└── Metrics/       → operational dashboards
```

Each module contains:
- `*Service.cs` — application-layer use cases
- `*Controller.cs` (or Minimal API map group) — HTTP surface
- `DTOs/` — request/response shapes
- `Validators/` — FluentValidation validators

---

## Design Patterns

### Repository Pattern
> **Interface:** `IRepository<T>` defined in the Domain layer.  
> **Implementation:** EF Core `Repository<T>` in Infrastructure.  
> Controllers and services depend only on the interface; the concrete class is registered in DI.

### Unit of Work
> **Interface:** `IUnitOfWork` exposes `CommitAsync()`.  
> **Implementation:** wraps `DbContext.SaveChangesAsync()` so a single database round-trip persists all changes made in one use-case execution.

### Factory Pattern
> **`NotificationFactory`** selects the correct message template for each locale (English, Sinhala, Tamil) and assembles a `Notification` object without coupling callers to locale logic.

### Strategy Pattern
> **`IAllocationStrategy`** is implemented by `MaxUrgencyStrategy`, `FairShareStrategy` and `FastestEtaStrategy`. The active strategy is resolved at runtime from configuration.

### Outbox Pattern
> Domain events are written to an `OutboxMessage` table in the same transaction as the originating command. A background worker reads unprocessed messages, publishes them, and marks them delivered — guaranteeing at-least-once delivery with idempotency-key deduplication.

---

## Cross-cutting Concerns

| Concern | Implementation |
|---|---|
| Authentication | Short-lived JWT + rotating refresh tokens |
| Authorisation | Deny-by-default role policies + ownership scope |
| Validation | FluentValidation on every DTO |
| Error responses | RFC 9457 Problem Details (`application/problem+json`) |
| Idempotency | `Idempotency-Key` header on mutating endpoints |
| Resilience | Polly: timeout → retry (exp. backoff) → circuit-breaker |
| Observability | Structured logs (log-redaction for PII), health-check endpoints |
| Security headers | CSP, HSTS, X-Content-Type-Options, referrer policy |

---

## Request Lifecycle

```
Client
  → TLS → Security-Headers middleware
  → CORS middleware
  → Rate-limiter
  → JWT auth middleware
  → Authorisation middleware
  → Controller / Minimal-API endpoint
      → FluentValidation (400 on failure)
      → Application Service (use-case)
          → Domain logic
          → Repository (read / write)
          → Unit of Work (commit)
          → Outbox (async events)
      → 200 / 201 / 204 response
  ← Problem Details (4xx / 5xx) on error
```
