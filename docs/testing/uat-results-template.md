# UAT Results — Crisis Aid Platform

**Version under test:** _______________  
**Git commit / tag:** _______________  
**Test date:** _______________  
**Environment:** ☐ Local  ☐ Staging  ☐ Production  
**QA Lead:** _______________

---

## Summary

| Metric | Count |
|--------|-------|
| Total test cases | 40 |
| ✅ Passed | |
| ❌ Failed | |
| ⏭ Skipped | |
| **Overall result** | ☐ PASS  ☐ FAIL  ☐ CONDITIONAL PASS |

---

## Results by Section

### 1. Authentication

| # | Test case | Result | Defect ref | Notes |
|---|-----------|--------|------------|-------|
| 1.1 | Register new citizen | | | |
| 1.2 | Login with valid credentials | | | |
| 1.3 | OTP verify | | | |
| 1.4 | OTP resend within cooldown | | | |
| 1.5 | Incorrect OTP / lockout | | | |
| 1.6 | Refresh session | | | |
| 1.7 | Logout | | | |
| 1.8 | Access protected page without login | | | |

### 2. Citizen — Request Lifecycle

| # | Test case | Result | Defect ref | Notes |
|---|-----------|--------|------------|-------|
| 2.1 | Submit aid request | | | |
| 2.2 | View own request | | | |
| 2.3 | Citizen cannot view another's request | | | |
| 2.4 | Attach evidence (valid) | | | |
| 2.5 | Attach invalid file type | | | |
| 2.6 | Request status update visible | | | |

### 3. Staff — Request Management

| # | Test case | Result | Defect ref | Notes |
|---|-----------|--------|------------|-------|
| 3.1 | View all requests | | | |
| 3.2 | Assign request to volunteer | | | |
| 3.3 | Update request status | | | |
| 3.4 | View request messages / audit trail | | | |

### 4. Coordinator — Decisions & AI

| # | Test case | Result | Defect ref | Notes |
|---|-----------|--------|------------|-------|
| 4.1 | View AI recommendation | | | |
| 4.2 | Approve recommendation | | | |
| 4.3 | Reject recommendation | | | |
| 4.4 | Cannot self-approve | | | |

### 5. Shelter & Allocation

| # | Test case | Result | Defect ref | Notes |
|---|-----------|--------|------------|-------|
| 5.1 | View public shelter list | | | |
| 5.2 | Add shelter (staff) | | | |
| 5.3 | Allocate within capacity | | | |
| 5.4 | Allocation conflict (409) | | | |

### 6. Admin Functions

| # | Test case | Result | Defect ref | Notes |
|---|-----------|--------|------------|-------|
| 6.1 | Send staff invitation | | | |
| 6.2 | Accept invitation | | | |
| 6.3 | Request role change | | | |
| 6.4 | Approve role change (two-person) | | | |
| 6.5 | Self-approval blocked | | | |
| 6.6 | View audit log | | | |

### 7. Accessibility & i18n

| # | Test case | Result | Defect ref | Notes |
|---|-----------|--------|------------|-------|
| 7.1 | Keyboard navigation | | | |
| 7.2 | Screen reader | | | |
| 7.3 | Switch to Sinhala | | | |
| 7.4 | Switch to Tamil | | | |
| 7.5 | Reduced motion | | | |

### 8. Security Spot Checks

| # | Test case | Result | Defect ref | Notes |
|---|-----------|--------|------------|-------|
| 8.1 | NIC never in clear text | | | |
| 8.2 | Security headers present | | | |
| 8.3 | CORS blocked from wrong origin | | | |
| 8.4 | Rate limit on login | | | |

---

## Defects Found

| Ref | Section | Severity | Description | Status |
|-----|---------|----------|-------------|--------|
| | | | | |

*Add rows as needed. Severity: P1 Blocking / P2 Functional / P3 Cosmetic.*

---

## Environment Notes

*Record any environment-specific issues that affected results (e.g. SMTP not configured, seed data missing).*

_______________

---

## Sign-off

| Person | Role | Signature | Date |
|--------|------|-----------|------|
| | QA Lead | | |
| | Product Owner / Coordinator | | |

**Decision:** ☐ Approved for release  ☐ Conditional approval (list conditions below)  ☐ Not approved

Conditions / comments:

_______________
