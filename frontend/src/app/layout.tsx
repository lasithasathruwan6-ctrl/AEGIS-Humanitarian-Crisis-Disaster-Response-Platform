import type { Metadata } from "next";
import "./globals.css";
import { Navbar } from "@/components/Navbar";
import { I18nProvider } from "@/i18n/I18nProvider";

export const metadata: Metadata = {
  title: "Crisis Aid Platform",
  description: "Humanitarian Crisis Management and Aid Coordination Platform",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>
        <I18nProvider>
          <a href="#main-content" className="skip-link">
            Skip to main content
          </a>
          <Navbar />
          <main id="main-content" tabIndex={-1}>
            {children}
          </main>
        </I18nProvider>
      </body>
    </html>
  );
}
