import { readFileSync } from "node:fs";
import path from "node:path";
import { describe, expect, it } from "vitest";

describe("prefers-reduced-motion support", () => {
  it("disables animations, transitions, and smooth scroll when reduced motion is preferred", () => {
    const css = readFileSync(
      path.resolve(__dirname, "../src/app/globals.css"),
      "utf8"
    );
    expect(css).toContain("@media (prefers-reduced-motion: reduce)");
    expect(css).toContain("scroll-behavior: auto");
    expect(css).toContain("animation-duration: 0.01ms !important");
    expect(css).toContain("transition-duration: 0.01ms !important");
  });
});
