"use client";

import { useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { RoleGuard } from "@/components/RoleGuard";
import { apiFetch } from "@/lib/api";

export default function NewRequestPage() {
  const router = useRouter();
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
      setError(err instanceof Error ? err.message : "Failed to submit request");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <RoleGuard allowedRoles={["Citizen"]}>
      <div className="container" style={{ maxWidth: 680 }}>
        <div className="card">
          <h1>New Assistance Request</h1>
          <form onSubmit={handleSubmit}>
            <div className="form-field">
              <label htmlFor="req-title">Title</label>
              <input id="req-title" className="form-input" required value={title} onChange={(e) => setTitle(e.target.value)} />
            </div>
            <div className="form-field">
              <label htmlFor="req-district">District</label>
              <input id="req-district" className="form-input" required value={district} onChange={(e) => setDistrict(e.target.value)} />
            </div>
            <div className="form-field">
              <label htmlFor="req-urgency">Urgency</label>
              <select id="req-urgency" className="form-input" value={urgency} onChange={(e) => setUrgency(e.target.value)}>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
                <option value="Critical">Critical</option>
              </select>
            </div>
            <div className="form-field">
              <label htmlFor="req-desc">Details</label>
              <textarea id="req-desc" className="form-input" rows={4} required value={description} onChange={(e) => setDescription(e.target.value)} />
            </div>
            {error && <p role="alert" style={{ color: "var(--color-danger)" }}>{error}</p>}
            <button type="submit" className="btn btn-primary" disabled={submitting}>
              {submitting ? "Submitting…" : "Submit Request"}
            </button>
          </form>
        </div>
      </div>
    </RoleGuard>
  );
}
