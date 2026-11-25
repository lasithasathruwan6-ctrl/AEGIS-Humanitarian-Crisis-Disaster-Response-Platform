"use client";

import { useState, type ChangeEvent, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { RoleGuard } from "@/components/RoleGuard";
import { useI18n } from "@/i18n/I18nProvider";
import { apiFetch } from "@/lib/api";
import { validateEvidenceFile } from "@/lib/validation";

export default function NewRequestPage() {
  const router = useRouter();
  const { t } = useI18n();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [urgency, setUrgency] = useState("High");
  const [district, setDistrict] = useState("");
  const [evidenceFile, setEvidenceFile] = useState<File | null>(null);
  const [fileError, setFileError] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0] ?? null;
    if (!file) {
      setEvidenceFile(null);
      setFileError(null);
      return;
    }
    const validationError = validateEvidenceFile(file);
    if (validationError) {
      setEvidenceFile(null);
      setFileError(validationError);
      return;
    }
    setEvidenceFile(file);
    setFileError(null);
  }

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
            <div className="form-field">
              <label htmlFor="req-evidence">Evidence (JPEG, PNG, WebP, or PDF, max 5 MB)</label>
              <input
                id="req-evidence"
                className="form-input"
                type="file"
                accept=".jpg,.jpeg,.png,.webp,.pdf"
                aria-invalid={Boolean(fileError)}
                onChange={handleFileChange}
              />
              {evidenceFile && !fileError && (
                <span className="muted">Attached: {evidenceFile.name}</span>
              )}
              {fileError && (
                <span role="alert" style={{ color: "var(--color-danger)", fontSize: "0.875rem" }}>
                  {fileError}
                </span>
              )}
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
