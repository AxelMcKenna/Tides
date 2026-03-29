"use client";

import { useState } from "react";
import Link from "next/link";

interface CarnivalItem {
  id: string;
  name: string;
  sanction: string;
  startDate: string;
  endDate: string;
  eventCount: number;
  hasResults: boolean;
}

import { formatDateRange } from "@/lib/dates";

export function CarnivalSearch({ carnivals }: { carnivals: CarnivalItem[] }) {
  const [query, setQuery] = useState("");

  const today = new Date().toISOString().split("T")[0];

  const filtered = query
    ? carnivals.filter(
        (c) =>
          c.name.toLowerCase().includes(query.toLowerCase()) ||
          c.sanction.toLowerCase().includes(query.toLowerCase())
      )
    : carnivals;

  const live = filtered.filter((c) => c.startDate <= today && c.endDate >= today);
  const upcoming = filtered.filter((c) => c.startDate > today);
  const past = filtered.filter((c) => c.endDate < today);

  return (
    <div className="space-y-8">
      {/* Search */}
      <div className="relative">
        <svg
          className="absolute left-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-ink-400"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={2}
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z"
          />
        </svg>
        <input
          type="text"
          placeholder="Search carnivals..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          className="w-full bg-white rounded-[--radius-md] border border-ink-200 pl-10 pr-4 py-2.5 min-h-[44px] text-sm text-ink-900 placeholder:text-ink-400 shadow-card focus:outline-none focus:ring-2 focus:ring-tide-500/20 focus:border-tide-500 transition-all"
        />
      </div>

      {/* Results */}
      {filtered.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-ink-400 font-heading text-sm">
            No carnivals matching &ldquo;{query}&rdquo;
          </p>
        </div>
      ) : (
        <>
          {live.length > 0 && <Section title="Live Now" carnivals={live} isLive />}
          {upcoming.length > 0 && <Section title="Upcoming" carnivals={upcoming} />}
          {past.length > 0 && <Section title="Completed" carnivals={past} />}
        </>
      )}
    </div>
  );
}

function Section({
  title,
  carnivals,
  isLive,
}: {
  title: string;
  carnivals: CarnivalItem[];
  isLive?: boolean;
}) {
  return (
    <section>
      <div className="flex items-center gap-2.5 mb-4">
        <span className={`h-4 w-1.5 rounded-full ${isLive ? "bg-live" : "bg-tide-600"}`} />
        <h2 className="text-xs font-heading font-bold uppercase tracking-[0.15em] text-ink-700">
          {title}
        </h2>
      </div>
      <div className="space-y-3">
        {carnivals.map((c, i) => (
          <Link
            key={c.id}
            href={`/carnival/${c.id}`}
            className={`group flex items-center justify-between bg-white rounded-[--radius-lg] border border-ink-200 border-l-4 ${isLive ? "border-l-live" : "border-l-tide-600"} px-5 py-4.5 shadow-card hover:shadow-float hover:-translate-y-px transition-all duration-standard animate-fade-up press-card`}
            style={{ animationDelay: `${i * 50}ms` }}
          >
            <div className="flex items-center gap-4">
              {isLive && (
                <div className="shrink-0 h-10 w-10 rounded-full bg-live/10 flex items-center justify-center">
                  <span className="relative flex h-3 w-3">
                    <span className="absolute inline-flex h-full w-full rounded-full bg-live opacity-60 live-dot" />
                    <span className="relative inline-flex h-3 w-3 rounded-full bg-live" />
                  </span>
                </div>
              )}
              <div>
                <div className="flex items-center gap-2.5">
                  <p className="font-heading font-bold text-ink-900 group-hover:text-tide-700 transition-colors">
                    {c.name}
                  </p>
                  {isLive && (
                    <span className="text-[10px] font-heading font-bold uppercase tracking-wider text-live bg-live/10 px-2 py-0.5 rounded-full">
                      Live
                    </span>
                  )}
                </div>
                <p className="text-sm text-ink-500 font-heading mt-0.5">
                  {formatDateRange(c.startDate, c.endDate)}
                  <span className="mx-2 text-ink-300">|</span>
                  <span className="text-xs uppercase tracking-wider">{c.sanction}</span>
                </p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <div className="text-right">
                <span className="text-xs font-heading text-ink-500">
                  {c.eventCount} event{c.eventCount !== 1 ? "s" : ""}
                </span>
                {c.hasResults && (
                  <span className="ml-2 text-[11px] font-heading font-bold text-signal-green bg-signal-green/12 px-2.5 py-0.5 rounded-full">
                    Results
                  </span>
                )}
              </div>
              <svg
                className="h-4 w-4 text-ink-300 group-hover:text-tide-500 transition-colors"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                strokeWidth={2}
              >
                <path strokeLinecap="round" strokeLinejoin="round" d="m8.25 4.5 7.5 7.5-7.5 7.5" />
              </svg>
            </div>
          </Link>
        ))}
      </div>
    </section>
  );
}
