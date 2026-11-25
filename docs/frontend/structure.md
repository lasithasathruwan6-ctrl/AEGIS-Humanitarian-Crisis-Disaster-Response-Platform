# Frontend Structure: Areas, Routes, and Guards

## 1. Overview
The Crisis Aid Platform web client is a Next.js (App Router, TypeScript) application located in `/frontend`. It communicates with the ASP.NET Core Modular Monolith backend at `/api/v1` using JWT bearer authentication and role-based access control aligned with [role-endpoint-matrix.md](../security/role-endpoint-matrix.md).

## 2. Directory Layout
```text
frontend/
├── src/
│   ├── app/                  # Next.js App Router pages & layouts
│   │   ├── (public)/         # Unauthenticated & shared public routes
│   │   ├── citizen/          # Citizen portal (requests, donations, profile)
│   │   ├── volunteer/        # Volunteer portal (assignments, availability)
│   │   ├── staff/            # Staff & Coordinator operations
│   │   └── admin/            # System administration & audit
│   ├── components/           # Shared UI, guards, map, chat, forms, states
│   ├── hooks/                # Custom hooks (polling, auth, i18n)
│   ├── i18n/                 # Translation dictionaries (en, si, ta) & provider
│   ├── lib/                  # API client, validation, auth helpers
│   └── styles/               # Unified design tokens (tokens.css)
└── tests/                    # Vitest + Testing Library unit/component tests
```

## 3. Functional Areas & Routes

| Area | Route | Purpose | Allowed Roles |
|---|---|---|---|
| Public | `/` | Crisis overview, shelter directory, quick navigation | Anon, All authenticated |
| Public | `/login` | Email/password login + OTP verification | Anon |
| Public | `/register` | Citizen registration | Anon |
| Public | `/accept-invitation` | Staff/Coordinator/Admin invitation acceptance | Anon (valid token) |
| Public | `/shelters` | Public shelter list and map view | Anon, All authenticated |
| Citizen | `/citizen` | Citizen dashboard (my requests, notifications) | Citizen |
| Citizen | `/citizen/requests/new` | Submit assistance request with evidence upload | Citizen |
| Citizen | `/citizen/requests/[id]` | Request details & real-time chat with responder | Citizen (owner) |
| Citizen | `/citizen/donations` | Record and view own donations | Citizen, Volunteer, Staff, Coordinator, Admin |
| Volunteer | `/volunteer` | Assigned tasks and availability status | Volunteer |
| Staff | `/staff` | Triage queue, request assignment, status updates | Staff, Coordinator, Admin |
| Staff | `/staff/shelters` | Manage shelters and capacity | Staff, Coordinator, Admin |
| Staff | `/staff/resources` | Manage relief resources and inventory | Staff, Coordinator, Admin |
| Staff | `/staff/ai-review` | Human-in-the-loop AI recommendation approval | Coordinator, Admin |
| Admin | `/admin` | Staff invitations, role-change requests & approvals | Admin |
| Admin | `/admin/audit-log` | System audit log viewer | Admin |

## 4. Route & Role Guards
Route protection is enforced on the client via `<RoleGuard allowedRoles={[...]}>` (and verified on every API call by the backend):

1. **Unauthenticated (`Anon`)**: Accessing any protected route (`/citizen/*`, `/volunteer/*`, `/staff/*`, `/admin/*`) redirects to `/login?next=<encoded-path>`.
2. **Authenticated Role Mismatch**: If the signed-in user's role is not in `allowedRoles`, `<RoleGuard>` renders an `AccessDenied` state or redirects to the user's home area (`/citizen`, `/volunteer`, `/staff`, or `/admin`).
3. **401 Session Expiry**: Any `/api/v1` response with HTTP `401` triggers a global session-expired event that displays a non-dismissible **Session Expired** modal prompting re-authentication without losing context.
4. **Two-Admin Role Change Rule**: `/admin` enforces UI feedback when the current Admin attempts to approve their own role-change request (backend requires two distinct Admin accounts).
