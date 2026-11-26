# Coverage Table — Feature vs. Test Type

**Purpose:** Each platform feature is mapped to the test types that verify it, so gaps are visible at a glance.

Legend: ✅ covered · 🔲 planned · ➖ not applicable

---

## Backend Features

| Feature / Module | Unit (xUnit) | Integration (TestHost) | Postman / Newman | UAT |
|---|:---:|:---:|:---:|:---:|
| **Auth — register** | ➖ | 🔲 | ✅ | ✅ |
| **Auth — login + OTP verify** | ✅ OtpServiceTests | ✅ AuthFlowTests | ✅ | ✅ |
| **Auth — OTP resend cooldown** | ✅ OtpResendCooldownTests | ➖ | 🔲 | ➖ |
| **Auth — refresh token rotation** | ✅ RefreshTokenRotationTests | ✅ AuthFlowTests | ✅ | ➖ |
| **Auth — refresh token storage & logout** | ✅ RefreshTokenStorageAndLogoutTests | ✅ AuthFlowTests | ✅ | ➖ |
| **Auth — account lockout** | ✅ AccountLockoutTests | ➖ | ➖ | ➖ |
| **Auth — access token validation** | ✅ AccessTokenTests | ✅ AuthFlowTests | ✅ | ➖ |
| **Security — password policy** | ✅ PasswordPolicyTests | ➖ | ➖ | ➖ |
| **Security — NIC encryption** | ✅ NicHashTests, NicMaskingTests, NicProtectorTests, NicValidatorTests | ➖ | ➖ | ➖ |
| **Security — CORS** | ✅ CorsTests | ➖ | ➖ | ➖ |
| **Security — rate limiting** | ✅ RateLimitingTests | ➖ | ➖ | ➖ |
| **Security — security headers** | ✅ SecurityHeadersTests | ➖ | ➖ | ➖ |
| **Security — log redaction** | ✅ LogRedactionTests | ➖ | ➖ | ➖ |
| **Security — upload validation** | ✅ UploadValidatorTests | ➖ | ➖ | ➖ |
| **Admin — staff invitations** | ✅ StaffInvitationTests | ➖ | ➖ | ✅ |
| **Admin — role change approval (two-person)** | ✅ RoleChangeApprovalTests | ➖ | ➖ | ✅ |
| **Admin — policy matrix (deny-by-default)** | ✅ PolicyMatrixTests | ➖ | ➖ | ➖ |
| **Admin — ownership scoping** | ✅ OwnershipTests | ➖ | ➖ | ➖ |
| **Requests — create** | ➖ | ✅ AuthFlowTests | ✅ | ✅ |
| **Requests — view own (citizen)** | ➖ | ✅ AuthFlowTests | ✅ | ✅ |
| **Requests — assign + status update** | ➖ | ✅ AuthFlowTests | ✅ | ✅ |
| **Decisions — AI recommendation approve** | ➖ | ✅ AllocationCapacityTests | ✅ | ✅ |
| **Decisions — audit log entry** | ➖ | ✅ AllocationCapacityTests | ✅ | ✅ |
| **Allocations — capacity conflict (409)** | ➖ | ✅ AllocationCapacityTests | ✅ | ✅ |
| **Shelters — CRUD** | ➖ | 🔲 | 🔲 | ✅ |
| **Volunteers — register / list** | ➖ | 🔲 | 🔲 | ✅ |
| **Resources — CRUD** | ➖ | 🔲 | 🔲 | ✅ |
| **Donations — submit / list** | ➖ | 🔲 | 🔲 | ✅ |
| **Crises — public list** | ➖ | 🔲 | 🔲 | ✅ |
| **Notifications — per-user** | ➖ | 🔲 | 🔲 | ✅ |
| **Metrics — coordinator view** | ➖ | 🔲 | 🔲 | ✅ |

---

## Frontend Features

| Feature / Component | Vitest unit | Postman / UAT |
|---|:---:|:---:|
| RoleGuard component | ✅ role-guard.test.tsx | ✅ |
| SessionExpiredModal | ✅ session-expired-modal.test.tsx | ✅ |
| RequestChat | ✅ request-chat.test.tsx | ✅ |
| ShelterMap | ✅ shelter-map.test.tsx | ✅ |
| LoadingSkeleton | ✅ loading-skeleton.test.tsx | ➖ |
| StatusState | ✅ status-state.test.tsx | ➖ |
| IconButton ARIA | ✅ icon-button-aria.test.tsx | ➖ |
| i18n (en/si/ta) | ✅ i18n-si, i18n-ta, i18n-persistence | ✅ |
| Accessibility focus | ✅ accessibility-focus.test.tsx | ✅ |
| Colour contrast | ✅ contrast.test.ts | ✅ |
| Reduced motion | ✅ reduced-motion.test.ts | ✅ |
| Evidence upload validation | ✅ evidence-upload-validation.test.ts | ✅ |
| Contact validation | ✅ validation-contact.test.ts | ✅ |
| Lazy loading | ✅ lazy-loading.test.ts | ➖ |
| Responsive forms | ✅ responsive-forms.test.ts | ✅ |
| Responsive table | ✅ responsive-table.test.ts | ✅ |
| usePolling hook | ✅ use-polling.test.tsx | ➖ |
| Key components mount | ✅ key-components.test.tsx | ➖ |

---

## Notes

- Cells marked 🔲 are planned for future sprints and tracked as GitHub issues.
- Postman tests run automatically via Newman in CI (`backend.yml`).
- UAT results are recorded in `docs/testing/uat-results-template.md`.
