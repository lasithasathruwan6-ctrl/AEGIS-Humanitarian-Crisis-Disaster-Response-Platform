import { render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { RoleGuard } from "@/components/RoleGuard";
import { clearSession, setSession } from "@/lib/auth";

const replaceMock = vi.fn();
let mockPathname = "/citizen";

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace: replaceMock }),
  usePathname: () => mockPathname,
}));

describe("RoleGuard redirects", () => {
  beforeEach(() => {
    clearSession();
    replaceMock.mockReset();
  });

  it("redirects unauthenticated users from /citizen to /login?next=%2Fcitizen", async () => {
    mockPathname = "/citizen";
    render(
      <RoleGuard allowedRoles={["Citizen"]}>
        <div>Citizen Secret</div>
      </RoleGuard>
    );

    await waitFor(() => {
      expect(replaceMock).toHaveBeenCalledWith("/login?next=%2Fcitizen");
    });
    expect(screen.queryByText("Citizen Secret")).not.toBeInTheDocument();
  });

  it("redirects unauthenticated users from /staff to /login?next=%2Fstaff", async () => {
    mockPathname = "/staff";
    render(
      <RoleGuard allowedRoles={["Staff", "Coordinator", "Admin"]}>
        <div>Staff Queue</div>
      </RoleGuard>
    );

    await waitFor(() => {
      expect(replaceMock).toHaveBeenCalledWith("/login?next=%2Fstaff");
    });
  });

  it("redirects a Citizen accessing Staff area to /citizen", async () => {
    mockPathname = "/staff";
    setSession({ accessToken: "t1", role: "Citizen", userId: "u1" });
    render(
      <RoleGuard allowedRoles={["Staff", "Coordinator", "Admin"]}>
        <div>Staff Queue</div>
      </RoleGuard>
    );

    await waitFor(() => {
      expect(replaceMock).toHaveBeenCalledWith("/citizen");
    });
  });

  it("redirects a Staff user accessing Citizen area to /staff", async () => {
    mockPathname = "/citizen";
    setSession({ accessToken: "t2", role: "Staff", userId: "u2" });
    render(
      <RoleGuard allowedRoles={["Citizen"]}>
        <div>Citizen Portal</div>
      </RoleGuard>
    );

    await waitFor(() => {
      expect(replaceMock).toHaveBeenCalledWith("/staff");
    });
  });

  it("renders protected content when role matches", async () => {
    mockPathname = "/staff";
    setSession({ accessToken: "t3", role: "Staff", userId: "u3" });
    render(
      <RoleGuard allowedRoles={["Staff", "Coordinator", "Admin"]}>
        <div>Staff Queue</div>
      </RoleGuard>
    );

    expect(await screen.findByText("Staff Queue")).toBeInTheDocument();
    expect(replaceMock).not.toHaveBeenCalled();
  });
});
