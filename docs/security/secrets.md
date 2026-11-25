# Secrets Handling

## Rules

1. No secret is committed: not in code, `appsettings.json`, scripts, docs, tests or issue text.
2. Real values live only in user-secrets (development), environment variables or the host's secret store (production).
3. `.env.example` lists every variable name with an empty value. It is the only `.env*` file tracked by git.
4. A secret that has ever been pushed is treated as leaked: rotate it first, then remove it from the code.

## What counts as a secret

| Name (configuration key) | Purpose | Format |
|---|---|---|
| `Auth:Tokens:SigningKey` | Signs access tokens | at least 32 bytes; the API refuses to start otherwise |
| `Security:NicEncryptionKey` | AES-256-GCM key for NIC at rest | base64 of exactly 32 bytes |
| `Security:NicHashKey` | Keyed hash that makes NIC unique | base64, at least 32 bytes |
| `Security:OtpHashKey` | Keyed hash for OTP codes | base64, at least 32 bytes |
| `ConnectionStrings:Default` | SQL Server login | connection string |
| `Smtp:Password` | Email delivery | string |

`Cors:AllowedOrigin`, token lifetimes and rate limits are configuration, not secrets, and may stay in `appsettings.json`.

Using one key for several purposes defeats the separation, so each key above must be a different random value.

## Local development

```powershell
dotnet user-secrets init --project backend/CrisisPlatform.Api
dotnet user-secrets set "Auth:Tokens:SigningKey" "<48+ random characters>" --project backend/CrisisPlatform.Api
```

Generate random values with `openssl rand -base64 32` or in PowerShell:
`[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))`.

## What git ignores

`.gitignore` blocks `.env`, `.env.*` (except `.env.example`), every `appsettings.<Environment>.json`, `secrets.json`, certificates and private keys (`*.pem`, `*.key`, `*.pfx`, `*.p12`, `*.jks`, `*.crt`) and `id_rsa*`.
Check a path with `git check-ignore -v <path>`.

## Before every commit

```powershell
git diff --staged
```

Look for keys, passwords, connection strings, tokens, NIC numbers, emails and phone numbers. Test data must be obviously fake and only in the test project.

## Status of this repository

A scan of the working tree and of the full git history (file names and `password=`, `secret`, `token`, `apikey`, `connectionstring`, private-key patterns) found no committed secrets as of the date of the security-testing report.
`appsettings.json` contains only the non-secret CORS origin, rate limit and logging settings.

## If a secret leaks

1. Rotate it immediately (new key, new password, revoke tokens).
2. Update user-secrets / the host secret store.
3. Tell the security owner; history rewriting is a team decision because it affects everyone's clones.
4. For NIC encryption keys, plan a re-encryption of stored data before retiring the old key.
