import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import RegisterPage from "@/app/register/page";
import SheltersPage from "@/app/shelters/page";
import { Navbar } from "@/components/Navbar";
import { I18nProvider } from "@/i18n/I18nProvider";

const replaceMock = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace: replaceMock }),
  usePathname: () => "/register",
}));

describe("three key frontend components", () => {
  beforeEach(() => {
    replaceMock.mockReset();
    vi.restoreAllMocks();
    window.localStorage.clear();
  });

  it("Navbar switches UI language across navigation links", () => {
    render(
      <I18nProvider>
        <Navbar />
      </I18nProvider>
    );

    expect(screen.getByRole("link", { name: "Shelters" })).toBeInTheDocument();
    fireEvent.change(screen.getByLabelText("Language"), {
      target: { value: "si" },
    });
    expect(
      screen.getByRole("link", { name: "ආරක්ෂිත ස්ථාන" })
    ).toBeInTheDocument();
  });

  it("RegisterPage blocks invalid NIC/phone/email and submits valid registration", async () => {
    const fetchSpy = vi
      .spyOn(globalThis, "fetch")
      .mockResolvedValue(new Response(null, { status: 204 }));

    render(<RegisterPage />);

    fireEvent.change(screen.getByLabelText("Full name"), {
      target: { value: "Nimal Perera" },
    });
    fireEvent.change(screen.getByLabelText("NIC"), {
      target: { value: "invalid" },
    });
    fireEvent.change(screen.getByLabelText("Phone"), {
      target: { value: "123" },
    });
    fireEvent.change(screen.getByLabelText("Email"), {
      target: { value: "bad" },
    });
    fireEvent.change(screen.getByLabelText("Password"), {
      target: { value: "StrongPass!123" },
    });

    fireEvent.click(screen.getByRole("button", { name: "Create account" }));
    expect(fetchSpy).not.toHaveBeenCalled();
    expect(screen.getAllByRole("alert").length).toBeGreaterThanOrEqual(3);

    fireEvent.change(screen.getByLabelText("NIC"), {
      target: { value: "199512345678" },
    });
    fireEvent.change(screen.getByLabelText("Phone"), {
      target: { value: "0771234567" },
    });
    fireEvent.change(screen.getByLabelText("Email"), {
      target: { value: "nimal@example.lk" },
    });

    fireEvent.click(screen.getByRole("button", { name: "Create account" }));
    await waitFor(() => {
      expect(replaceMock).toHaveBeenCalledWith("/login");
    });
  });

  it("SheltersPage renders fetched shelter cards and occupancy", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(
        JSON.stringify([
          {
            id: "sh-1",
            name: "Town Hall Relief Center",
            district: "Galle",
            capacity: 200,
            occupied: 85,
          },
        ]),
        { status: 200, headers: { "Content-Type": "application/json" } }
      )
    );

    render(<SheltersPage />);
    expect(
      await screen.findByText("Town Hall Relief Center")
    ).toBeInTheDocument();
    expect(screen.getByText("Occupancy: 85 / 200")).toBeInTheDocument();
  });
});
