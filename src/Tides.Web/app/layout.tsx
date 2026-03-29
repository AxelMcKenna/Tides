import type { Metadata, Viewport } from "next";
import { DM_Sans, Geist, Geist_Mono } from "next/font/google";
import "./globals.css";

const dmSans = DM_Sans({
  subsets: ["latin"],
  weight: ["500", "600", "700"],
  variable: "--font-heading",
  display: "swap",
});

const geist = Geist({
  subsets: ["latin"],
  variable: "--font-body",
  display: "swap",
});

const geistMono = Geist_Mono({
  subsets: ["latin"],
  variable: "--font-data",
  display: "swap",
});

export const metadata: Metadata = {
  title: "Shore — Live Results",
  description: "Surf Life Saving competition results — live heat draws, placings, and club standings.",
};

export const viewport: Viewport = {
  themeColor: "#101828",
  width: "device-width",
  initialScale: 1,
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html
      lang="en"
      className={`${dmSans.variable} ${geist.variable} ${geistMono.variable}`}
    >
      <body className="bg-ink-50 text-ink-900">
        <a
          href="#main-content"
          className="sr-only focus:not-sr-only focus:fixed focus:top-4 focus:left-4 focus:z-50 focus:rounded-md focus:bg-tide-500 focus:px-4 focus:py-2 focus:text-white focus:text-sm focus:font-medium"
        >
          Skip to content
        </a>
        {children}
      </body>
    </html>
  );
}
