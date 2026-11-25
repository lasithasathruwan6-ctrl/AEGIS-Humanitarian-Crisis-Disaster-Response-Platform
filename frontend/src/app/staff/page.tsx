"use client";

import { useEffect, useState } from "react";
import { RoleGuard } from "@/components/RoleGuard";
import { apiFetch } from "@/lib/api";

interface StaffRequestRow {
  id: string;
  title: string;
  district: string;
  urgency: string;
  status: string;
}

export default function StaffDashboardPage() {
  const [rows, setRows] = useState<StaffRequestRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    apiFetch<StaffRequestRow[]>("/requests")
      .then((data) => {
        if (active) setRows(data);
      })
      .catch((err) => {
        if (active) setError(err instanceof Error ? err.message : "Unable to load triage queue");
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, []);

  return (
    <RoleGuard allowedRoles={["Staff", "Coordinator", "Admin"]}>
      <div className="container">
        <h1>Staff Triage Queue</h1>
        {loading && <p className="muted">Loading queue…</p>}
        {error && <p role="alert" style={{ color: "var(--color-danger)" }}>{error}</p>}
        {!loading && !error && rows.length === 0 && (
          <p className="muted">No requests in the queue.</p>
        )}
        {rows.length > 0 && (
          <table style={{ width: "100%", borderCollapse: "collapse", background: "var(--color-surface)" }}>
            <thead>
              <tr>
                <th>Title</th>
                <th>District</th>
                <th>Urgency</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((row) => (
                <tr key={row.id}>
                  <td>{row.title}</td>
                  <td>{row.district}</td>
                  <td>{row.urgency}</td>
                  <td>{row.status}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </RoleGuard>
  );
}
