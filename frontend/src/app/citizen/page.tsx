"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { LoadingSkeleton } from "@/components/LoadingSkeleton";
import { RoleGuard } from "@/components/RoleGuard";
import { StatusState } from "@/components/StatusState";
import { apiFetch } from "@/lib/api";

interface AidRequestSummary {
  id: string;
  title: string;
  status: string;
  urgency: string;
}

export default function CitizenDashboardPage() {
  const [requests, setRequests] = useState<AidRequestSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadRequests = useCallback(() => {
    setLoading(true);
    setError(null);
    return apiFetch<AidRequestSummary[]>("/requests")
      .then((data) => setRequests(data))
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Unable to load requests")
      )
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    void loadRequests();
  }, [loadRequests]);

  return (
    <RoleGuard allowedRoles={["Citizen"]}>
      <div className="container">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "var(--space-4)" }}>
          <h1>Citizen Portal</h1>
          <Link href="/citizen/requests/new" className="btn btn-primary">
            New Request
          </Link>
        </div>

        {loading && <LoadingSkeleton variant="cards" count={3} label="Loading your requests" />}
        {error && (
          <StatusState
            variant="error"
            title="Unable to load requests"
            message={error}
            actionLabel="Retry"
            onAction={() => void loadRequests()}
          />
        )}
        {!loading && !error && requests.length === 0 && (
          <StatusState
            variant="empty"
            title="No requests yet"
            message="You have not submitted any assistance requests yet."
          />
        )}
        <div className="grid-cards">
          {requests.map((req) => (
            <article key={req.id} className="card">
              <h2>{req.title}</h2>
              <span className="badge badge-info">{req.status}</span>
            </article>
          ))}
        </div>
      </div>
    </RoleGuard>
  );
}
