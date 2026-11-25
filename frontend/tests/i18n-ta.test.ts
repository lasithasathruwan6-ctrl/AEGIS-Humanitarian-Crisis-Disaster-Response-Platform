import { describe, expect, it } from "vitest";
import { en } from "@/i18n/en";
import { ta } from "@/i18n/ta";

describe("Tamil citizen translations", () => {
  it("provides a non-empty Tamil string for every English key", () => {
    for (const key of Object.keys(en) as Array<keyof typeof en>) {
      expect(ta[key]).toBeTruthy();
      expect(ta[key].trim().length).toBeGreaterThan(0);
    }
  });
});
