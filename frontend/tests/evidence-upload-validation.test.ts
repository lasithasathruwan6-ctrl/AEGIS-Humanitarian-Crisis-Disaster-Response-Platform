import { describe, expect, it } from "vitest";
import {
  MAX_EVIDENCE_SIZE_BYTES,
  validateEvidenceFile,
} from "@/lib/validation";

describe("evidence upload validation", () => {
  it("accepts JPEG, PNG, WebP, and PDF files within 5 MB", () => {
    expect(
      validateEvidenceFile({ name: "photo.jpg", type: "image/jpeg", size: 1024 })
    ).toBeNull();
    expect(
      validateEvidenceFile({ name: "scan.png", type: "image/png", size: 2048 })
    ).toBeNull();
    expect(
      validateEvidenceFile({ name: "damage.webp", type: "image/webp", size: 4096 })
    ).toBeNull();
    expect(
      validateEvidenceFile({
        name: "report.pdf",
        type: "application/pdf",
        size: MAX_EVIDENCE_SIZE_BYTES,
      })
    ).toBeNull();
  });

  it("rejects unsupported MIME types", () => {
    const err = validateEvidenceFile({
      name: "payload.exe",
      type: "application/x-msdownload",
      size: 1024,
    });
    expect(err).toContain("Unsupported file type");
  });

  it("rejects empty files and files larger than 5 MB", () => {
    expect(
      validateEvidenceFile({ name: "empty.png", type: "image/png", size: 0 })
    ).toContain("empty");
    expect(
      validateEvidenceFile({
        name: "huge.jpg",
        type: "image/jpeg",
        size: MAX_EVIDENCE_SIZE_BYTES + 1,
      })
    ).toContain("5 MB limit");
  });
});
