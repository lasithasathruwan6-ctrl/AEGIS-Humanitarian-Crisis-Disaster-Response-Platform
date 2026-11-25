"use client";

import { useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { RoleGuard } from "@/components/RoleGuard";
import { useI18n } from "@/i18n/I18nProvider";
import { apiFetch } from "@/lib/api";

export default function NewRequestPage() {
  const router = useRouter();
  const { t } = useI18n();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [urgency, setUrgency] = useState("High");
  const [district, setDistrict] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await apiFetch("/requests", {
        method: "POST",
        body: JSON.stringify({ title, description, urgency, district }),
      });
      router.replace("/citizen");
    } catch (err) {
      setError(err instanceof Error ? err.message : t("submitRequestFailed"));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <RoleGuard allowedRoles={["Citizen"]}>
      <div className="container" style={{ maxWidth: 680 }}>
        <div className="card">
          <h1>{t("newRequestTitle")}</h1>
          <form onSubmit={handleSubmit}>
            <div className="form-field">
              <label htmlFor="req-title">{t("fieldTitle")}</label>
              <input id="req-title" className="form-input" required value={title} onChange={(e) => setTitle(e.target.value)} />
            </div>
            <div className="form-grid">
              <div className="form-field">
                <label htmlFor="req-district">{t("fieldDistrict")}</label>
                <input id="req-district" className="form-input" required value={district} onChange={(e) => setDistrict(e.target.value)} />
              </div>
              <div className="form-field">
                <label htmlFor="req-urgency">{t("fieldUrgency")}</label>
                <select id="req-urgency" className="form-input" value={urgency} onChange={(e) => setUrgency(e.target.value)}>
                  <option value="Low">{t("urgencyLow")}</option>
                  <option value="Medium">{t("urgencyMedium")}</option>
                  <option value="High">{t("urgencyHigh")}</option>
                  <option value="Critical">{t("urgencyCritical")}</option>
                </select>
              </div>
            </div>
            <div className="form-field">
              <label htmlFor="req-desc">{t("fieldDetails")}</label>
              <textarea id="req-desc" className="form-input" rows={4} required value={description} onChange={(e) => setDescription(e.target.value)} />
            </div>
            {error && <p role="alert" style={{ color: "var(--color-danger)" }}>{error}</p>}
            <div className="form-actions">
              <button type="submit" className="btn btn-primary" disabled={submitting}>
                {submitting ? t("submittingRequest") : t("submitRequest")}
              </button>
            </div>
          </form>
        </div>
      </div>
    </RoleGuard>
  );
}
