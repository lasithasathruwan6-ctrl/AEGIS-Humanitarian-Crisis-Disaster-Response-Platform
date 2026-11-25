# Frontend UI & Engineering Guide

## 1. Design Tokens
All design primitives live in `frontend/src/styles/tokens.css` and are imported once by `frontend/src/app/globals.css`:
- **Surfaces & Text:** `--color-bg`, `--color-surface`, `--color-surface-alt`, `--color-border`, `--color-text`, `--color-text-muted`, `--color-text-inverse`
- **Brand & Status:** `--color-primary`, `--color-danger`, `--color-warning`, `--color-success`, `--color-info` (plus `*-soft` badge/alert backgrounds)
- **Spacing & Radii:** `--space-1` (`0.25rem`) through `--space-12` (`3rem`), `--radius-sm` through `--radius-full`
- **Motion & Layering:** `--transition-fast`, `--transition-normal`, `--z-nav`, `--z-modal`

Never hard-code hex values in components; reference CSS custom properties so contrast and theming remain consistent.

## 2. Accessibility Standards
- **Keyboard Focus:** Every interactive element (`a`, `button`, `input`, `select`, `textarea`, `[tabindex]`) renders a 3px `:focus-visible` outline, and the root layout provides a `.skip-link` to `#main-content`.
- **Icon-Only Buttons:** Use `<IconButton label="..." icon="..." />` (`frontend/src/components/IconButton.tsx`), which enforces a required `label` mapped to `aria-label` and hides decorative glyphs with `aria-hidden="true"`.
- **Colour Contrast:** All foreground/background token pairs are verified against WCAG 2.1 AA (`>= 4.5:1`) in `frontend/tests/contrast.test.ts`.
- **Reduced Motion:** `@media (prefers-reduced-motion: reduce)` disables smooth scrolling, pulse animations, and CSS transitions.

## 3. Responsive Layout
- **Small-Phone Forms (`<= 480px`):** `.form-grid` collapses from two columns to a single column, `.form-input` enforces `font-size: 1rem` and `min-height: 2.75rem` touch targets, and `.form-actions` stacks full-width submit buttons.
- **Tables (`<= 640px`):** Wrap tables in `.table-responsive` with `.data-table` and `data-label="..."` on each `<td>` so wide tables scroll horizontally on medium viewports and stack into labeled key-value rows on narrow phones.

## 4. Async UI States & Resilience
- **Loading:** Render `<LoadingSkeleton variant="cards" | "table" />` (`role="status"`, `aria-busy="true"`).
- **Empty & Error:** Render `<StatusState variant="empty" | "error" />` with an optional retry callback.
- **401 Handling:** `apiFetch` clears expired credentials and dispatches `crisis-aid:unauthorized`, opening `<SessionExpiredModal />` so users can sign back in with `?next=` preserved.
- **Polling:** `usePolling` (`frontend/src/hooks/usePolling.ts`) schedules background refreshes with exponential back-off on failure and aborts in-flight requests on unmount.
- **Chat & Map:** `<RequestChat />` auto-scrolls and supports optimistic send + per-message retry; `<ShelterMap />` displays an accessible fallback alert and retry action if map tiles fail to load. Both heavy modules are lazy-loaded via `next/dynamic`.

## 5. Internationalisation (i18n) & Validation
- **Languages:** English (`en`), Sinhala (`si`), and Tamil (`ta`) dictionaries in `frontend/src/i18n/` are served via `<I18nProvider />`, persisted in `localStorage` (`crisis_aid_locale`), and reflected on `<html lang="...">`.
- **Validation:** `frontend/src/lib/validation.ts` validates Sri Lankan NICs (9-digit + `V`/`X` and 12-digit), local/`+94` phone numbers, emails, and evidence uploads (JPEG, PNG, WebP, PDF up to 5 MB).

## 6. Commands
Run from `frontend/`:
```bash
npm install
npm run dev        # Start local Next.js dev server
npm run check      # Zero-warning ESLint + strict TypeScript check
npm test           # Run Vitest suite
npm run build      # Production build
```
