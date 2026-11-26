# Test Plan — Crisis Aid Platform

**Version:** 1.0  
**Date:** 2026-11-26  
**Author:** Member 6 — Rasindu Gimhana (QA / Testing)  
**Status:** Active

---

## 1. Scope

This plan covers all automated and manual testing activities for the Crisis Aid Platform across the backend (.NET 10 / ASP.NET Core), the frontend (Next.js 15 / Vitest) and the API surface (Postman / Newman).

**In scope**

| Layer | What is tested |
|---|---|
| Backend unit tests | Security services: tokens, OTP, lockout, NIC, password policy, CORS, rate limiting, role policies, audit |
| Backend integration tests | Full HTTP round-trips through TestHost: auth flow, request lifecycle, allocation capacity, decision approval |
| Frontend unit tests | Components, hooks, i18n, accessibility, validation, responsive behaviour |
| API / Postman | Auth flows, request lifecycle, decisions, audit verification, conflict (409) |
| UAT | Manual acceptance against role matrix, documented in `uat-form.md` |

**Out of scope**

- Performance / load testing (tracked separately)
- Third-party services (SMTP, external map tiles)
- Data migration scripts

---

## 2. Test Strategy

### 2.1 Testing Pyramid

```
         ┌─────────────────────┐
         │   UAT / Postman     │  ← manual + Newman CI gate
         ├─────────────────────┤
         │  Integration Tests  │  ← TestHost, in-memory
         ├─────────────────────┤
         │     Unit Tests      │  ← xUnit (backend), Vitest (frontend)
         └─────────────────────┘
```

### 2.2 Environments

| Environment | Purpose | Who runs it |
|---|---|---|
| Local | Developer self-check | Each member |
| CI (GitHub Actions) | Build + unit + integration gate on every push/PR | Automated |
| UAT | Manual exploratory test before release | QA lead + stakeholders |

### 2.3 Failure Policy

- A failing unit or integration test **blocks merge** to `main`.
- A new endpoint or service must ship with at least one test.
- Flaky tests are quarantined within one working day and tracked as issues.

---

## 3. Backend Test Organisation

```
backend/CrisisPlatform.Tests/
└── Security/          ← existing unit tests (Member 1 security suite)
Integration/           ← added by Member 6
├── AuthFlowTests.cs
├── AllocationCapacityTests.cs
└── Helpers/
    └── TestDataBuilders.cs
```

Framework: **xUnit 2.9** + **Microsoft.AspNetCore.TestHost 10**.  
No real database. Services that need a store use in-memory fakes (`StoreFakes.cs`, `TestDoubles.cs`).

---

## 4. Frontend Test Organisation

```
frontend/tests/
├── accessibility-focus.test.tsx
├── contrast.test.ts
├── evidence-upload-validation.test.ts
├── i18n-*.test.ts
├── key-components.test.tsx
├── lazy-loading.test.ts
├── loading-skeleton.test.tsx
├── reduced-motion.test.ts
├── request-chat.test.tsx
├── responsive-*.test.ts
├── role-guard.test.tsx
├── session-expired-modal.test.tsx
├── shelter-map.test.tsx
├── status-state.test.tsx
├── use-polling.test.tsx
└── validation-contact.test.ts
```

Framework: **Vitest 3 + Testing Library**.  
Run: `npm test` from `frontend/`.

---

## 5. API / Postman Test Organisation

Collection: `docs/postman/crisis-aid-platform.postman_collection.json`  
Environment: `docs/postman/crisis-aid-platform.postman_environment.json` (no secrets)

Folders in the collection:

| Folder | Description |
|---|---|
| Auth | Register, login, OTP verify, refresh, logout |
| Request Lifecycle | Create, view own, assign, update status |
| Decisions & Audit | AI recommendation approve, audit log entry |
| Conflict (409) | Allocation exceeds shelter capacity |

Newman CI gate: `.github/workflows/backend.yml` runs `newman run …` after the API starts.

---

## 6. Coverage Targets

| Layer | Target line/branch coverage |
|---|---|
| Backend security services | ≥ 90 % line, ≥ 80 % branch |
| Backend integration paths | All happy paths + key error paths |
| Frontend components | All rendered components have ≥ 1 test |

Detailed feature × test-type mapping: [`coverage-table.md`](coverage-table.md).

---

## 7. Roles and Responsibilities

| Role | Person | Responsibility |
|---|---|---|
| QA Lead | Member 6 | Test plan, Postman, integration tests, UAT coordination |
| Members 1–5 | Individual | Unit tests for their own modules |
| All | All | Fix failing tests before merging |

---

## 8. Entry / Exit Criteria

### Entry (start testing)
- Code builds with no errors.
- At least one test exists for each security service.

### Exit (ready to release)
- All CI checks green.
- UAT sign-off recorded in `uat-results-template.md`.
- No open P1 bugs.

---

## 9. Test Execution Schedule

| Milestone | Activity | Date |
|---|---|---|
| Sprint end | Full CI run (unit + integration) | Rolling |
| Pre-release | Newman run against staging | Before every release |
| Submission | UAT with recorded results | ≥ 2 days before submission |

---

## 10. Defect Management

Defects are filed as GitHub Issues with label `bug`. Priority:

| Priority | SLA |
|---|---|
| P1 – blocking | Fix before next merge |
| P2 – functional gap | Fix within current sprint |
| P3 – cosmetic | Backlog |
