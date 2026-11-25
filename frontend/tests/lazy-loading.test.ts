import { readFileSync } from "node:fs";
import path from "node:path";
import { describe, expect, it } from "vitest";

describe("lazy-loaded heavy components", () => {
  it("lazy-loads ShelterMap via next/dynamic on the shelters page", () => {
    const sheltersPage = readFileSync(
      path.resolve(__dirname, "../src/app/shelters/page.tsx"),
      "utf8"
    );
    expect(sheltersPage).toContain('from "next/dynamic"');
    expect(sheltersPage).toContain('import("@/components/ShelterMap")');
  });

  it("lazy-loads RequestChat via next/dynamic on the request detail page", () => {
    const detailPage = readFileSync(
      path.resolve(__dirname, "../src/app/citizen/requests/[id]/page.tsx"),
      "utf8"
    );
    expect(detailPage).toContain('from "next/dynamic"');
    expect(detailPage).toContain('import("@/components/RequestChat")');
  });
});
