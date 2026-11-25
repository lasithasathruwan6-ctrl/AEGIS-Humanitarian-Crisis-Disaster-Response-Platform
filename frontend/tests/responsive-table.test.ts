import { readFileSync } from "node:fs";
import path from "node:path";
import { describe, expect, it } from "vitest";

describe("responsive admin and staff tables", () => {
  it("defines .table-responsive scroll and stacked mobile .data-table rules", () => {
    const css = readFileSync(
      path.resolve(__dirname, "../src/app/globals.css"),
      "utf8"
    );
    expect(css).toContain(".table-responsive");
    expect(css).toContain("overflow-x: auto");
    expect(css).toContain("@media (max-width: 640px)");
    expect(css).toContain("content: attr(data-label)");
  });

  it("uses .table-responsive and data-label cells in admin and staff pages", () => {
    const admin = readFileSync(
      path.resolve(__dirname, "../src/app/admin/page.tsx"),
      "utf8"
    );
    const staff = readFileSync(
      path.resolve(__dirname, "../src/app/staff/page.tsx"),
      "utf8"
    );
    expect(admin).toContain('className="table-responsive"');
    expect(admin).toContain('data-label="Actor"');
    expect(staff).toContain('className="table-responsive"');
    expect(staff).toContain('data-label="Title"');
  });
});
