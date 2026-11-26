# Contributing to Crisis Aid Platform

Thank you for contributing! This guide covers commit style, the review process and folder ownership.

---

## Table of Contents

1. [Getting started](#1-getting-started)
2. [Commit style](#2-commit-style)
3. [Branch and PR workflow](#3-branch-and-pr-workflow)
4. [Code review](#4-code-review)
5. [Folder ownership](#5-folder-ownership)
6. [Testing requirements](#6-testing-requirements)
7. [Security rules](#7-security-rules)
8. [Documentation rules](#8-documentation-rules)

---

## 1. Getting started

```bash
git clone https://github.com/lasithasathruwan6-ctrl/crisis-aid-platform.git
cd crisis-aid-platform
cp .env.example .env     # fill in secrets — never commit this file
```

Confirm prerequisites: `.NET 10 SDK`, `Node.js 20 LTS`, `Docker`, `Git`.  
See [`docs/deploy.md`](docs/deploy.md) for the full local development setup.

---

## 2. Commit style

We use **Conventional Commits** (https://www.conventionalcommits.org).

### Format

```
<type>(<optional scope>): <short description>

[optional body — wrap at 72 characters]

[optional footer: BREAKING CHANGE, Closes #<issue>]
```

### Allowed types

| Type | When to use |
|------|-------------|
| `feat` | New feature visible to end users |
| `fix` | Bug fix |
| `test` | Adding or updating tests only |
| `docs` | Documentation only |
| `refactor` | Code change that is neither a fix nor a feature |
| `chore` | Build process, CI, dependencies, config files |
| `perf` | Performance improvement |
| `style` | Formatting (no logic change) — use sparingly |

### Rules

- **Subject line:** imperative mood, no capital first letter, no trailing period, ≤ 72 characters.
- **One logical change per commit.** Split unrelated changes across separate commits.
- **No empty commits.** No whitespace-only or message-only commits.
- **No mass-generated filler.** Every commit must contain a real, reviewable change.
- **No secrets.** Run `git diff --staged` before every commit and check for keys, passwords, NIC numbers, emails or connection strings.
- **Build must pass** before you push. Run `dotnet build` and `npm run build` locally.

### Examples

```
feat(requests): add evidence attachment endpoint

POST /requests/{id}/evidence now accepts image/jpeg and image/png up
to 5 MB. Upload is validated by signature (magic bytes) before storage.

Closes #42
```

```
fix(auth): reject refresh tokens after account lockout

The lockout check was applied only to the login endpoint. This change
propagates it to the refresh endpoint so that locked accounts cannot
silently extend their session.
```

```
chore: add Dependabot configuration for NuGet and npm
```

---

## 3. Branch and PR workflow

1. **Branch from `main`:**
   ```bash
   git checkout main
   git pull origin main
   git checkout -b feature/m<number>-<short-topic>
   ```
   Example: `feature/m6-integration-tests`

2. **Keep the branch short-lived.** Rebase on `main` regularly:
   ```bash
   git fetch origin
   git rebase origin/main
   ```

3. **Open a Pull Request** against `main` when your work is ready for review.
   - Title follows the commit style: `feat(module): short description`.
   - Body: what changed, why, how to test it.
   - Link to any related issue.

4. **Do not force-push** to a PR branch after review has started without notifying the reviewer.

5. **Squash or rebase** before merge to keep `main` history clean. No merge commits.

---

## 4. Code review

### Reviewer responsibilities

- Review within **48 hours** of the PR being opened.
- Check that the commit message describes the actual change.
- Verify that new code has tests (see §6).
- Check the diff for secrets (§7).
- Leave actionable feedback. Approve only when all blockers are resolved.

### Author responsibilities

- Address every comment or explain why it was not actioned.
- Do not merge your own PR.
- Two-person rule for role-change approvals (enforced in code — also enforced in PR review).

---

## 5. Folder ownership

Each team member owns their area. Only the owner (or the owner's explicit approval) should change files in that area. Cross-area changes must be discussed first.

| Area | Owner | Member |
|------|-------|--------|
| `backend/` architecture, security, auth, admin | Lasitha Sathruwan | 1 |
| `backend/` database layer, requests, human-in-the-loop | Hasindu Indusara | 2 |
| `backend/` AI services, resources | Sasmitha Thejan | 3 |
| `backend/` notifications, mapping, async | Dimalsha Dewmini | 4 |
| `backend/` shelters, volunteers | Prageeth Ekanayake | 5 |
| `docs/testing/`, `docs/diagrams/`, `docs/deploy.md`, `docs/env-reference.md`, `docs/release-checklist.md`, `CONTRIBUTING.md`, `.github/workflows/`, `docker-compose.yml`, `backend/Dockerfile`, `docs/postman/` | Rasindu Gimhana | 6 |
| `frontend/` | Members 4–6 (shared) | — |
| `docs/security/` | Member 1 (primary) | — |
| `docs/frontend/` | Member 5 (primary) | — |
| `database/` | Member 2 (primary) | — |

> [!NOTE]
> If you need to change a file outside your area (e.g. to wire up a new module in `Program.cs`),
> open the PR and tag the area owner as a required reviewer.

---

## 6. Testing requirements

- Every new service or endpoint must ship with **at least one test**.
- Backend unit tests go in `backend/CrisisPlatform.Tests/Security/` (security) or the appropriate subfolder.
- Backend integration tests go in `backend/CrisisPlatform.Tests/Integration/`.
- Frontend tests go in `frontend/tests/`.
- Test data fixtures may only exist inside the test project — never in the application.
- No fake/demo users, sample records or mock modes in the application code.

See [`docs/testing/test-plan.md`](docs/testing/test-plan.md) and [`docs/testing/coverage-table.md`](docs/testing/coverage-table.md).

---

## 7. Security rules

1. **No secret is ever committed** — not in code, config, scripts, docs or test files.
2. Run `git diff --staged` before every commit. If you see a key, password, connection string or NIC number, abort and rotate the value.
3. `.env` and `appsettings.Development.json` are git-ignored. Only `.env.example` is tracked.
4. A secret that has been pushed is treated as leaked: rotate it immediately.
5. All new endpoints must be added to `docs/security/role-endpoint-matrix.md` in the same change.

Full rules: [`docs/security/secrets.md`](docs/security/secrets.md).

---

## 8. Documentation rules

- Update `docs/env-reference.md` when you add or remove an environment variable.
- Update `docs/security/role-endpoint-matrix.md` when you add or change an endpoint.
- Update `docs/testing/coverage-table.md` when you add a new feature.
- Keep `README.md` accurate. If the quick-start steps change, update them in the same PR.
- Diagrams as code live in `docs/diagrams/`. Update the relevant diagram when the architecture changes.
