# Backend API Endpoints

**Base URL:** `/api/v1` (plus `/health` and `/openapi/v1.json`)  
**Error format:** RFC 9457 Problem Details (`application/problem+json`)  
**Auth:** Bearer JWT (`Authorization: Bearer <token>`)

---

## Requests (`/api/v1/requests`)

| Method | Path | Description | Query / Headers | Responses |
|---|---|---|---|---|
| `POST` | `/api/v1/requests` | Submit a new aid request | Header: `Idempotency-Key` (required) | `201 Created`, `400`, `401`, `403`, `409` |
| `GET` | `/api/v1/requests` | List aid requests (paginated) | `district`, `page` (default 1), `pageSize` (default 20, max 100) | `200 OK` (`PagedResult<RequestDto>`), `400`, `401`, `403`, `409` |
| `GET` | `/api/v1/requests/{id}` | Retrieve a single aid request | Route: `id` (`guid`) | `200 OK`, `404 Not Found` |

### Example `POST /api/v1/requests`
```json
{
  "nicHash": "a1b2c3d4e5f67890abcdef1234567890",
  "district": "Colombo",
  "needType": "Water",
  "urgency": 4,
  "description": "Clean drinking water needed for 15 displaced residents.",
  "contactPhone": "0771234567"
}
```

---

## Health & Observability

| Method | Path | Description | Responses |
|---|---|---|---|
| `GET` | `/health` | Standard ASP.NET Core readiness probe | `200 Healthy`, `503 Unhealthy` |
| `GET` | `/health/services` | `ServiceHealthReport` for `database`, `ai`, and `email` | `200 OK` (Healthy/Degraded), `503` (Unhealthy) |
| `GET` | `/openapi/v1.json` | OpenAPI 3.x spec with JWT Bearer scheme, examples & error codes | `200 OK` |

---

## Standard Error Codes

| Status | Meaning |
|---|---|
| `400 Bad Request` | Validation failure (`CreateRequestValidator`) or missing `Idempotency-Key` header |
| `401 Unauthorized` | Missing, expired, or invalid JWT access token |
| `403 Forbidden` | Authenticated user lacks required role or ownership scope |
| `409 Conflict` | `Idempotency-Key` reused with a different payload or invalid state transition |
| `500 Internal Server Error` | Unhandled exception (sanitized Problem Details with `traceId`) |
