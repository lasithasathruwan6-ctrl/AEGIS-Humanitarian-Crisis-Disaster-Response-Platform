"use client";

import { useCallback, useEffect, useState } from "react";
import { LoadingSkeleton } from "@/components/LoadingSkeleton";
import { RoleGuard } from "@/components/RoleGuard";
import { StatusState } from "@/components/StatusState";
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

  const loadAuditLog = useCallback(() => {
    setLoading(true);
    setError(null);
    return apiFetch<AuditEntry[]>("/admin/audit-log")
      .then((data) => setEntries(data))
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Unable to load audit log")
      )
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    void loadAuditLog();
  }, [loadAuditLog]);

  return (
    <RoleGuard allowedRoles={["Admin"]}>
      <div className="container">
        <h1>Admin Console</h1>
        {loading && <LoadingSkeleton variant="table" count={4} label="Loading audit log" />}
        {error && (
          <StatusState
            variant="error"
            title="Unable to load audit log"
            message={error}
            actionLabel="Retry"
            onAction={() => void loadAuditLog()}
          />
        )}
        {!loading && !error && entries.length === 0 && (
          <StatusState
            variant="empty"
            title="No audit entries"
            message="No audit log entries have been recorded yet."
          />
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
