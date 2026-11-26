# UAT Form — Crisis Aid Platform

**Version:** 1.0  
**Date:** _______________  
**Tester name:** _______________  
**Tester role:** _______________  
**Environment tested:** ☐ Local  ☐ Staging  ☐ Production  
**Browser / device:** _______________

---

> [!IMPORTANT]
> Complete every section in order. Mark each test case with the result.
> Record the exact error message or screenshot reference for any **FAIL**.
> Sign off at the bottom when all P1 items pass.

---

## Legend

| Mark | Meaning |
|------|---------|
| ✅ PASS | Behaviour matches the expected result exactly |
| ❌ FAIL | Behaviour differs — record details in the Notes column |
| ⏭ SKIP | Cannot be tested in this session — record reason |

---

## 1. Authentication

| # | Test case | Steps | Expected result | Result | Notes |
|---|-----------|-------|-----------------|--------|-------|
| 1.1 | Register new citizen | Navigate to `/register`, fill name, email, NIC and password, submit | Account created; OTP email sent | | |
| 1.2 | Login with valid credentials | Navigate to `/login`, enter email and password | OTP prompt appears | | |
| 1.3 | OTP verify | Enter the OTP from the email | Redirect to citizen dashboard | | |
| 1.4 | OTP resend within cooldown | Click Resend within 60 s | Error message shown; no new email | | |
| 1.5 | Incorrect OTP | Enter wrong OTP 3 times | Account temporarily locked; appropriate message | | |
| 1.6 | Refresh session | Leave session idle until access token expires; perform an action | Session refreshed silently | | |
| 1.7 | Logout | Click Logout | Redirected to login; refresh token invalidated | | |
| 1.8 | Access protected page without login | Navigate directly to `/citizen/requests` | Redirect to `/login` | | |

---

## 2. Citizen — Request Lifecycle

| # | Test case | Steps | Expected result | Result | Notes |
|---|-----------|-------|-----------------|--------|-------|
| 2.1 | Submit aid request | Log in as Citizen; click New Request; fill type, description, location; submit | Request created; status = `Pending` | | |
| 2.2 | View own request | Navigate to My Requests | Only own requests listed | | |
| 2.3 | Citizen cannot view another citizen's request | Try to access `/citizen/requests/<other-id>` | 403 or redirect | | |
| 2.4 | Attach evidence | Upload a valid image to a request | File accepted | | |
| 2.5 | Attach invalid file type | Upload an executable file | Error shown; file rejected | | |
| 2.6 | Request status update visible | Staff updates status; citizen refreshes | Status reflects update | | |

---

## 3. Staff — Request Management

| # | Test case | Steps | Expected result | Result | Notes |
|---|-----------|-------|-----------------|--------|-------|
| 3.1 | View all requests | Log in as Staff; navigate to Requests | Full list visible | | |
| 3.2 | Assign request to volunteer | Select a request; assign a volunteer | Status = `Assigned`; volunteer notified | | |
| 3.3 | Update request status | Change status to `InProgress` then `Resolved` | Status updates reflected | | |
| 3.4 | View request messages / audit trail | Open a request; view chat/messages | Messages and timestamps shown | | |

---

## 4. Coordinator — Decisions & AI

| # | Test case | Steps | Expected result | Result | Notes |
|---|-----------|-------|-----------------|--------|-------|
| 4.1 | View AI recommendation | Log in as Coordinator; open a request with a recommendation | Recommendation card shown | | |
| 4.2 | Approve recommendation | Click Approve; add a note | Decision recorded; audit entry created | | |
| 4.3 | Reject recommendation | Click Reject; add a reason | Decision recorded; status = `Rejected` | | |
| 4.4 | Coordinator cannot approve own staff invitation | Attempt self-approval action | Action blocked | | |

---

## 5. Shelter & Allocation

| # | Test case | Steps | Expected result | Result | Notes |
|---|-----------|-------|-----------------|--------|-------|
| 5.1 | View public shelter list | Navigate to `/shelters` without login | List visible with capacity info | | |
| 5.2 | Add shelter (staff) | Log in as Staff; create a shelter with capacity 10 | Shelter appears in list | | |
| 5.3 | Allocate within capacity | Allocate 5 occupants to the shelter | Allocation accepted | | |
| 5.4 | Allocation conflict (409) | Allocate 999 occupants | Error message; no allocation | | |

---

## 6. Admin Functions

| # | Test case | Steps | Expected result | Result | Notes |
|---|-----------|-------|-----------------|--------|-------|
| 6.1 | Send staff invitation | Log in as Admin; send invite to a new email | Invitation email sent | | |
| 6.2 | Accept invitation | Click link in invitation email | Account created with Staff role | | |
| 6.3 | Request role change | Admin A initiates role change for a user | Pending approval state | | |
| 6.4 | Approve role change (two-person) | Admin B (different account) approves | Role updated; audit entry created | | |
| 6.5 | Self-approval blocked | Admin A tries to approve their own role-change request | Error shown | | |
| 6.6 | View audit log | Navigate to Admin › Audit Log | All significant actions listed with actor and timestamp | | |

---

## 7. Accessibility & i18n

| # | Test case | Steps | Expected result | Result | Notes |
|---|-----------|-------|-----------------|--------|-------|
| 7.1 | Keyboard navigation | Tab through all interactive elements on the login page | Focus order is logical; all controls reachable | | |
| 7.2 | Screen reader | Use a screen reader on the citizen dashboard | All buttons and form fields have accessible labels | | |
| 7.3 | Switch language to Sinhala | Select සිංහල in the language picker | UI text changes to Sinhala | | |
| 7.4 | Switch language to Tamil | Select தமிழ் in the language picker | UI text changes to Tamil | | |
| 7.5 | Reduced motion | Enable OS reduced-motion setting; reload | Animations suppressed | | |

---

## 8. Security Spot Checks

| # | Test case | Steps | Expected result | Result | Notes |
|---|-----------|-------|-----------------|--------|-------|
| 8.1 | NIC never returned in clear text | Submit a request and inspect the API response | NIC shown as masked value (e.g. `***5678`) | | |
| 8.2 | Security headers present | Open DevTools › Network; check any API response | `X-Content-Type-Options`, `X-Frame-Options`, `Strict-Transport-Security` present | | |
| 8.3 | CORS blocked from wrong origin | Send request from a different origin | CORS error; request blocked | | |
| 8.4 | Rate limit on login | Submit login 6 times within 60 s | 429 response on the 6th attempt | | |

---

## Sign-off

| Criterion | Status |
|-----------|--------|
| All P1 test cases (1.x, 2.1, 5.4, 8.x) pass | ☐ |
| No open FAIL items without an agreed workaround | ☐ |
| Results recorded in `uat-results-template.md` | ☐ |

**Tester signature:** _______________ **Date:** _______________  
**QA Lead sign-off:** _______________ **Date:** _______________
