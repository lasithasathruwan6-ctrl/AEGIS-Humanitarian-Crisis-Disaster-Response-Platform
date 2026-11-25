import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { SessionExpiredModal } from "@/components/SessionExpiredModal";
import { apiFetch } from "@/lib/api";

const replaceMock = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace: replaceMock }),
  usePathname: () => "/citizen",
}));

describe("SessionExpiredModal on 401 responses", () => {
  beforeEach(() => {
    replaceMock.mockReset();
    vi.restoreAllMocks();
  });

  it("opens modal when apiFetch receives HTTP 401 and redirects on Sign in again", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(null, { status: 401 })
    );

    render(<SessionExpiredModal />);
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();

    await expect(apiFetch("/requests")).rejects.toThrow("Session expired");

    const dialog = await screen.findByRole("dialog");
    expect(dialog).toHaveAttribute("aria-modal", "true");
    expect(dialog).toHaveTextContent("Session Expired");

    fireEvent.click(screen.getByRole("button", { name: "Sign in again" }));
    await waitFor(() => {
      expect(replaceMock).toHaveBeenCalledWith("/login?next=%2Fcitizen");
    });
  });
});
