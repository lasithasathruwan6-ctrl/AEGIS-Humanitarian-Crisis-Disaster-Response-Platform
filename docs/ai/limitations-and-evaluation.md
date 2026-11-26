# AI Limitations & Evaluation

## 1. Known Limitations
- **Keyword & Heuristic Coverage**: `TriageEngine` relies on deterministic weights and curated English, Sinhala, Tamil, and Singlish keywords; novel slang or regional dialects not yet in the keyword list rely on coordinator review.
- **External LLM Dependency**: `GroqAssistantClient` depends on external Groq API availability and rate limits (`429`); when unavailable, timed out, or returning malformed JSON, the system falls back to `TriageEngine` and static safety templates.
- **Data Sparsity**: Sparse citizen descriptions (`< 20` characters) yield low confidence (`0.42`), intentionally routing cases to human review rather than guessing priority.
- **Fairness Monitoring Scope**: Disparity checks flag statistical divergence across languages and districts, but require sufficient decision volume to be meaningful.

## 2. Evaluation & Verification
All AI behaviors are verified via automated xUnit tests in `backend/CrisisPlatform.Tests/AI/`:
- **Triage & Multilingual Safety**: Weight sum (`1.00`), urgency ranking, confidence review gate, critical-keyword priority floor, deterministic counterfactuals, and Sinhala/Tamil/Singlish keywords.
- **Privacy & Resilience**: `PiiRedactor` (old/new NIC, spaced phones, emails), Groq `429` retry & timeout fallback, invalid JSON fallback, prompt-injection blocking, input length cap, and configured emergency contacts.
- **Governance & Domain Rules**: `Active` / `Shadow` / `Off` audited mode switching, override analytics, calibration buckets, fairness disparity flags, draft-only copilot replies, volunteer skill/distance ranking, and donation anomaly rules.
