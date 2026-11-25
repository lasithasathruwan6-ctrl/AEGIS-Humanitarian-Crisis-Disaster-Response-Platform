"use client";

import { useState } from "react";
import { IconButton } from "@/components/IconButton";

export interface MapShelterPoint {
  id: string;
  name: string;
  district: string;
  capacity: number;
  occupied: number;
  latitude?: number;
  longitude?: number;
}

interface ShelterMapProps {
  shelters: MapShelterPoint[];
  tileUrl?: string;
}

const DEFAULT_TILE_URL = "https://tile.openstreetmap.org/7/91/59.png";

export function ShelterMap({
  shelters,
  tileUrl = DEFAULT_TILE_URL,
}: ShelterMapProps) {
  const [zoom, setZoom] = useState(7);
  const [tileFailed, setTileFailed] = useState(false);
  const [attempt, setAttempt] = useState(0);

  function handleRetryTiles() {
    setTileFailed(false);
    setAttempt((prev) => prev + 1);
  }

  return (
    <section
      className="card"
      aria-label="Shelter locations map"
      style={{ marginBottom: "var(--space-6)" }}
    >
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: "var(--space-3)",
        }}
      >
        <h2 style={{ margin: 0 }}>Shelter Map</h2>
        <div style={{ display: "flex", gap: "var(--space-2)" }}>
          <IconButton
            label="Zoom in map"
            icon="+"
            onClick={() => setZoom((z) => Math.min(z + 1, 16))}
          />
          <IconButton
            label="Zoom out map"
            icon="−"
            onClick={() => setZoom((z) => Math.max(z - 1, 5))}
          />
        </div>
      </div>

      {tileFailed ? (
        <div
          role="alert"
          style={{
            background: "var(--color-warning-soft)",
            color: "var(--color-warning)",
            border: "1px solid var(--color-warning)",
            borderRadius: "var(--radius-md)",
            padding: "var(--space-4)",
          }}
        >
          <p style={{ marginTop: 0, fontWeight: 600 }}>
            Map tiles could not be loaded.
          </p>
          <p style={{ marginTop: 0 }}>
            Your connection may be limited. You can retry loading the map or use the shelter list below.
          </p>
          <button
            type="button"
            className="btn btn-secondary"
            onClick={handleRetryTiles}
          >
            Retry loading map
          </button>
        </div>
      ) : (
        <div
          style={{
            position: "relative",
            borderRadius: "var(--radius-md)",
            overflow: "hidden",
            border: "1px solid var(--color-border)",
            background: "var(--color-surface-alt)",
            minHeight: 220,
          }}
        >
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img
            key={`${zoom}-${attempt}`}
            src={`${tileUrl}?z=${zoom}&r=${attempt}`}
            alt="Regional map tiles showing emergency shelter areas"
            style={{ width: "100%", height: 220, objectFit: "cover", display: "block" }}
            onError={() => setTileFailed(true)}
          />
          <div
            style={{
              position: "absolute",
              bottom: "var(--space-2)",
              left: "var(--space-2)",
              background: "var(--color-surface)",
              padding: "var(--space-1) var(--space-2)",
              borderRadius: "var(--radius-sm)",
              fontSize: "0.75rem",
            }}
          >
            Zoom {zoom} • {shelters.length} shelter(s)
          </div>
        </div>
      )}
    </section>
  );
}
