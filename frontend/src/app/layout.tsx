import type { Metadata } from "next";
import "./globals.css";
import { Navbar } from "@/components/Navbar";

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
        <Navbar />
        <main id="main-content">{children}</main>
      </body>
    </html>
  );
}
