"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { LoadingSkeleton } from "@/components/LoadingSkeleton";
import { RoleGuard } from "@/components/RoleGuard";
import { StatusState } from "@/components/StatusState";
import { useI18n } from "@/i18n/I18nProvider";
import { apiFetch } from "@/lib/api";

interface AidRequestSummary {
  id: string;
  title: string;
  status: string;
  urgency: string;
}

export default function CitizenDashboardPage() {
  const { t } = useI18n();
  const [requests, setRequests] = useState<AidRequestSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadRequests = useCallback(() => {
    setLoading(true);
    setError(null);
    return apiFetch<AidRequestSummary[]>("/requests")
      .then((data) => setRequests(data))
      .catch((err) =>
        setError(err instanceof Error ? err.message : t("errorLoadingRequestsTitle"))
      )
      .finally(() => setLoading(false));
  }, [t]);

  useEffect(() => {
    void loadRequests();
  }, [loadRequests]);

  return (
    <RoleGuard allowedRoles={["Citizen"]}>
      <div className="container">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "var(--space-4)" }}>
          <h1>{t("citizenPortalTitle")}</h1>
          <Link href="/citizen/requests/new" className="btn btn-primary">
            {t("newRequestButton")}
          </Link>
        </div>

        {loading && <LoadingSkeleton variant="cards" count={3} label={t("loadingRequests")} />}
        {error && (
          <StatusState
            variant="error"
            title={t("errorLoadingRequestsTitle")}
            message={error}
            actionLabel={t("retryButton")}
            onAction={() => void loadRequests()}
          />
        )}
        {!loading && !error && requests.length === 0 && (
          <StatusState
            variant="empty"
            title={t("emptyRequestsTitle")}
            message={t("emptyRequestsMessage")}
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
