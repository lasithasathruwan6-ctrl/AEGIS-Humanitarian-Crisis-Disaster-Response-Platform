# Dependency Vulnerability Audit

Audit date: 2026-11-25

## Backend (.NET 10)

Command run:

```powershell
dotnet list CrisisPlatform.slnx package --vulnerable --include-transitive
dotnet list CrisisPlatform.slnx package --outdated
```

### Results

| Project | Vulnerable packages | Notes |
|---|---|---|
| `CrisisPlatform.Api` | 0 | Transitive packages included (`Microsoft.AspNetCore.OpenApi` 10.0.14, `Microsoft.IdentityModel.JsonWebTokens` 8.23.0) |
| `CrisisPlatform.Tests` | 0 | Transitive packages included (`Microsoft.AspNetCore.TestHost` 10.0.14, `Microsoft.NET.Test.Sdk` 18.12.0, `xunit` 2.9.3, `xunit.runner.visualstudio` 4.0.1) |

`dotnet list package --outdated` lists `11.0.0` for `Microsoft.AspNetCore.OpenApi` and `Microsoft.AspNetCore.TestHost`. Those target `net11.0` and are not compatible with the solution's `net10.0` target framework, so no backend package updates were applied.

## Frontend (Next.js / Node)

Command run in `frontend/`:

```powershell
npm.cmd audit
```

### Results

`npm audit` reported **5 vulnerabilities** (2 moderate, 1 high, 2 critical), all requiring major-version breaking upgrades (`npm audit fix --force`):

| Package | Severity | Advisories | Dependency path | Suggested fix |
|---|---|---|---|---|
| `postcss` (`<=8.5.22`) | High | GHSA-qx2v-qp2m-jg93, GHSA-6g55-p6wh-862q, GHSA-fxqj-rqcc-2cmp, GHSA-r28c-9q8g-f849 | `next` (15.3.0) | `next@16.4.6` (breaking) |
| `tinypool` (`<=2.1.1`) | Critical | GHSA-5gmw-xhrv-c9v3, GHSA-85c8-ppgw-ccpr | `vitest` (3.1.2, devDependency) | `vitest@5.1.2` (breaking) |
| `@vitest/mocker` (`<=4.1.11`) | Moderate | GHSA-82fw-gwwq-j7x9, GHSA-qxg6-89mg-rqv8 | `vitest` (3.1.2, devDependency) | `vitest@5.1.2` (breaking) |

### Action taken

No `frontend/` files were modified here because `frontend/` is outside the backend security area and every reported fix is a breaking major upgrade (`next@16.4.6`, `vitest@5.1.2`). The frontend owners should evaluate upgrading `next` and `vitest` in a dedicated frontend PR with full UI and test verification.
