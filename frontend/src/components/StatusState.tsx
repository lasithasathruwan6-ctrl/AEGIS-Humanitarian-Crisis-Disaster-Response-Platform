"use client";

interface StatusStateProps {
  variant: "empty" | "error";
  title: string;
  message: string;
  actionLabel?: string;
  onAction?: () => void;
}

export function StatusState({
  variant,
  title,
  message,
  actionLabel,
  onAction,
}: StatusStateProps) {
  const isError = variant === "error";

  return (
    <section
      className="card"
      role={isError ? "alert" : "status"}
      style={{
        borderColor: isError ? "var(--color-danger)" : "var(--color-border)",
        background: isError ? "var(--color-danger-soft)" : "var(--color-surface)",
        textAlign: "center",
        padding: "var(--space-8) var(--space-4)",
      }}
    >
      <h2
        style={{
          marginTop: 0,
          marginBottom: "var(--space-2)",
          color: isError ? "var(--color-danger)" : "var(--color-text)",
        }}
      >
        {title}
      </h2>
      <p
        style={{
          marginTop: 0,
          marginBottom: actionLabel && onAction ? "var(--space-4)" : 0,
          color: isError ? "var(--color-danger)" : "var(--color-text-muted)",
        }}
      >
        {message}
      </p>
      {actionLabel && onAction && (
        <button type="button" className="btn btn-secondary" onClick={onAction}>
          {actionLabel}
        </button>
      )}
    </section>
  );
}
