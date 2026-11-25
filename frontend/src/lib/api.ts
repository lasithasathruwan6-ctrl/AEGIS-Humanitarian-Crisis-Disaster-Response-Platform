import { clearSession, getSession } from "./auth";

export class ApiError extends Error {
  status: number;
  constructor(message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL ?? "/api/v1";

export async function apiFetch<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const session = getSession();
  const headers = new Headers(options.headers);
  if (!headers.has("Content-Type") && !(options.body instanceof FormData)) {
    headers.set("Content-Type", "application/json");
  }
  if (session?.accessToken) {
    headers.set("Authorization", `Bearer ${session.accessToken}`);
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  });

  if (response.status === 401) {
    clearSession();
    if (typeof window !== "undefined") {
      window.dispatchEvent(new CustomEvent("crisis-aid:unauthorized"));
    }
    throw new ApiError("Session expired", 401);
  }

  if (!response.ok) {
    const text = await response.text().catch(() => "Request failed");
    throw new ApiError(text || "Request failed", response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
