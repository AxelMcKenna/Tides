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
    <nav className="flex gap-1 overflow-x-auto scrollbar-none mb-6 border-b border-ink-200 pb-px">
      {tabs.map((tab) => {
        const href = `${base}${tab.href}`;
        const isActive =
          tab.href === "" ? pathname === base : pathname.startsWith(href);

        return (
          <Link
            key={tab.label}
            href={href}
            aria-current={isActive ? "page" : undefined}
            className={`relative whitespace-nowrap px-4 py-2.5 min-h-[44px] flex items-center text-sm font-heading font-semibold transition-colors duration-fast ${
              isActive
                ? "text-tide-700"
                : "text-ink-400 hover:text-ink-700"
            }`}
          >
            {tab.label}
            {isActive && (
              <span className="absolute bottom-0 left-2 right-2 h-[2.5px] rounded-full bg-tide-600" />
            )}
          </Link>
        );
      })}
    </nav>
  );
}
