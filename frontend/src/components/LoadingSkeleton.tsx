interface LoadingSkeletonProps {
  variant?: "cards" | "table";
  count?: number;
  label?: string;
}

export function LoadingSkeleton({
  variant = "cards",
  count = 3,
  label = "Loading content",
}: LoadingSkeletonProps) {
  const items = Array.from({ length: count }, (_, idx) => idx);

  if (variant === "table") {
    return (
      <div
        role="status"
        aria-busy="true"
        aria-label={label}
        className="card"
        style={{ display: "flex", flexDirection: "column", gap: "var(--space-3)" }}
      >
        {items.map((idx) => (
          <div
            key={idx}
            className="skeleton-box"
            style={{ height: "2.25rem", width: "100%" }}
          />
        ))}
      </div>
    );
  }

  return (
    <div
      role="status"
      aria-busy="true"
      aria-label={label}
      className="grid-cards"
    >
      {items.map((idx) => (
        <div key={idx} className="card" style={{ display: "flex", flexDirection: "column", gap: "var(--space-3)" }}>
          <div className="skeleton-box" style={{ height: "1.25rem", width: "65%" }} />
          <div className="skeleton-box" style={{ height: "0.875rem", width: "45%" }} />
          <div className="skeleton-box" style={{ height: "0.875rem", width: "80%" }} />
        </div>
      ))}
    </div>
  );
}
