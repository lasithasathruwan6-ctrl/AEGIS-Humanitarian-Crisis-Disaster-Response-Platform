# Security Testing Report

Test run date: 2026-11-25
Test command: `dotnet test CrisisPlatform.slnx --nologo -v q`
Result: **271 passed, 0 failed, 0 skipped** (`CrisisPlatform.Tests.dll`, `net10.0`).

## Implemented controls and test coverage

| Area | Implementation | Test file |
|---|---|---|
| Security headers | `SecurityHeadersMiddleware` (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Content-Security-Policy`, `Permissions-Policy`, `Cross-Origin-Resource-Policy`) | `SecurityHeadersTests.cs` |
| CORS | `CorsExtensions.AddFrontendCors` (single configured origin, no wildcards, fail-fast on invalid config) | `CorsTests.cs` |
| Rate limiting | `AuthRateLimiting` (per-client fixed window on `POST /api/v1/auth/login` and `/register`) | `RateLimitingTests.cs` |
| Password policy | `PasswordPolicy` (12–128 chars, upper/lower/digit/symbol, common password & email-name checks) | `PasswordPolicyTests.cs` |
| Email OTP | `OtpService` (HMAC-SHA256 hash at rest, 10-min expiry, 5-attempt limit, 60s resend cooldown) | `OtpServiceTests.cs`, `OtpResendCooldownTests.cs` |
| Access & refresh tokens | `AccessTokenService` (15-min JWT, max 30-min guard), `RefreshTokenService` (SHA-256 hash, rotation, replay family revocation, logout revocation) | `AccessTokenTests.cs`, `RefreshTokenRotationTests.cs`, `RefreshTokenStorageAndLogoutTests.cs` |
| Account lockout | `AccountLockoutService` (locks for 15 min after 5 failures, per-account, resets on success) | `AccountLockoutTests.cs` |
| NIC protection | `NicValidator`, `NicProtector` (AES-256-GCM + tamper detection), `NicMasker` / `MaskedNicJsonConverter`, `NicHasher` + `NicUniquenessService` | `NicValidatorTests.cs`, `NicProtectorTests.cs`, `NicMaskingTests.cs`, `NicHashTests.cs` |
| Authorisation & ownership | `AuthorizationExtensions` (deny-by-default fallback, role policies), `OwnerScope` & `OwnerOrStaffHandler` | `PolicyMatrixTests.cs`, `OwnershipTests.cs` |
| Staff invitations | `StaffInvitationService` (SHA-256 hashed token, single-use, 24-hour expiry, excludes `Admin`) | `StaffInvitationTests.cs` |
| Role change approval | `RoleChangeService` (four-eyes approval, requester and target cannot approve, 72-hour expiry) | `RoleChangeApprovalTests.cs` |
| Log redaction | `LogRedactor` & `RedactingLoggerProvider` (redacts emails, NICs, phones, JWTs, auth headers, secrets, drops scopes) | `LogRedactionTests.cs` |
| Upload validation | `UploadValidator` (PDF/JPEG/PNG allowlist, extension + content-type + magic-byte signature, size & filename checks) | `UploadValidatorTests.cs` |

## Documentation

- [Threat Model (STRIDE)](threat-model.md)
- [Role vs Endpoint Matrix](role-endpoint-matrix.md)
- [Secrets Handling](secrets.md)
- [Dependency Audit](dependency-audit.md)

## Known gaps / next integration steps

- Security services (`OtpService`, `RefreshTokenService`, `AccountLockoutService`, `NicUniquenessService`, `StaffInvitationService`, `RoleChangeService`) define repository/store interfaces (`IOtpStore`, `IRefreshTokenStore`, `ILockoutStore`, `INicIndex`, `IInvitationStore`, `IRoleChangeStore`) that still need SQL Server implementations and API endpoint wiring.
- `frontend/` has 5 npm audit advisories (`postcss`, `tinypool`, `@vitest/mocker`) requiring major-version upgrades by the frontend owners.
