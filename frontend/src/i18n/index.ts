import { en, type Dictionary, type TranslationKey } from "./en";
import { si } from "./si";
import { ta } from "./ta";

export type Locale = "en" | "si" | "ta";

export const LOCALE_LABELS: Record<Locale, string> = {
  en: "English",
  si: "සිංහල",
  ta: "தமிழ்",
};

const dictionaries: Record<Locale, Dictionary> = {
  en,
  si,
  ta,
};

export function getDictionary(locale: Locale): Dictionary {
  return dictionaries[locale] ?? en;
}

export type { Dictionary, TranslationKey };
