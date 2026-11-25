# Threat Model (STRIDE)

Scope: Crisis Aid Platform API (ASP.NET Core, SQL Server) and its web client.
Status: design baseline; controls marked *planned* are tracked in the security commit plan.

## Main assets

| ID | Asset | Why it matters |
|----|-------|----------------|
| A1 | Citizen identity data (NIC, phone, email) | Personal data; NIC is a national identifier |
| A2 | Credentials and sessions (passwords, OTPs, access/refresh tokens) | Account takeover |
| A3 | Aid requests and conversations | Contain locations and needs of people in crisis |
| A4 | Staff and admin accounts, role assignments | Privileged operations |
| A5 | Resource, shelter and donation records | Integrity drives real-world aid allocation |
| A6 | Secrets (DB connection string, NIC encryption/hash keys, JWT signing key, SMTP) | Compromise defeats all other controls |
| A7 | Audit log | Accountability for sensitive actions |

## STRIDE analysis

| Asset | Threat | Category | Mitigation | Test / evidence |
|-------|--------|----------|------------|-----------------|
| A2 | Credential stuffing, brute-forcing login | Spoofing | Rate limiting on login/registration, account lockout, password policy | rate-limit, lockout, password tests |
| A2 | OTP guessing or replay | Spoofing | OTP hashed at rest, short expiry, attempt limit, resend cooldown | OTP tests |
| A2 | Stolen refresh token reuse | Spoofing | Rotation, hashed storage, revoke on logout | token tests |
| A1 | Citizen reads another citizen's NIC/requests | Information disclosure | Ownership checks, NIC masked in responses | authorisation and masking tests |
| A1 | Database leak exposes NIC | Information disclosure | AES-GCM encryption, keyed hash for lookup/uniqueness | NIC tests |
| A3 | Tampering with another user's request | Tampering | Ownership/role policies on every write | policy matrix tests |
| A4 | Citizen calls staff endpoints (privilege escalation) | Elevation of privilege | Role-based authorisation policies, deny by default | policy matrix test |
| A4 | Single admin grants themselves or others elevated roles | Elevation of privilege | Role change needs a different second approver; invitations single-use, 24h | approval and invitation tests |
| A4 | Admin denies having changed a role | Repudiation | Audit log of role changes with actor and approver | audit entries |
| A7 | Log entries altered or deleted | Tampering / Repudiation | Append-only audit table, restricted DB permissions | *planned* |
| A6 | Secret committed to git | Information disclosure | `.gitignore`, `.env.example`, user-secrets, secret scanning | docs/security/secrets.md |
| A5 | Malicious upload (disguised executable, oversized file) | Tampering / DoS | Type, size and file-signature validation | upload tests |
| All | Browser-based attacks (clickjacking, MIME sniffing, hostile origin) | Tampering / Information disclosure | Security headers, CORS restricted to configured frontend | header and CORS tests |
| All | Request flooding | Denial of service | Rate limiting, request size limits | rate-limit tests |
| A1, A6 | PII or secrets in logs | Information disclosure | Log redaction, never log NIC/OTP/tokens | logging test |

## Out of scope

Infrastructure hardening (network, OS, SQL Server patching), physical security, and third-party AI service internals.
