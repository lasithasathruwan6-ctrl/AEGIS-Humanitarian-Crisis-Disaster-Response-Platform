import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { LoadingSkeleton } from "@/components/LoadingSkeleton";

describe("LoadingSkeleton", () => {
  it("renders accessible status with aria-busy and requested card count", () => {
    const { container } = render(
      <LoadingSkeleton variant="cards" count={4} label="Loading shelters" />
    );
    const status = screen.getByRole("status", { name: "Loading shelters" });
    expect(status).toHaveAttribute("aria-busy", "true");
    expect(container.querySelectorAll(".card")).toHaveLength(4);
  });

  it("renders table rows when variant is table", () => {
    const { container } = render(
      <LoadingSkeleton variant="table" count={5} label="Loading queue" />
    );
    const status = screen.getByRole("status", { name: "Loading queue" });
    expect(status).toHaveAttribute("aria-busy", "true");
    expect(container.querySelectorAll(".skeleton-box")).toHaveLength(5);
  });
});
