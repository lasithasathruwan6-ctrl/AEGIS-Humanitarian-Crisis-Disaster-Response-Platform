# Running the Postman Collection with Newman

[Newman](https://github.com/postmanlabs/newman) is the official CLI runner for Postman collections.
It lets you run the full API test suite without opening the Postman app — including in CI.

---

## Prerequisites

| Tool | Minimum version | Install |
|---|---|---|
| Node.js | 18 LTS | https://nodejs.org |
| Newman | 6.x | `npm install -g newman` |
| Newman HTML reporter (optional) | 3.x | `npm install -g newman-reporter-htmlextra` |

> [!IMPORTANT]
> The API must be running before you execute Newman.
> For local development, start it with `docker compose up --build` from the repo root (see `docker-compose.yml`),
> or with `dotnet run --project backend/CrisisPlatform.Api` and a local SQL Server instance.

---

## Quick Start

```bash
# From the repo root
newman run docs/postman/crisis-aid-platform.postman_collection.json \
  -e docs/postman/crisis-aid-platform.postman_environment.json \
  --env-var "baseUrl=http://localhost:8080"
```

The environment file ships with `baseUrl=http://localhost:8080`.
Override it with `--env-var` to point at a different host (e.g. a staging server).

---

## Running Individual Folders

```bash
# Auth flows only
newman run docs/postman/crisis-aid-platform.postman_collection.json \
  -e docs/postman/crisis-aid-platform.postman_environment.json \
  --folder "Auth"

# Request Lifecycle only
newman run docs/postman/crisis-aid-platform.postman_collection.json \
  -e docs/postman/crisis-aid-platform.postman_environment.json \
  --folder "Request Lifecycle"

# Decisions and audit
newman run docs/postman/crisis-aid-platform.postman_collection.json \
  -e docs/postman/crisis-aid-platform.postman_environment.json \
  --folder "Decisions & Audit"

# Capacity conflict (409)
newman run docs/postman/crisis-aid-platform.postman_collection.json \
  -e docs/postman/crisis-aid-platform.postman_environment.json \
  --folder "Conflict (409)"
```

---

## HTML Report

```bash
newman run docs/postman/crisis-aid-platform.postman_collection.json \
  -e docs/postman/crisis-aid-platform.postman_environment.json \
  --reporters cli,htmlextra \
  --reporter-htmlextra-export newman-report.html
```

Open `newman-report.html` in a browser to see a detailed per-request breakdown.

---

## Variables That Need Values Before a Full Run

Some tests depend on data created by earlier requests (e.g. `requestId`, `shelterId`).
For exploratory or manual runs, set these in the environment file or via `--env-var`:

| Variable | Where it comes from |
|---|---|
| `otpCode` | Email received after login — must be entered manually |
| `volunteerId` | Created in a prior test session or by a setup script |
| `shelterId` | Created in a prior test session or by a setup script |
| `recommendationId` | Returned after creating an AI recommendation |
| `staffAccessToken` | Run the Auth folder with a staff account first |
| `coordinatorAccessToken` | Run the Auth folder with a coordinator account first |
| `adminAccessToken` | Run the Auth folder with an admin account first |

> [!NOTE]
> The OTP flow requires email delivery.
> In automated CI runs (where email is unavailable), skip the OTP-gated requests
> or use a test mode that bypasses OTP (if the team enables one for the test environment).

---

## CI Integration

The backend GitHub Actions workflow (`.github/workflows/backend.yml`) is designed to run Newman
after the API starts. Add a step like this after the `dotnet run` step:

```yaml
- name: Start API in background
  run: dotnet run --project CrisisPlatform.Api &
  working-directory: backend

- name: Wait for API
  run: npx wait-on http://localhost:8080/healthz --timeout 30000

- name: Run Newman
  run: |
    npm install -g newman
    newman run docs/postman/crisis-aid-platform.postman_collection.json \
      -e docs/postman/crisis-aid-platform.postman_environment.json \
      --env-var "baseUrl=http://localhost:8080" \
      --folder "Auth" \
      --folder "Conflict (409)"
```

> [!TIP]
> Run only the folders that do not depend on a live email server in CI.
> The Auth folder (up to and including Login) and the Conflict (409) folder are good candidates.

---

## Helper Script

A convenience shell script is provided at `scripts/run-newman.sh`.
It wraps the full Newman invocation with sensible defaults.

```bash
# Run all folders against localhost:8080
bash scripts/run-newman.sh

# Run against a different host
BASE_URL=https://staging.example.com bash scripts/run-newman.sh
```
