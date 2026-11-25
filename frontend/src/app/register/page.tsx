"use client";

import { useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import {
  validateRegistrationFields,
  type RegistrationErrors,
} from "@/lib/validation";

export default function RegisterPage() {
  const router = useRouter();
  const [fullName, setFullName] = useState("");
  const [nic, setNic] = useState("");
  const [phone, setPhone] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [fieldErrors, setFieldErrors] = useState<RegistrationErrors>({});
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);

    const validation = validateRegistrationFields({ nic, phone, email });
    setFieldErrors(validation);
    if (Object.keys(validation).length > 0) {
      return;
    }

    setSubmitting(true);
    try {
      await apiFetch("/auth/register", {
        method: "POST",
        body: JSON.stringify({ fullName, nic, phone, email, password }),
      });
      router.replace("/login");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Registration failed");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="container" style={{ maxWidth: 520 }}>
      <div className="card">
        <h1>Citizen Registration</h1>
        <form onSubmit={handleSubmit} noValidate>
          <div className="form-field">
            <label htmlFor="reg-name">Full name</label>
            <input id="reg-name" className="form-input" required value={fullName} onChange={(e) => setFullName(e.target.value)} />
          </div>
          <div className="form-field">
            <label htmlFor="reg-nic">NIC</label>
            <input
              id="reg-nic"
              className="form-input"
              required
              aria-invalid={Boolean(fieldErrors.nic)}
              aria-describedby={fieldErrors.nic ? "reg-nic-error" : undefined}
              value={nic}
              onChange={(e) => setNic(e.target.value)}
            />
            {fieldErrors.nic && (
              <span id="reg-nic-error" role="alert" style={{ color: "var(--color-danger)", fontSize: "0.875rem" }}>
                {fieldErrors.nic}
              </span>
            )}
          </div>
          <div className="form-field">
            <label htmlFor="reg-phone">Phone</label>
            <input
              id="reg-phone"
              className="form-input"
              required
              aria-invalid={Boolean(fieldErrors.phone)}
              aria-describedby={fieldErrors.phone ? "reg-phone-error" : undefined}
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
            />
            {fieldErrors.phone && (
              <span id="reg-phone-error" role="alert" style={{ color: "var(--color-danger)", fontSize: "0.875rem" }}>
                {fieldErrors.phone}
              </span>
            )}
          </div>
          <div className="form-field">
            <label htmlFor="reg-email">Email</label>
            <input
              id="reg-email"
              className="form-input"
              type="email"
              required
              aria-invalid={Boolean(fieldErrors.email)}
              aria-describedby={fieldErrors.email ? "reg-email-error" : undefined}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
            {fieldErrors.email && (
              <span id="reg-email-error" role="alert" style={{ color: "var(--color-danger)", fontSize: "0.875rem" }}>
                {fieldErrors.email}
              </span>
            )}
          </div>
          <div className="form-field">
            <label htmlFor="reg-password">Password</label>
            <input id="reg-password" className="form-input" type="password" required value={password} onChange={(e) => setPassword(e.target.value)} />
          </div>
          {error && (
            <p role="alert" style={{ color: "var(--color-danger)" }}>
              {error}
            </p>
          )}
          <button type="submit" className="btn btn-primary" disabled={submitting}>
            {submitting ? "Creating account…" : "Create account"}
          </button>
        </form>
      </div>
    </div>
  );
}
