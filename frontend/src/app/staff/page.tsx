"use client";

import { useCallback } from "react";
import { LoadingSkeleton } from "@/components/LoadingSkeleton";
import { RoleGuard } from "@/components/RoleGuard";
import { StatusState } from "@/components/StatusState";
import { usePolling } from "@/hooks/usePolling";
import { apiFetch } from "@/lib/api";

interface StaffRequestRow {
  id: string;
  title: string;
  district: string;
  urgency: string;
  status: string;
}

export default function StaffDashboardPage() {
  const fetchQueue = useCallback(
    (signal: AbortSignal) => apiFetch<StaffRequestRow[]>("/requests", { signal }),
    []
  );

  const { data, loading, error, retry } = usePolling<StaffRequestRow[]>({
    fetcher: fetchQueue,
    baseIntervalMs: 10000,
    maxIntervalMs: 60000,
  });

  const rows = data ?? [];

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
            onAction={retry}
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
