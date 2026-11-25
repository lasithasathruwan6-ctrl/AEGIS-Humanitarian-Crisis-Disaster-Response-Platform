"use client";

import { useCallback, useEffect, useRef, useState } from "react";

export interface UsePollingOptions<T> {
  fetcher: (signal: AbortSignal) => Promise<T>;
  baseIntervalMs?: number;
  maxIntervalMs?: number;
  enabled?: boolean;
}

export interface UsePollingResult<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
  retry: () => void;
}

export function computeBackoffDelay(
  failures: number,
  baseIntervalMs: number,
  maxIntervalMs: number
): number {
  if (failures <= 0) return baseIntervalMs;
  return Math.min(baseIntervalMs * 2 ** failures, maxIntervalMs);
}

export function usePolling<T>({
  fetcher,
  baseIntervalMs = 5000,
  maxIntervalMs = 60000,
  enabled = true,
}: UsePollingOptions<T>): UsePollingResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState<boolean>(enabled);
  const [error, setError] = useState<string | null>(null);

  const fetcherRef = useRef(fetcher);
  fetcherRef.current = fetcher;

  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const abortRef = useRef<AbortController | null>(null);
  const failuresRef = useRef(0);
  const mountedRef = useRef(true);

  const clearPending = useCallback(() => {
    if (timerRef.current !== null) {
      clearTimeout(timerRef.current);
      timerRef.current = null;
    }
    if (abortRef.current) {
      abortRef.current.abort();
      abortRef.current = null;
    }
  }, []);

  const runCycle = useCallback(async () => {
    if (!enabled || !mountedRef.current) return;

    abortRef.current?.abort();
    const controller = new AbortController();
    abortRef.current = controller;

    try {
      const result = await fetcherRef.current(controller.signal);
      if (!mountedRef.current || controller.signal.aborted) return;
      failuresRef.current = 0;
      setData(result);
      setError(null);
    } catch (err) {
      if (!mountedRef.current || controller.signal.aborted) return;
      failuresRef.current += 1;
      setError(err instanceof Error ? err.message : "Polling request failed");
    } finally {
      if (mountedRef.current && !controller.signal.aborted) {
        setLoading(false);
        const delay = computeBackoffDelay(
          failuresRef.current,
          baseIntervalMs,
          maxIntervalMs
        );
        timerRef.current = setTimeout(() => {
          void runCycle();
        }, delay);
      }
    }
  }, [baseIntervalMs, enabled, maxIntervalMs]);

  const retry = useCallback(() => {
    clearPending();
    failuresRef.current = 0;
    setLoading(true);
    setError(null);
    void runCycle();
  }, [clearPending, runCycle]);

  useEffect(() => {
    mountedRef.current = true;
    if (enabled) {
      setLoading(true);
      void runCycle();
    } else {
      clearPending();
      setLoading(false);
    }
    return () => {
      mountedRef.current = false;
      clearPending();
    };
  }, [clearPending, enabled, runCycle]);

  return { data, loading, error, retry };
}
