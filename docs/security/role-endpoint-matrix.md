# Role versus Endpoint Matrix

Base path: `/api/v1`. Policy is **deny by default**: an endpoint with no entry here requires an authenticated Administrator.
Endpoints are the planned module surface and must be kept in sync with the code and Swagger.

Roles: **Anon** (not signed in), **Citizen**, **Volunteer**, **Staff** (partner/organisation staff), **Coordinator**, **Admin**.

Legend: ✔ allowed, ○ allowed for own records only, ✖ forbidden.

| Endpoint | Anon | Citizen | Volunteer | Staff | Coordinator | Admin |
|----------|:----:|:-------:|:---------:|:-----:|:-----------:|:-----:|
| `POST /auth/register` | ✔ | ✖ | ✖ | ✖ | ✖ | ✖ |
| `POST /auth/login`, `/auth/otp/verify`, `/auth/otp/resend` | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `POST /auth/refresh`, `/auth/logout` | ✖ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `POST /auth/accept-invitation` (valid token) | ✔ | ✖ | ✖ | ✖ | ✖ | ✖ |
| `GET /crises` (public list) | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `POST/PUT /crises` | ✖ | ✖ | ✖ | ✖ | ✔ | ✔ |
| `POST /requests` | ✖ | ✔ | ✖ | ✖ | ✖ | ✖ |
| `GET /requests/{id}`, `/requests/{id}/messages` | ✖ | ○ | assigned | ✔ | ✔ | ✔ |
| `GET /requests` (all) | ✖ | ✖ | ✖ | ✔ | ✔ | ✔ |
| `PUT /requests/{id}/status`, `/assign` | ✖ | ✖ | ✖ | ✔ | ✔ | ✔ |
| `GET /shelters` | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `POST/PUT /shelters` | ✖ | ✖ | ✖ | ✔ | ✔ | ✔ |
| `GET/POST /volunteers` | ✖ | ✖ | ○ | ✔ | ✔ | ✔ |
| `GET/POST/PUT /resources` | ✖ | ✖ | ✖ | ✔ | ✔ | ✔ |
| `POST /donations` | ✖ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `GET /donations` (all) | ✖ | ✖ | ✖ | ✖ | ✔ | ✔ |
| `GET /notifications` | ✖ | ○ | ○ | ○ | ○ | ○ |
| `POST /ai/recommendations/{id}/approve` (human-in-the-loop) | ✖ | ✖ | ✖ | ✖ | ✔ | ✔ |
| `GET /metrics` | ✖ | ✖ | ✖ | ✖ | ✔ | ✔ |
| `POST /admin/staff-invitations` | ✖ | ✖ | ✖ | ✖ | ✖ | ✔ |
| `POST /admin/role-changes` (request) | ✖ | ✖ | ✖ | ✖ | ✖ | ✔ |
| `POST /admin/role-changes/{id}/approve` (must differ from requester) | ✖ | ✖ | ✖ | ✖ | ✖ | ✔ |
| `GET /admin/audit-log` | ✖ | ✖ | ✖ | ✖ | ✖ | ✔ |

## Rules

1. Authentication: JWT bearer access token; short lifetime, refresh token rotated on use.
2. "Own records" is enforced by comparing the token subject with the record owner in the query, not only in the controller.
3. NIC is never returned in clear text; responses show a masked value.
4. Role changes need two different Admin accounts (requester and approver).
5. This matrix is the source for the policy-matrix tests; a new endpoint must be added here in the same change.
