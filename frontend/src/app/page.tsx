import Link from "next/link";

export default function HomePage() {
  return (
    <div className="container">
      <section className="card" style={{ marginBottom: "var(--space-6)" }}>
        <span className="badge badge-info">Humanitarian Coordination</span>
        <h1 style={{ marginTop: "var(--space-3)", marginBottom: "var(--space-2)" }}>
          Crisis Aid Platform
        </h1>
        <p className="muted" style={{ marginBottom: "var(--space-5)" }}>
          Coordinate emergency assistance requests, shelters, volunteers, and relief resources.
        </p>
        <div style={{ display: "flex", gap: "var(--space-3)", flexWrap: "wrap" }}>
          <Link href="/citizen/requests/new" className="btn btn-primary">
            Request Assistance
          </Link>
          <Link href="/shelters" className="btn btn-secondary">
            View Shelters
          </Link>
        </div>
      </section>

      <section className="grid-cards">
        <article className="card">
          <h2>Citizens</h2>
          <p className="muted">Submit assistance requests, upload evidence, and track status updates.</p>
          <Link href="/citizen">Open Citizen Portal</Link>
        </article>
        <article className="card">
          <h2>Staff &amp; Coordinators</h2>
          <p className="muted">Triage incoming requests, assign responders, and manage shelters.</p>
          <Link href="/staff">Open Staff Portal</Link>
        </article>
        <article className="card">
          <h2>Administrators</h2>
          <p className="muted">Manage staff invitations, role approvals, and audit logs.</p>
          <Link href="/admin">Open Admin Console</Link>
        </article>
      </section>
    </div>
  );
}
