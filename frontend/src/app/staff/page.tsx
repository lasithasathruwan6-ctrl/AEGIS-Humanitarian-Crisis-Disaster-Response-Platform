"use client";

import { useCallback, useEffect, useState } from "react";
import { LoadingSkeleton } from "@/components/LoadingSkeleton";
import { RoleGuard } from "@/components/RoleGuard";
import { StatusState } from "@/components/StatusState";
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

  const loadQueue = useCallback(() => {
    setLoading(true);
    setError(null);
    return apiFetch<StaffRequestRow[]>("/requests")
      .then((data) => setRows(data))
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Unable to load triage queue")
      )
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    void loadQueue();
  }, [loadQueue]);

  return (
    <RoleGuard allowedRoles={["Staff", "Coordinator", "Admin"]}>
      <div className="container">
        <h1>Staff Triage Queue</h1>
        {loading && <LoadingSkeleton variant="table" count={4} label="Loading triage queue" />}
        {error && (
          <StatusState
            variant="error"
            title="Unable to load triage queue"
            message={error}
            actionLabel="Retry"
            onAction={() => void loadQueue()}
          />
        )}
        {!loading && !error && rows.length === 0 && (
          <StatusState
            variant="empty"
            title="Queue is empty"
            message="No requests are waiting in the triage queue."
          />
        )}
        {rows.length > 0 && (
          <div className="table-responsive">
            <table className="data-table">
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
                    <td data-label="Title">{row.title}</td>
                    <td data-label="District">{row.district}</td>
                    <td data-label="Urgency">{row.urgency}</td>
                    <td data-label="Status">{row.status}</td>
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
