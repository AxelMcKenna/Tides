"use client";

import { useState, useMemo } from "react";
import Link from "next/link";
import type { EventSummaryResponse } from "@/lib/types";

const categoryColors: Record<string, string> = {
  surf: "bg-cat-surf",
  beach: "bg-cat-beach",
  board: "bg-cat-board",
  boat: "bg-cat-boat",
};

function getCategoryColor(category: string): string {
  const lower = category.toLowerCase();
  for (const [key, cls] of Object.entries(categoryColors)) {
    if (lower.includes(key)) return cls;
  }
  return "bg-tide-900";
}

export function EventExplorer({
  events,
  carnivalId,
  isLive = false,
}: {
  events: EventSummaryResponse[];
  carnivalId: string;
  isLive?: boolean;
}) {
  const [query, setQuery] = useState("");
  const [ageGroup, setAgeGroup] = useState("");
  const [gender, setGender] = useState("");
  const [category, setCategory] = useState("");

  const ageGroups = useMemo(() => [...new Set(events.map((e) => e.ageGroup))].sort(), [events]);
  const genders = useMemo(() => [...new Set(events.map((e) => e.gender))].sort(), [events]);
  const categories = useMemo(() => [...new Set(events.map((e) => e.category))].sort(), [events]);

  const filtered = useMemo(() => {
    return events.filter((e) => {
      if (query) {
        const q = query.toLowerCase();
        if (
          !e.name.toLowerCase().includes(q) &&
          !e.ageGroup.toLowerCase().includes(q) &&
          !e.gender.toLowerCase().includes(q) &&
          !e.category.toLowerCase().includes(q)
        )
          return false;
      }
      if (ageGroup && e.ageGroup !== ageGroup) return false;
      if (gender && e.gender !== gender) return false;
      if (category && e.category !== category) return false;
      return true;
    });
  }, [events, query, ageGroup, gender, category]);

  const hasAnyFilter = query || ageGroup || gender || category;

  return (
    <div className="space-y-4">
      {/* Header: live badge + event count */}
      <div className="flex items-center gap-3">
        {isLive && (
          <span className="inline-flex items-center gap-1.5 rounded-full bg-live/10 px-2.5 py-1">
            <span className="relative flex h-2 w-2">
              <span className="absolute inline-flex h-full w-full rounded-full bg-live opacity-60 live-dot" />
              <span className="relative inline-flex h-2 w-2 rounded-full bg-live" />
            </span>
            <span className="text-[11px] font-heading font-semibold uppercase tracking-wider text-live">
              Live
            </span>
          </span>
        )}
        <span className="text-sm font-heading font-semibold text-ink-700">
          {events.length} Events
        </span>
      </div>

      {/* Filter bar — all controls same height */}
      <div className="flex flex-col sm:flex-row gap-2">
        <div className="relative flex-1">
          <svg
            className="absolute left-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-ink-400"
            fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}
          >
            <path strokeLinecap="round" strokeLinejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" />
          </svg>
          <input
            type="text"
            placeholder="Search events..."
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            className="w-full h-11 bg-white rounded-[--radius-md] border border-ink-200/60 pl-10 pr-4 text-sm text-ink-900 placeholder:text-ink-400 shadow-card focus:outline-none focus:ring-2 focus:ring-tide-500/20 focus:border-tide-500 transition-all"
          />
        </div>
        <div className="flex gap-2">
          {ageGroups.length > 1 && (
            <FilterSelect label="Age" value={ageGroup} options={ageGroups} onChange={setAgeGroup} />
          )}
          {genders.length > 1 && (
            <FilterSelect label="Gender" value={gender} options={genders} onChange={setGender} />
          )}
          {categories.length > 1 && (
            <FilterSelect label="Discipline" value={category} options={categories} onChange={setCategory} />
          )}
        </div>
      </div>

      {/* Count + clear */}
      {hasAnyFilter && (
        <div className="flex items-center justify-between">
          <p className="text-xs font-heading text-ink-400">
            {filtered.length} of {events.length} events
          </p>
          <button
            onClick={() => { setQuery(""); setAgeGroup(""); setGender(""); setCategory(""); }}
            className="text-xs font-heading font-medium text-tide-600 hover:text-tide-700 transition-colors cursor-pointer"
          >
            Clear filters
          </button>
        </div>
      )}

      {/* Event list */}
      {filtered.length === 0 ? (
        <div className="text-center py-10">
          <p className="text-sm text-ink-400 font-heading">No events match your filters</p>
        </div>
      ) : (
        <div className="space-y-2">
          {filtered.map((event, i) => (
            <Link
              key={event.id}
              href={`/carnival/${carnivalId}/results?eventId=${event.id}`}
              className="group flex items-center justify-between bg-white rounded-[--radius-md] border border-ink-200 px-4 py-3.5 shadow-card hover:shadow-float hover:-translate-y-px transition-all duration-standard press-card animate-fade-up"
              style={{ animationDelay: `${i * 30}ms` }}
            >
              <div className="flex items-center gap-3">
                <div className={`h-9 w-9 rounded-[--radius-sm] ${getCategoryColor(event.category)} flex items-center justify-center shrink-0`}>
                  <span className="text-[10px] font-heading font-bold text-white uppercase">
                    {event.category.slice(0, 3)}
                  </span>
                </div>
                <div>
                  <p className="font-heading font-semibold text-ink-900 group-hover:text-tide-700 transition-colors">
                    {event.name}
                  </p>
                  <div className="flex items-center gap-1.5 mt-0.5">
                    <span className="text-xs text-ink-500 font-heading">{event.ageGroup}</span>
                    <span className="text-ink-300">&middot;</span>
                    <span className="text-xs text-ink-500 font-heading">{event.gender}</span>
                    <span className="text-ink-300">&middot;</span>
                    <span className="text-xs text-ink-500 font-heading">{event.category}</span>
                  </div>
                </div>
              </div>
              <div className="flex items-center gap-2 shrink-0">
                {event.hasResults && (
                  <span className="text-[11px] font-heading font-bold text-signal-green bg-signal-green/12 px-2.5 py-0.5 rounded-full">
                    Results
                  </span>
                )}
                {event.roundCount > 0 && !event.hasResults && (
                  <span className="text-[11px] font-heading font-bold text-tide-700 bg-tide-100 px-2.5 py-0.5 rounded-full">
                    Drawn
                  </span>
                )}
                {event.roundCount === 0 && !event.hasResults && (
                  <span className="text-[11px] font-heading font-bold text-ink-500 bg-ink-100 px-2.5 py-0.5 rounded-full">
                    Pending
                  </span>
                )}
                <svg className="h-4 w-4 text-ink-300 group-hover:text-tide-500 transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="m8.25 4.5 7.5 7.5-7.5 7.5" />
                </svg>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}

function FilterSelect({
  label,
  value,
  options,
  onChange,
}: {
  label: string;
  value: string;
  options: string[];
  onChange: (value: string) => void;
}) {
  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      aria-label={`Filter by ${label.toLowerCase()}`}
      className={`h-11 rounded-[--radius-md] border border-ink-200/60 bg-white px-3 text-sm shadow-card cursor-pointer hover:border-ink-400 focus:outline-none focus:ring-2 focus:ring-tide-500/20 focus:border-tide-500 transition-all ${
        value ? "text-tide-700 font-medium" : "text-ink-500"
      }`}
    >
      <option value="">{label}</option>
      {options.map((opt) => (
        <option key={opt} value={opt}>{opt}</option>
      ))}
    </select>
  );
}
