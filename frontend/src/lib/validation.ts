const OLD_NIC_REGEX = /^[0-9]{9}[vVxX]$/;
const NEW_NIC_REGEX = /^[0-9]{12}$/;
const SL_PHONE_REGEX = /^(?:\+94|0)[1-9][0-9]{8}$/;
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function validateNic(value: string): string | null {
  const trimmed = value.trim();
  if (!trimmed) return "NIC is required.";
  if (!OLD_NIC_REGEX.test(trimmed) && !NEW_NIC_REGEX.test(trimmed)) {
    return "Enter a valid Sri Lankan NIC (e.g. 901234567V or 199012345678).";
  }
  return null;
}

export function validatePhone(value: string): string | null {
  const normalized = value.replace(/[\s-]/g, "");
  if (!normalized) return "Phone number is required.";
  if (!SL_PHONE_REGEX.test(normalized)) {
    return "Enter a valid phone number (e.g. 0771234567 or +94771234567).";
  }
  return null;
}

export function validateEmail(value: string): string | null {
  const trimmed = value.trim();
  if (!trimmed) return "Email address is required.";
  if (!EMAIL_REGEX.test(trimmed)) {
    return "Enter a valid email address (e.g. name@example.com).";
  }
  return null;
}

export interface RegistrationErrors {
  nic?: string;
  phone?: string;
  email?: string;
}

export function validateRegistrationFields(input: {
  nic: string;
  phone: string;
  email: string;
}): RegistrationErrors {
  const errors: RegistrationErrors = {};
  const nicErr = validateNic(input.nic);
  const phoneErr = validatePhone(input.phone);
  const emailErr = validateEmail(input.email);

  if (nicErr) errors.nic = nicErr;
  if (phoneErr) errors.phone = phoneErr;
  if (emailErr) errors.email = emailErr;

  return errors;
}
