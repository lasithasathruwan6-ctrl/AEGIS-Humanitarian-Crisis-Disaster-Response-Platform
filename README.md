# Crisis Aid Platfo

**SE205.3 Software Architecture Coursework 2026**
AI-Powered Humanitarian Crisis Management and Aid Coordination Platform

A multi-organisation platform for coordinating emergency assistance, shelters, volunteers, resources, donations and AI-assisted (human-approved) decision-making during disasters.

## Tech Stack
| Layer | Technology |
|---|---|
| Backend | ASP.NET Core Web API (C#, .NET 10) |
| Database | SQL Server |
| Frontend | Next.js 15 (App Router, React 19, TypeScript) |
| API Docs | Swagger / OpenAPI |
| Architecture | Modular Monolith |

## Repository Structure
```
/backend    ASP.NET Core solution
/frontend   Web client
/database   SQL scripts (schema, seed data)
/docs       Report, diagrams, Postman collection
```

## Team
| Member | Name | GitHub | Area |
|---|---|---|---|
| 1 | Lasitha Sathruwan | @lasithasathruwan6-ctrl | Architecture, Security, Auth, Admin |
| 2 | Hasindu Indusara| @hasindu-indusara | Database, Requests, Human-in-the-loop |
| 3 | Sasmitha Thejan| @bandara331| AI services, Resources |
| 4 | Dimalsha Dewmini| @dimalsha-dewmini| Notifications, Mapping, Async |
| 5 | Prageeth Ekanayake| @Madhusha-Prageeth| Shelters, Volunteers |
| 6 | Rasindu Gimhana|@gimhanarasidu38-create | Citizens, Donations, QA |

## Prerequisites
- .NET 10 SDK
- SQL Server (+ SSMS)
- Node.js (LTS)
- Git

## Getting Started
```bash
git clone https://github.com/lasithasathruwan6-ctrl/crisis-aid-platform.git
cd crisis-aid-platform
git checkout -b feature/m<number>-<topic>
```

### Backend
```bash
dotnet run --project backend/CrisisPlatform.Api
```
Swagger UI: https://localhost:<port>/swagger *(update when ready)*

### Frontend
```bash
cd frontend
npm install
npm run dev
```
- **Checks & Tests:** `npm run check` (ESLint + `tsc --noEmit`), `npm test` (Vitest + Testing Library), `npm run build`
- **Documentation:** [docs/frontend/structure.md](docs/frontend/structure.md) (areas, routes, role guards) and [docs/frontend/ui-guide.md](docs/frontend/ui-guide.md) (design tokens, accessibility, i18n, responsive & async patterns)

## Configuration
Secrets are never committed. Use `dotnet user-secrets` or `appsettings.Development.json` (git-ignored). See [.env.example](.env.example) and [docs/security/secrets.md](docs/security/secrets.md).

## Security
- **Threat Model & Matrix:** [docs/security/threat-model.md](docs/security/threat-model.md) and [docs/security/role-endpoint-matrix.md](docs/security/role-endpoint-matrix.md)
- **Controls Implemented:** Security headers, strict single-origin CORS, auth rate limiting, password policy, hashed OTP with cooldown, short-lived JWTs + rotating/revokable refresh tokens, account lockout, AES-256-GCM NIC encryption + masking + keyed uniqueness hash, deny-by-default role policies + ownership scoping, single-use 24h staff invitations, two-person role change approval, log redaction, and upload signature validation.
- **Verification & Audits:** [docs/security/security-testing-report.md](docs/security/security-testing-report.md) (271 passing tests) and [docs/security/dependency-audit.md](docs/security/dependency-audit.md).

## Test Credentials
*(dummy accounts per role to be added)*

## API Information
Base URL: `/api/v1`. Swagger documents all endpoints. Postman collection in `/docs`.

## Git Workflow
- No direct pushes to `main`; use feature branches + Pull Requests.
- Branch naming: `feature/m<number>-<topic>`.
- Every member commits with their own GitHub account.

## Known Limitations
*(to be updated before submission)*
