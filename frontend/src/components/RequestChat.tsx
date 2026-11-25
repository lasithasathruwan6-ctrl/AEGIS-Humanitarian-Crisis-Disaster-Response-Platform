"use client";

import { useEffect, useRef, useState, type FormEvent } from "react";
import { apiFetch } from "@/lib/api";

export interface ChatMessage {
  id: string;
  sender: string;
  body: string;
  createdAt: string;
  status?: "sending" | "sent" | "failed";
}

interface RequestChatProps {
  requestId: string;
  initialMessages?: ChatMessage[];
}

export function RequestChat({
  requestId,
  initialMessages = [],
}: RequestChatProps) {
  const [messages, setMessages] = useState<ChatMessage[]>(initialMessages);
  const [draft, setDraft] = useState("");
  const endRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    setMessages(initialMessages);
  }, [initialMessages]);

  useEffect(() => {
    endRef.current?.scrollIntoView?.({ behavior: "smooth" });
  }, [messages.length]);

  async function dispatchMessage(msg: ChatMessage) {
    try {
      const saved = await apiFetch<ChatMessage>(
        `/requests/${requestId}/messages`,
        {
          method: "POST",
          body: JSON.stringify({ body: msg.body }),
        }
      );
      setMessages((prev) =>
        prev.map((m) => (m.id === msg.id ? { ...saved, status: "sent" } : m))
      );
    } catch {
      setMessages((prev) =>
        prev.map((m) => (m.id === msg.id ? { ...m, status: "failed" } : m))
      );
    }
  }

  function handleSend(e: FormEvent) {
    e.preventDefault();
    const trimmed = draft.trim();
    if (!trimmed) return;

    const optimistic: ChatMessage = {
      id: `temp-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
      sender: "You",
      body: trimmed,
      createdAt: new Date().toISOString(),
      status: "sending",
    };

    setMessages((prev) => [...prev, optimistic]);
    setDraft("");
    void dispatchMessage(optimistic);
  }

  function handleRetry(msg: ChatMessage) {
    setMessages((prev) =>
      prev.map((m) => (m.id === msg.id ? { ...m, status: "sending" } : m))
    );
    void dispatchMessage(msg);
  }

  return (
    <section className="card" aria-label="Request conversation">
      <h2 style={{ marginTop: 0 }}>Messages</h2>
      <div
        data-testid="chat-scroll-region"
        style={{
          maxHeight: 320,
          overflowY: "auto",
          display: "flex",
          flexDirection: "column",
          gap: "var(--space-3)",
          padding: "var(--space-3)",
          background: "var(--color-surface-alt)",
          borderRadius: "var(--radius-md)",
          marginBottom: "var(--space-4)",
        }}
      >
        {messages.length === 0 && (
          <p className="muted" style={{ margin: 0 }}>
            No messages yet.
          </p>
        )}
        {messages.map((msg) => (
          <div
            key={msg.id}
            style={{
              background: "var(--color-surface)",
              border: "1px solid var(--color-border)",
              borderRadius: "var(--radius-md)",
              padding: "var(--space-3)",
            }}
          >
            <div style={{ display: "flex", justifyContent: "space-between", gap: "var(--space-2)" }}>
              <strong>{msg.sender}</strong>
              {msg.status === "sending" && <span className="muted">Sending…</span>}
              {msg.status === "failed" && (
                <span style={{ color: "var(--color-danger)", fontSize: "0.875rem" }}>
                  Failed to send
                </span>
              )}
            </div>
            <p style={{ margin: "var(--space-1) 0 0" }}>{msg.body}</p>
            {msg.status === "failed" && (
              <button
                type="button"
                className="btn btn-secondary"
                style={{ marginTop: "var(--space-2)" }}
                onClick={() => handleRetry(msg)}
              >
                Retry
              </button>
            )}
          </div>
        ))}
        <div ref={endRef} />
      </div>

      <form onSubmit={handleSend} style={{ display: "flex", gap: "var(--space-2)" }}>
        <label htmlFor="chat-message-input" className="skip-link">
          Message
        </label>
        <input
          id="chat-message-input"
          className="form-input"
          placeholder="Write a message…"
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
        />
        <button type="submit" className="btn btn-primary">
          Send
        </button>
      </form>
    </section>
  );
}
