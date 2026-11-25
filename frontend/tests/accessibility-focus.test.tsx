import { readFileSync } from "node:fs";
import path from "node:path";
import { describe, expect, it } from "vitest";

describe("visible focus styles", () => {
  it("defines :focus-visible outline rules for interactive elements in globals.css", () => {
    const css = readFileSync(
      path.resolve(__dirname, "../src/app/globals.css"),
      "utf8"
    );
    expect(css).toContain(":focus-visible");
    expect(css).toContain("a:focus-visible");
    expect(css).toContain("button:focus-visible");
    expect(css).toContain("input:focus-visible");
    expect(css).toContain("select:focus-visible");
    expect(css).toContain("textarea:focus-visible");
    expect(css).toContain("outline: 3px solid");
  });

  it("includes skip-to-content link in root layout", () => {
    const layout = readFileSync(
      path.resolve(__dirname, "../src/app/layout.tsx"),
      "utf8"
    );
    expect(layout).toContain('className="skip-link"');
    expect(layout).toContain('href="#main-content"');
  });
});
