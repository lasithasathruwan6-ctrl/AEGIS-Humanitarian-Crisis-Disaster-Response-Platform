"use client";

import { useEffect, useState } from "react";
import { LoadingSkeleton } from "@/components/LoadingSkeleton";
import { RoleGuard } from "@/components/RoleGuard";
import { apiFetch } from "@/lib/api";

interface AuditEntry {
  id: string;
  actor: string;
  action: string;
  timestamp: string;
}

export default function AdminDashboardPage() {
  const [entries, setEntries] = useState<AuditEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    apiFetch<AuditEntry[]>("/admin/audit-log")
      .then((data) => {
        if (active) setEntries(data);
      })
      .catch((err) => {
        if (active) setError(err instanceof Error ? err.message : "Unable to load audit log");
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, []);

  return (
    <RoleGuard allowedRoles={["Admin"]}>
      <div className="container">
        <h1>Admin Console</h1>
        {loading && <LoadingSkeleton variant="table" count={4} label="Loading audit log" />}
        {error && <p role="alert" style={{ color: "var(--color-danger)" }}>{error}</p>}
        {!loading && !error && entries.length === 0 && (
          <p className="muted">No audit log entries recorded.</p>
        )}
        {entries.length > 0 && (
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Actor</th>
                  <th>Action</th>
                  <th>Timestamp</th>
                </tr>
              </thead>
              <tbody>
                {entries.map((entry) => (
                  <tr key={entry.id}>
                    <td data-label="Actor">{entry.actor}</td>
                    <td data-label="Action">{entry.action}</td>
                    <td data-label="Timestamp">{entry.timestamp}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </RoleGuard>
  );
}
