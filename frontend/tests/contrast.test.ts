import { readFileSync } from "node:fs";
import path from "node:path";
import { describe, expect, it } from "vitest";

function hexToRgb(hex: string): [number, number, number] {
  const clean = hex.replace("#", "");
  return [
    parseInt(clean.slice(0, 2), 16),
    parseInt(clean.slice(2, 4), 16),
    parseInt(clean.slice(4, 6), 16),
  ];
}

function relativeLuminance(hex: string): number {
  const [r, g, b] = hexToRgb(hex).map((c) => {
    const s = c / 255;
    return s <= 0.03928 ? s / 12.92 : Math.pow((s + 0.055) / 1.055, 2.4);
  });
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}

function contrastRatio(fg: string, bg: string): number {
  const l1 = relativeLuminance(fg);
  const l2 = relativeLuminance(bg);
  const lighter = Math.max(l1, l2);
  const darker = Math.min(l1, l2);
  return (lighter + 0.05) / (darker + 0.05);
}

function parseTokens(): Record<string, string> {
  const css = readFileSync(
    path.resolve(__dirname, "../src/styles/tokens.css"),
    "utf8"
  );
  const tokens: Record<string, string> = {};
  for (const match of css.matchAll(/(--[a-z-]+):\s*(#[0-9a-fA-F]{6})/g)) {
    tokens[match[1]] = match[2];
  }
  return tokens;
}

describe("WCAG AA colour contrast", () => {
  const tokens = parseTokens();

  const pairs: Array<[string, string]> = [
    ["--color-text", "--color-surface"],
    ["--color-text", "--color-bg"],
    ["--color-text-muted", "--color-surface"],
    ["--color-text-muted", "--color-bg"],
    ["--color-text-inverse", "--color-primary"],
    ["--color-danger", "--color-danger-soft"],
    ["--color-warning", "--color-warning-soft"],
    ["--color-success", "--color-success-soft"],
    ["--color-info", "--color-info-soft"],
  ];

  it.each(pairs)("%s on %s meets 4.5:1 contrast ratio", (fgKey, bgKey) => {
    const ratio = contrastRatio(tokens[fgKey], tokens[bgKey]);
    expect(ratio).toBeGreaterThanOrEqual(4.5);
  });
});
