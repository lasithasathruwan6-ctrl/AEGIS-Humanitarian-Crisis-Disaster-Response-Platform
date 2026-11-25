import { readFileSync } from "node:fs";
import path from "node:path";
import { describe, expect, it } from "vitest";

describe("small phone request form layout", () => {
  it("stacks form-grid and full-width form-actions below 480px", () => {
    const css = readFileSync(
      path.resolve(__dirname, "../src/app/globals.css"),
      "utf8"
    );
    expect(css).toContain("@media (max-width: 480px)");
    expect(css).toContain(".form-grid");
    expect(css).toContain("grid-template-columns: 1fr");
    expect(css).toContain(".form-actions .btn");
  });

  it("uses form-grid and form-actions on the new request page", () => {
    const page = readFileSync(
      path.resolve(__dirname, "../src/app/citizen/requests/new/page.tsx"),
      "utf8"
    );
    expect(page).toContain('className="form-grid"');
    expect(page).toContain('className="form-actions"');
  });
});
