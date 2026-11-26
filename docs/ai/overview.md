# AI Overview — Crisis Aid Platform

## 1. Purpose & Design Principle
AI components in the Crisis Aid Platform assist coordinators with faster triage, multilingual citizen communication, volunteer matching, and anomaly flagging. **Human-in-the-loop (HITL) rule:** AI never makes binding operational decisions or sends external messages autonomously; it only computes recommendations, scores, and draft text for named human staff to **Approve**, **Modify**, **Reject**, or **Defer**.

## 2. AI Roles in the Platform
| Capability | Component | Output | Human Control |
| :--- | :--- | :--- | :--- |
| Request Triage | `TriageEngine` | Urgency score (0–100), priority tier, rationale, counterfactual note | Coordinator records final decision; confidence gate flags uncertain cases |
| Multilingual Assistant | `GroqAssistantService` | Structured `en`/`si`/`ta` guidance + configured emergency contacts | Prompt-injection & length guards; PII redacted before outbound call |
| AI Governance | `AiGovernanceService` | `Active` / `Shadow` / `Off` modes, calibration, fairness checks | SystemAdmin switches mode with mandatory audited reason |
| Volunteer Matching | `VolunteerMatcher` | Ranked volunteer candidates by skill overlap & distance | Coordinator manually assigns volunteer |
| Donation Anomaly | `DonationAnomalyDetector` | Flags high-value or rapid-repeat pledges | Donor Officer reviews and approves/rejects |

## 3. Data Flow (Intake to Decision)
1. Citizen submits an assistance request (`en`, `si`, `ta`, or Singlish).
2. `PiiRedactor` strips NIC numbers, mobile numbers, and emails before any external AI call.
3. `AiGovernanceService` checks the live mode:
   - **`Off` (Kill Switch)**: skips AI scoring and routes directly to `ManualTriage`.
   - **`Shadow`**: scores silently for evaluation without exposing recommendations to staff UI.
   - **`Active`**: scores and surfaces rationale + confidence; cases below threshold are flagged `NeedsHumanReview`.
4. Staff member records a decision (`Approve` / `Modify` / `Reject` / `Defer`), which is sealed in the audit ledger and feeds override & fairness analytics.
