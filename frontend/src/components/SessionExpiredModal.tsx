"use client";

import { useEffect, useState } from "react";
import { usePathname, useRouter } from "next/navigation";

export const UNAUTHORIZED_EVENT = "crisis-aid:unauthorized";

export function SessionExpiredModal() {
  const [open, setOpen] = useState(false);
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    function handleUnauthorized() {
      setOpen(true);
    }
    window.addEventListener(UNAUTHORIZED_EVENT, handleUnauthorized);
    return () => {
      window.removeEventListener(UNAUTHORIZED_EVENT, handleUnauthorized);
    };
  }, []);

  if (!open) return null;

  function handleSignIn() {
    setOpen(false);
    const nextParam = encodeURIComponent(pathname || "/");
    router.replace(`/login?next=${nextParam}`);
  }

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="session-expired-title"
      style={{
        position: "fixed",
        inset: 0,
        background: "rgba(15, 23, 42, 0.55)",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        padding: "var(--space-4)",
        zIndex: "var(--z-modal)",
      }}
    >
      <div className="card" style={{ maxWidth: 420, width: "100%" }}>
        <h2 id="session-expired-title" style={{ marginTop: 0 }}>
          Session Expired
        </h2>
        <p className="muted" style={{ marginBottom: "var(--space-5)" }}>
          Your session has expired or is no longer valid. Please sign in again to continue.
        </p>
        <div className="form-actions">
          <button type="button" className="btn btn-primary" onClick={handleSignIn}>
            Sign in again
          </button>
        </div>
      </div>
    </div>
  );
}
