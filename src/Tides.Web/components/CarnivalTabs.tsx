"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const tabs = [
  { label: "Overview", href: "" },
  { label: "Results", href: "/results" },
  { label: "Draw", href: "/draw" },
  { label: "Leaderboard", href: "/leaderboard" },
];

export function CarnivalTabs({ carnivalId }: { carnivalId: string }) {
  const pathname = usePathname();
  const base = `/carnival/${carnivalId}`;

  return (
    <nav className="flex gap-0.5 overflow-x-auto scrollbar-none -mb-px">
      {tabs.map((tab) => {
        const href = `${base}${tab.href}`;
        const isActive =
          tab.href === "" ? pathname === base : pathname.startsWith(href);

        return (
          <Link
            key={tab.label}
            href={href}
            className={`relative whitespace-nowrap px-5 py-3 text-sm font-heading font-medium transition-colors duration-fast border-b-2 ${
              isActive
                ? "text-white border-white"
                : "text-tide-300/50 border-transparent hover:text-tide-200"
            }`}
          >
            {tab.label}
          </Link>
        );
      })}
    </nav>
  );
}
