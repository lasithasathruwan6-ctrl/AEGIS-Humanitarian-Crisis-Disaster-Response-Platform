import { fireEvent, render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it } from "vitest";
import { I18nProvider, useI18n } from "@/i18n/I18nProvider";

function LanguageProbe() {
  const { locale, setLocale, t } = useI18n();
  return (
    <div>
      <span data-testid="locale">{locale}</span>
      <span data-testid="title">{t("citizenPortalTitle")}</span>
      <button type="button" onClick={() => setLocale("ta")}>
        Switch Tamil
      </button>
    </div>
  );
}

describe("language choice persistence", () => {
  beforeEach(() => {
    window.localStorage.clear();
    document.documentElement.lang = "en";
  });

  it("saves locale to localStorage and restores it on a new visit", () => {
    const { unmount } = render(
      <I18nProvider>
        <LanguageProbe />
      </I18nProvider>
    );

    fireEvent.click(screen.getByRole("button", { name: "Switch Tamil" }));
    expect(screen.getByTestId("locale")).toHaveTextContent("ta");
    expect(document.documentElement.lang).toBe("ta");
    expect(window.localStorage.getItem("crisis_aid_locale")).toBe("ta");

    unmount();

    render(
      <I18nProvider>
        <LanguageProbe />
      </I18nProvider>
    );
    expect(screen.getByTestId("locale")).toHaveTextContent("ta");
    expect(screen.getByTestId("title")).toHaveTextContent("குடிமக்கள் தளம்");
  });
});
