import { describe, expect, it } from "vitest";
import {
  validateEmail,
  validateNic,
  validatePhone,
  validateRegistrationFields,
} from "@/lib/validation";

describe("NIC, phone, and email validation", () => {
  it("accepts valid 9-digit+V/X and 12-digit Sri Lankan NICs", () => {
    expect(validateNic("901234567V")).toBeNull();
    expect(validateNic("901234567x")).toBeNull();
    expect(validateNic("199012345678")).toBeNull();
  });

  it("returns clear message for invalid or empty NIC", () => {
    expect(validateNic("")).toContain("required");
    expect(validateNic("12345")).toContain("valid Sri Lankan NIC");
  });

  it("validates local and +94 phone numbers", () => {
    expect(validatePhone("0771234567")).toBeNull();
    expect(validatePhone("+94 77 123 4567")).toBeNull();
    expect(validatePhone("1234")).toContain("valid phone number");
  });

  it("validates email format and aggregates form errors", () => {
    expect(validateEmail("citizen@example.org")).toBeNull();
    expect(validateEmail("bad-email")).toContain("valid email");

    const errors = validateRegistrationFields({
      nic: "bad",
      phone: "123",
      email: "nope",
    });
    expect(errors.nic).toBeTruthy();
    expect(errors.phone).toBeTruthy();
    expect(errors.email).toBeTruthy();
  });
});
