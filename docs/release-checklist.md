# Release Checklist — Crisis Aid Platform

Use this checklist for every release. Check each item before tagging and deploying.
Record who signed off and the date in the sign-off table at the bottom.

---

## 1. Code Quality

- [ ] All CI checks green on the commit to be released (`backend.yml`, `frontend.yml`).
- [ ] `dotnet test` passes locally with 0 failures.
- [ ] `npm test` (Vitest) passes locally with 0 failures.
- [ ] `npm run lint` and `npm run typecheck` produce 0 errors/warnings.
- [ ] `npm run build` succeeds (Next.js production build).
- [ ] No `TODO`, `FIXME` or `HACK` comments introduced in this release without an issue reference.

---

## 2. Security

- [ ] `git diff --staged` (or the PR diff) contains no secrets, keys, connection strings or personal data.
- [ ] No `.env` file, `appsettings.Development.json` or `secrets.json` committed.
- [ ] All new endpoints are covered by the role-endpoint matrix (`docs/security/role-endpoint-matrix.md`).
- [ ] Any new NuGet or npm dependency has been reviewed for known vulnerabilities (`dotnet list package --vulnerable`, `npm audit`).
- [ ] Dependabot PRs for the release period have been reviewed and merged or deferred with justification.
- [ ] NIC data is never returned in clear text in any new API response.

---

## 3. Database

- [ ] Any schema changes have a corresponding SQL script in `database/`.
- [ ] The SQL script has been tested on a clean database.
- [ ] A backup of the production/staging database has been taken before applying the migration.

---

## 4. Configuration & Secrets

- [ ] All environment variables in `docs/env-reference.md` are set in the target environment.
- [ ] Secrets have been rotated if any were suspected of leaking.
- [ ] `Cors:AllowedOrigin` is set to the correct production/staging URL (not `localhost`).
- [ ] `ASPNETCORE_ENVIRONMENT` is set to `Production` (not `Development`).

---

## 5. Testing

- [ ] Newman run against staging passed (all folders, 0 failures).
- [ ] UAT completed and results recorded in `docs/testing/uat-results-template.md`.
- [ ] All P1 UAT items passed (or a waiver has been approved by the QA Lead).
- [ ] No open P1 bugs in the GitHub issue tracker.

---

## 6. Documentation

- [ ] `README.md` quick-start steps are accurate for this version.
- [ ] `docs/deploy.md` reflects any infrastructure changes.
- [ ] `docs/env-reference.md` is up to date (new variables added, removed ones deleted).
- [ ] `docs/testing/coverage-table.md` updated if new features were added.
- [ ] API Swagger docs reviewed — no placeholder or stale descriptions.

---

## 7. Git & Versioning

- [ ] Release commit is on `main` and all team members have reviewed and merged their PRs.
- [ ] `git log --oneline` for the release range reviewed — no empty or WIP commits.
- [ ] Git tag created: `git tag -a v<major>.<minor>.<patch> -m "Release v<version>"`
- [ ] Tag pushed: `git push origin v<major>.<minor>.<patch>`

---

## 8. Deployment

- [ ] Docker image built and tagged: `docker build -f backend/Dockerfile -t crisis-aid-api:<version> backend/`
- [ ] Image pushed to registry (if applicable).
- [ ] `docker compose up` (or equivalent) confirmed healthy on staging before production.
- [ ] Health endpoint (`GET /healthz`) returns 200 after deploy.
- [ ] Reverse proxy is forwarding to the new container.
- [ ] Browser smoke test: login, create a request, view it — no console errors.

---

## 9. Post-Deployment

- [ ] Monitor error logs for 30 minutes after deployment.
- [ ] Notify the team (GitHub Discussion or agreed channel) that the release is live.
- [ ] Update the GitHub Release page with the changelog.
- [ ] Archive the UAT results document.

---

## Sign-off

| Step | Signed off by | Date |
|------|--------------|------|
| Code quality | | |
| Security review | | |
| Database migration | | |
| Config & secrets | | |
| Testing (Newman + UAT) | | |
| Documentation | | |
| Git tag | | |
| Deployment confirmed | | |

**Release version:** _______________  
**Git tag:** _______________  
**Deployment date:** _______________
