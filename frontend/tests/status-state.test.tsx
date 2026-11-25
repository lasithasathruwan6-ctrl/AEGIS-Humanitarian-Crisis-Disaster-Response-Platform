import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { StatusState } from "@/components/StatusState";

describe("StatusState", () => {
  it("renders empty state with status role", () => {
    render(
      <StatusState
        variant="empty"
        title="No shelters available"
        message="No active shelters are listed right now."
      />
    );
    expect(screen.getByRole("status")).toHaveTextContent("No shelters available");
  });

  it("renders error state with alert role and invokes retry action", () => {
    const onRetry = vi.fn();
    render(
      <StatusState
        variant="error"
        title="Unable to load shelters"
        message="Network timeout"
        actionLabel="Retry"
        onAction={onRetry}
      />
    );
    expect(screen.getByRole("alert")).toHaveTextContent("Network timeout");
    fireEvent.click(screen.getByRole("button", { name: "Retry" }));
    expect(onRetry).toHaveBeenCalledTimes(1);
  });
});
