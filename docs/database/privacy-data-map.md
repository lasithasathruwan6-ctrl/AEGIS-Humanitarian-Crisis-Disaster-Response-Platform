# Privacy Data Map & PDPA Retention Notes

This document maps all personal data stored in `CrisisPlatform` and defines technical safeguards, retention schedules, and alignment with the **Sri Lanka Personal Data Protection Act (PDPA, No. 9 of 2022)**.

---

## 1. Personal Data Inventory

| Table.Column | Data Category | Protection Control | Purpose / Lawful Basis |
|---|---|---|---|
| `Users.FullName` | Identity | Access-controlled (`OwnerScope` + role policies) | Account identification & coordination |
| `Users.Email` | Contact / Identity | Unique index, log-redacted | Authentication, OTP delivery, staff invitations |
| `Users.EncryptedNic` | National ID (Sensitive) | AES-256-GCM encrypted at rest (`Security:NicEncryptionKey`); masked in UI/API | Citizen verification during disaster relief |
| `Users.NicHash` | National ID Fingerprint | Keyed HMAC-SHA256 (`Security:NicHashKey`, separate from encryption key) | Duplicate-account prevention without plaintext storage |
| `AidRequests.ContactPhone` | Contact | Masked/redacted in application logs (`LogRedaction`) | Emergency responder outreach |
| `AidRequests.Latitude`, `Longitude` | Location | Scoped to assigned staff/volunteers (`OwnerScope`) | Dispatch & shelter routing |
| `AidRequests.Description` | Free-text | Redacted by `PiiRedactor` before external AI (`Groq`) calls | Triage & aid allocation |
| `RequestMessages.Body` | Communications | Authenticated thread participants only | Citizen–staff coordination |
| `Donations.DonorId` | Financial Attribution | Protected by `ON DELETE NO ACTION` | Financial auditability & receipts |
| `AuditEntries.PerformedBy` | Security Audit | Immutable (`INSTEAD OF` triggers + SHA-256 hash chain) | Accountability & incident forensics |

---

## 2. Retention & Disposal Schedule

| Record Type | Active Retention | Post-Retention Action | Notes |
|---|---|---|---|
| `OtpChallenges` | 10 minutes (TTL) | Purge expired/consumed rows daily | Only HMAC-SHA256 `CodeHash` is stored |
| `RefreshTokens` | 7 days (configurable) | Purge expired/revoked rows after 30 days | Rotating tokens; revocation tracked |
| `OutboxMessages` (Delivered) | 30 days | Batched deletion (`cleanup-old-outbox.sql`) | Dead-lettered rows retained for investigation |
| `AidRequests` (Closed) | 2 years post-crisis | Soft-delete (`DeletedAt`), anonymise `ContactPhone` & `Description` | Retain aggregate district/need statistics |
| `Users` (Deactivated) | 90 days grace | Soft-delete (`DeletedAt`), wipe `EncryptedNic`, rotate/null `NicHash` | Filtered unique index frees NIC for re-registration |
| `AuditEntries` | 7 years | Append-only archive (never updated/deleted in-place) | Required for compliance & non-repudiation |

---

## 3. PDPA Alignment Notes

- **Data Minimisation:** Only fields strictly required for emergency response (`District`, `NeedType`, `Urgency`, `ContactPhone`) are collected on `AidRequests`.
- **Purpose Limitation & AI Guardrails:** `PiiRedactor` strips NIC numbers, phone numbers, and emails before any prompt leaves the boundary to external LLM providers.
- **Storage Limitation:** Ephemeral security artifacts (`OtpChallenges`, `RefreshTokens`, sent `OutboxMessages`) have automated expiry/cleanup windows.
- **Integrity & Confidentiality:** NICs use dual-key separation (AES-256-GCM encryption + separate HMAC-SHA256 uniqueness key) and strict role-based + ownership-scoped access control.
