import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { RequestChat } from "@/components/RequestChat";

describe("RequestChat", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
    Element.prototype.scrollIntoView = vi.fn();
  });

  it("auto-scrolls, optimistically appends, and retries failed sends", async () => {
    const fetchSpy = vi
      .spyOn(globalThis, "fetch")
      .mockResolvedValueOnce(new Response("Network error", { status: 500 }))
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            id: "srv-1",
            sender: "You",
            body: "Need clean water",
            createdAt: "2026-11-25T10:00:00Z",
          }),
          { status: 200, headers: { "Content-Type": "application/json" } }
        )
      );

    render(<RequestChat requestId="req-42" />);

    fireEvent.change(screen.getByPlaceholderText("Write a message…"), {
      target: { value: "Need clean water" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Send" }));

    expect(screen.getByText("Need clean water")).toBeInTheDocument();
    expect(Element.prototype.scrollIntoView).toHaveBeenCalled();

    const retryBtn = await screen.findByRole("button", { name: "Retry" });
    expect(screen.getByText("Failed to send")).toBeInTheDocument();

    fireEvent.click(retryBtn);

    await waitFor(() => {
      expect(screen.queryByText("Failed to send")).not.toBeInTheDocument();
    });
    expect(fetchSpy).toHaveBeenCalledTimes(2);
    expect(screen.getAllByText("Need clean water")).toHaveLength(1);
  });
});
