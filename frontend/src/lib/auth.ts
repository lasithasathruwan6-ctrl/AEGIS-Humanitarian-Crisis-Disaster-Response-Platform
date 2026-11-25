export type UserRole =
  | "Citizen"
  | "Volunteer"
  | "Staff"
  | "Coordinator"
  | "Admin";

export interface AuthSession {
  accessToken: string;
  role: UserRole;
  userId: string;
  displayName?: string;
}

const SESSION_KEY = "crisis_aid_session";

export function getSession(): AuthSession | null {
  if (typeof window === "undefined") return null;
  try {
    const raw = window.localStorage.getItem(SESSION_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as Partial<AuthSession>;
    if (!parsed.accessToken || !parsed.role || !parsed.userId) return null;
    return parsed as AuthSession;
  } catch {
    return null;
  }
}

export function setSession(session: AuthSession): void {
  if (typeof window === "undefined") return;
  window.localStorage.setItem(SESSION_KEY, JSON.stringify(session));
}

export function clearSession(): void {
  if (typeof window === "undefined") return;
  window.localStorage.removeItem(SESSION_KEY);
}

export function getHomeRouteForRole(role: UserRole): string {
  switch (role) {
    case "Citizen":
      return "/citizen";
    case "Volunteer":
      return "/volunteer";
    case "Staff":
    case "Coordinator":
      return "/staff";
    case "Admin":
      return "/admin";
  }
}
