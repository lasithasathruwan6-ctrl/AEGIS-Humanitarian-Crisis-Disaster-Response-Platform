"use client";

import { useEffect, useState } from "react";
import { usePathname, useRouter } from "next/navigation";
import { getHomeRouteForRole, getSession, type UserRole } from "@/lib/auth";

interface RoleGuardProps {
  allowedRoles: UserRole[];
  children: React.ReactNode;
}

export function RoleGuard({ allowedRoles, children }: RoleGuardProps) {
  const router = useRouter();
  const pathname = usePathname();
  const [authorized, setAuthorized] = useState(false);

  useEffect(() => {
    const session = getSession();
    if (!session) {
      const nextParam = encodeURIComponent(pathname || "/");
      router.replace(`/login?next=${nextParam}`);
      return;
    }

    if (!allowedRoles.includes(session.role)) {
      router.replace(getHomeRouteForRole(session.role));
      return;
    }

    setAuthorized(true);
  }, [allowedRoles, pathname, router]);

  if (!authorized) {
    return (
      <div className="container" role="status" aria-live="polite">
        <p className="muted">Checking access permissions…</p>
      </div>
    );
  }

  return <>{children}</>;
}
