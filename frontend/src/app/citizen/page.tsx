"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { LoadingSkeleton } from "@/components/LoadingSkeleton";
import { RoleGuard } from "@/components/RoleGuard";
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

  useEffect(() => {
    let active = true;
    apiFetch<AidRequestSummary[]>("/requests")
      .then((data) => {
        if (active) setRequests(data);
      })
      .catch((err) => {
        if (active) setError(err instanceof Error ? err.message : "Unable to load requests");
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, []);

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
        {error && <p role="alert" style={{ color: "var(--color-danger)" }}>{error}</p>}
        {!loading && !error && requests.length === 0 && (
          <p className="muted">You have not submitted any assistance requests yet.</p>
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
