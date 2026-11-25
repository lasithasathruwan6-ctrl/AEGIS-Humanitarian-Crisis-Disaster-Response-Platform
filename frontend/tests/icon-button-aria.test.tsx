import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { IconButton } from "@/components/IconButton";
import { Navbar } from "@/components/Navbar";

describe("icon-only button aria-labels", () => {
  it("renders IconButton with an accessible name and aria-hidden icon", () => {
    render(<IconButton label="Dismiss alert" icon="✕" />);
    const btn = screen.getByRole("button", { name: "Dismiss alert" });
    expect(btn).toBeInTheDocument();
    expect(btn.querySelector('[aria-hidden="true"]')).toHaveTextContent("✕");
  });

  it("toggles Navbar menu button aria-label and aria-expanded", () => {
    render(<Navbar />);
    const toggle = screen.getByRole("button", { name: "Open navigation menu" });
    expect(toggle).toHaveAttribute("aria-expanded", "false");

    fireEvent.click(toggle);
    expect(
      screen.getByRole("button", { name: "Close navigation menu" })
    ).toHaveAttribute("aria-expanded", "true");
  });
});
