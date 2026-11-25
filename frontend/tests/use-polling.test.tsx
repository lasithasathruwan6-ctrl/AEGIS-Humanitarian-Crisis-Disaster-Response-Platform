import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { computeBackoffDelay, usePolling } from "@/hooks/usePolling";

describe("usePolling hook", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.restoreAllMocks();
  });

  it("computes exponential back-off capped at maxIntervalMs", () => {
    expect(computeBackoffDelay(0, 1000, 10000)).toBe(1000);
    expect(computeBackoffDelay(1, 1000, 10000)).toBe(2000);
    expect(computeBackoffDelay(2, 1000, 10000)).toBe(4000);
    expect(computeBackoffDelay(5, 1000, 10000)).toBe(10000);
  });

  it("backs off on errors, resets on success, and aborts/cleans up on unmount", async () => {
    const signals: AbortSignal[] = [];
    const fetcher = vi
      .fn<(signal: AbortSignal) => Promise<string>>()
      .mockImplementationOnce(async (signal) => {
        signals.push(signal);
        throw new Error("temporary outage");
      })
      .mockImplementation(async (signal) => {
        signals.push(signal);
        return "ok";
      });

    const { result, unmount } = renderHook(() =>
      usePolling({
        fetcher,
        baseIntervalMs: 1000,
        maxIntervalMs: 8000,
      })
    );

    await act(async () => {
      await Promise.resolve();
    });
    expect(result.current.error).toBe("temporary outage");
    expect(fetcher).toHaveBeenCalledTimes(1);

    // After 1 failure, next poll is at 2000ms, not 1000ms
    await act(async () => {
      vi.advanceTimersByTime(1000);
      await Promise.resolve();
    });
    expect(fetcher).toHaveBeenCalledTimes(1);

    await act(async () => {
      vi.advanceTimersByTime(1000);
      await Promise.resolve();
    });
    expect(fetcher).toHaveBeenCalledTimes(2);
    expect(result.current.data).toBe("ok");
    expect(result.current.error).toBeNull();

    unmount();
    expect(signals.at(-1)?.aborted).toBe(true);

    await act(async () => {
      vi.advanceTimersByTime(5000);
      await Promise.resolve();
    });
    expect(fetcher).toHaveBeenCalledTimes(2);
  });
});
