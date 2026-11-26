# AI Prompts, Fields & Validation Rules

## 1. Prompt Versions
| Feature | Prompt Version | Output Format | Fallback |
| :--- | :--- | :--- | :--- |
| Request Triage | `aegis-triage-v2.4` | JSON (`priority`, `urgencyScore`, `confidence`, `rationale`) | Deterministic `TriageEngine` |
| Citizen Assistant | `aegis-assistant-v1.2` | Plain text + configured emergency contacts | Static safety guidance |
| Chat Summary & Reply | `aegis-copilot-v1.0` | Draft summary / suggested reply (never auto-sent) | Staff manual reply |

## 2. Input Fields & Redaction Rules
- **Allowed fields**: `category`, `area`, `language`, `peopleCount`, `vulnerableFlags`, and `redactedDescription`.
- **Forbidden fields**: `Nic`, `ContactMobile`, `ContactEmail`, `UserId`, `FullName`.
- All free-text inputs pass through `PiiRedactor` before leaving the API boundary (strips old 9-digit + `V`/`X` and new 12-digit NICs, Sri Lankan phone numbers, and email addresses).

## 3. Output Validation & Safety Guards
1. **Strict JSON Schema**: Any malformed or non-JSON response from Groq triggers automatic fallback to `TriageEngine`.
2. **Input Length Cap**: Citizen assistant prompts exceeding the configured max character limit (e.g., `1000` chars) are rejected with HTTP 400.
3. **Prompt Injection Guard**: System instructions cannot be overridden by user text (`ignore previous instructions`, role hijacking, etc.).
4. **Confidence Gate**: Recommendations with `confidence < ConfidenceThreshold` (default `0.60`) are flagged `NeedsHumanReview`.
5. **Critical Keyword Floor**: Presence of critical hazard/medical keywords in English, Sinhala, Tamil, or Singlish prevents priority from dropping below `High`.
