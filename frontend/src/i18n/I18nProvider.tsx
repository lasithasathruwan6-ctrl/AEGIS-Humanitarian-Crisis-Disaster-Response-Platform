"use client";

import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import { getDictionary, type Locale, type TranslationKey } from "./index";

interface I18nContextValue {
  locale: Locale;
  setLocale: (next: Locale) => void;
  t: (key: TranslationKey) => string;
}

const I18nContext = createContext<I18nContextValue | null>(null);

export function I18nProvider({ children }: { children: ReactNode }) {
  const [locale, setLocale] = useState<Locale>("en");

  const value = useMemo<I18nContextValue>(() => {
    const dict = getDictionary(locale);
    return {
      locale,
      setLocale,
      t: (key) => dict[key],
    };
  }, [locale]);

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useI18n(): I18nContextValue {
  const ctx = useContext(I18nContext);
  if (!ctx) {
    const fallback = getDictionary("en");
    return {
      locale: "en",
      setLocale: () => {},
      t: (key) => fallback[key],
    };
  }
  return ctx;
}
