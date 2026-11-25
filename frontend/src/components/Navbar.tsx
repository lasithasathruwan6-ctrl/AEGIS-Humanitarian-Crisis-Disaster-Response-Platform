"use client";

import { useState } from "react";
import Link from "next/link";

export function Navbar() {
  const [menuOpen, setMenuOpen] = useState(false);

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
          Crisis Aid Platform
        </Link>

        <nav style={{ display: "flex", alignItems: "center", gap: "var(--space-4)" }}>
          <Link href="/shelters">Shelters</Link>
          <Link href="/citizen">Citizen</Link>
          <Link href="/staff">Staff</Link>
          <Link href="/admin">Admin</Link>
          <Link href="/login" className="btn btn-secondary">
            Sign in
          </Link>
          <button
            type="button"
            className="btn btn-secondary btn-icon"
            onClick={() => setMenuOpen((prev) => !prev)}
          >
            {menuOpen ? "✕" : "☰"}
          </button>
        </nav>
      </div>
    </header>
  );
}
