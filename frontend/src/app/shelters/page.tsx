"use client";

import { useEffect, useState } from "react";
import { LoadingSkeleton } from "@/components/LoadingSkeleton";
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

  useEffect(() => {
    let active = true;
    apiFetch<ShelterItem[]>("/shelters")
      .then((data) => {
        if (active) setShelters(data);
      })
      .catch((err) => {
        if (active) setError(err instanceof Error ? err.message : "Failed to load shelters");
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, []);

  return (
    <div className="container">
      <h1>Shelters</h1>
      {loading && <LoadingSkeleton variant="cards" count={3} label="Loading shelters" />}
      {error && <p role="alert" style={{ color: "var(--color-danger)" }}>{error}</p>}
      {!loading && !error && shelters.length === 0 && (
        <p className="muted">No shelters available right now.</p>
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
