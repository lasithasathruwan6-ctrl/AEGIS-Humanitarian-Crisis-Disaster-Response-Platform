"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { LoadingSkeleton } from "@/components/LoadingSkeleton";
import { RequestChat, type ChatMessage } from "@/components/RequestChat";
import { RoleGuard } from "@/components/RoleGuard";
import { StatusState } from "@/components/StatusState";
import { apiFetch } from "@/lib/api";

export default function CitizenRequestDetailPage() {
  const params = useParams<{ id: string }>();
  const requestId = params?.id ?? "";
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!requestId) return;
    let active = true;
    apiFetch<ChatMessage[]>(`/requests/${requestId}/messages`)
      .then((data) => {
        if (active) setMessages(data);
      })
      .catch((err) => {
        if (active) setError(err instanceof Error ? err.message : "Failed to load messages");
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, [requestId]);

  return (
    <RoleGuard allowedRoles={["Citizen", "Volunteer", "Staff", "Coordinator", "Admin"]}>
      <div className="container">
        <h1>Request #{requestId}</h1>
        {loading && <LoadingSkeleton variant="cards" count={1} label="Loading conversation" />}
        {error && (
          <StatusState variant="error" title="Unable to load messages" message={error} />
        )}
        {!loading && !error && (
          <RequestChat requestId={requestId} initialMessages={messages} />
        )}
      </div>
    </RoleGuard>
  );
}
