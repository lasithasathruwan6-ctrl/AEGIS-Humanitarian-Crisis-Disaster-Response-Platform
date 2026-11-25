"use client";

import { useCallback, useEffect, useState } from "react";
import { LoadingSkeleton } from "@/components/LoadingSkeleton";
import { StatusState } from "@/components/StatusState";
import { apiFetch } from "@/lib/api";

interface ShelterItem {
  id: string;
  name: string;
  district: string;
  capacity: number;
  occupied: number;
}

export default function SheltersPage() {
  const [shelters, setShelters] = useState<ShelterItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadShelters = useCallback(() => {
    setLoading(true);
    setError(null);
    return apiFetch<ShelterItem[]>("/shelters")
      .then((data) => setShelters(data))
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Failed to load shelters")
      )
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    void loadShelters();
  }, [loadShelters]);

  return (
    <div className="container">
      <h1>Shelters</h1>
      {loading && <LoadingSkeleton variant="cards" count={3} label="Loading shelters" />}
      {error && (
        <StatusState
          variant="error"
          title="Unable to load shelters"
          message={error}
          actionLabel="Retry"
          onAction={() => void loadShelters()}
        />
      )}
      {!loading && !error && shelters.length === 0 && (
        <StatusState
          variant="empty"
          title="No shelters available"
          message="No active shelters are listed right now."
        />
      )}
      <div className="grid-cards">
        {shelters.map((s) => (
          <article key={s.id} className="card">
            <h2>{s.name}</h2>
            <p className="muted">{s.district}</p>
            <p>
              Occupancy: {s.occupied} / {s.capacity}
            </p>
          </article>
        ))}
      </div>
    </div>
  );
}
