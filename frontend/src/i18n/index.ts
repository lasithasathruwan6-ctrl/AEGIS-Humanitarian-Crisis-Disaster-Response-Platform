import { en, type Dictionary, type TranslationKey } from "./en";
import { si } from "./si";

export type Locale = "en" | "si";

export const LOCALE_LABELS: Record<Locale, string> = {
  en: "English",
  si: "සිංහල",
};

const dictionaries: Record<Locale, Dictionary> = {
  en,
  si,
};

export function getDictionary(locale: Locale): Dictionary {
  return dictionaries[locale] ?? en;
}

export type { Dictionary, TranslationKey };
