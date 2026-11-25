import { describe, expect, it } from "vitest";
import { en } from "@/i18n/en";
import { si } from "@/i18n/si";

describe("Sinhala citizen translations", () => {
  it("provides a non-empty Sinhala string for every English key", () => {
    for (const key of Object.keys(en) as Array<keyof typeof en>) {
      expect(si[key]).toBeTruthy();
      expect(si[key].trim().length).toBeGreaterThan(0);
    }
  });
});
