"use client";

import { useState } from "react";
import Link from "next/link";
import { IconButton } from "@/components/IconButton";
import { LOCALE_LABELS, type Locale } from "@/i18n";
import { useI18n } from "@/i18n/I18nProvider";

export function Navbar() {
  const [menuOpen, setMenuOpen] = useState(false);
  const { locale, setLocale, t } = useI18n();

  return (
    <header
      style={{
        background: "var(--color-surface)",
        borderBottom: "1px solid var(--color-border)",
        position: "sticky",
        top: 0,
        zIndex: "var(--z-nav)",
      }}
    >
      <div
        className="container"
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          paddingTop: "var(--space-3)",
          paddingBottom: "var(--space-3)",
        }}
      >
        <Link href="/" style={{ fontWeight: 700, fontSize: "1.125rem", color: "var(--color-text)" }}>
          {t("appTitle")}
        </Link>

        <nav
          id="primary-navigation"
          aria-label="Primary navigation"
          style={{ display: "flex", alignItems: "center", gap: "var(--space-4)", flexWrap: "wrap" }}
        >
          <Link href="/shelters">{t("navShelters")}</Link>
          <Link href="/citizen">{t("navCitizen")}</Link>
          <Link href="/staff">{t("navStaff")}</Link>
          <Link href="/admin">{t("navAdmin")}</Link>
          <select
            aria-label={t("languageLabel")}
            className="form-input"
            style={{ width: "auto", minHeight: "2.25rem", padding: "var(--space-1) var(--space-2)" }}
            value={locale}
            onChange={(e) => setLocale(e.target.value as Locale)}
          >
            {(Object.keys(LOCALE_LABELS) as Locale[]).map((code) => (
              <option key={code} value={code}>
                {LOCALE_LABELS[code]}
              </option>
            ))}
          </select>
          <Link href="/login" className="btn btn-secondary">
            {t("navSignIn")}
          </Link>
          <IconButton
            label={menuOpen ? "Close navigation menu" : "Open navigation menu"}
            aria-expanded={menuOpen}
            aria-controls="primary-navigation"
            icon={menuOpen ? "✕" : "☰"}
            onClick={() => setMenuOpen((prev) => !prev)}
          />
        </nav>
      </div>
    </header>
  );
}
