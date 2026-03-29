"use client";

import { useState, useMemo } from "react";
import { useSignalR } from "@/lib/signalr";
import { PlacingBadge } from "./PlacingBadge";
import { LiveBadge } from "./LiveBadge";
import type { CarnivalResultsResponse, ResultResponse } from "@/lib/types";

export function LiveResults({
  carnivalId,
  initialData,
}: {
  carnivalId: string;
  initialData: CarnivalResultsResponse;
}) {
  const [data, setData] = useState(initialData);
  const [search, setSearch] = useState("");
  const [ageGroup, setAgeGroup] = useState("");
  const [gender, setGender] = useState("");

  const connectionStatus = useSignalR(carnivalId, {
    ResultRecorded: (result: ResultResponse) => {
      setData((prev) => patchResult(prev, result));
    },
    ResultCorrected: (result: ResultResponse) => {
      setData((prev) => patchResult(prev, result));
    },
  });

  const ageGroups = useMemo(
    () => [...new Set(data.events.map((e) => e.ageGroup))].sort(),
    [data.events]
  );
  const genders = useMemo(
    () => [...new Set(data.events.map((e) => e.gender))].sort(),
    [data.events]
  );

  const filteredEvents = useMemo(() => {
    let events = data.events;

    if (ageGroup) events = events.filter((e) => e.ageGroup === ageGroup);
    if (gender) events = events.filter((e) => e.gender === gender);

    if (search) {
      const q = search.toLowerCase();
      events = events
        .map((event) => ({
          ...event,
          heats: event.heats
            .map((heat) => ({
              ...heat,
              results: heat.results.filter(
                (r) =>
                  r.members.some(
                    (m) =>
                      m.firstName.toLowerCase().includes(q) ||
                      m.lastName.toLowerCase().includes(q)
                  ) ||
                  r.clubName.toLowerCase().includes(q) ||
                  event.eventName.toLowerCase().includes(q)
              ),
            }))
            .filter((h) => h.results.length > 0),
        }))
        .filter((e) => e.heats.length > 0);
    }

    return events;
  }, [data.events, search, ageGroup, gender]);

  const hasAnyFilter = search || ageGroup || gender;

  if (data.events.length === 0 || data.events.every((e) => e.heats.length === 0)) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center">
        <div className="h-12 w-12 rounded-full bg-tide-50 flex items-center justify-center mb-4">
          <svg className="h-6 w-6 text-tide-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
          </svg>
        </div>
        <p className="text-ink-700 font-heading font-medium">No results yet</p>
        <p className="text-sm text-ink-500 mt-1">Results will appear here live as heats are completed.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Filter bar: search left, dropdowns + live badge right */}
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
            placeholder="Search competitors, clubs..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full bg-white rounded-[--radius-md] border border-ink-200/60 pl-10 pr-4 py-2.5 min-h-[44px] text-sm text-ink-900 placeholder:text-ink-400 shadow-card focus:outline-none focus:ring-2 focus:ring-tide-500/20 focus:border-tide-500 transition-all"
          />
        </div>
        <div className="flex items-center gap-2">
          {ageGroups.length > 1 && (
            <FilterSelect label="Age" value={ageGroup} options={ageGroups} onChange={setAgeGroup} />
          )}
          {genders.length > 1 && (
            <FilterSelect label="Gender" value={gender} options={genders} onChange={setGender} />
          )}
          <LiveBadge status={connectionStatus} />
        </div>
      </div>

      {/* Count + clear when filtering */}
      {hasAnyFilter && (
        <div className="flex items-center justify-between">
          <p className="text-xs font-heading text-ink-400">
            {filteredEvents.length} event{filteredEvents.length !== 1 ? "s" : ""}
          </p>
          <button
            onClick={() => { setSearch(""); setAgeGroup(""); setGender(""); }}
            className="text-xs font-heading font-medium text-tide-600 hover:text-tide-700 transition-colors cursor-pointer"
          >
            Clear filters
          </button>
        </div>
      )}

      {filteredEvents.length === 0 && hasAnyFilter && (
        <div className="text-center py-10">
          <p className="text-sm text-ink-400 font-heading">No results match your filters</p>
        </div>
      )}

      {filteredEvents.map((event, eventIndex) => (
        <section
          key={event.eventId}
          className="animate-fade-up"
          style={{ animationDelay: `${eventIndex * 60}ms` }}
        >
          {/* Event header */}
          <div className="flex items-baseline gap-3 mb-3">
            <h2 className="text-base font-heading font-bold uppercase tracking-wider text-ink-900">
              {event.eventName}
            </h2>
            <div className="flex items-center gap-1.5 text-xs text-ink-500 font-heading font-medium">
              <span>{event.ageGroup}</span>
              <span className="text-ink-300">/</span>
              <span>{event.gender}</span>
            </div>
          </div>

          {/* Heats */}
          <div className="space-y-3">
            {event.heats.map((heat) => (
              <div
                key={heat.heatId}
                className="bg-white rounded-[--radius-lg] border border-ink-200 shadow-card overflow-hidden"
              >
                {/* Heat header */}
                <div className="px-4 py-3 border-b border-ink-100 flex items-center justify-between bg-ink-50">
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-heading font-bold text-ink-900">
                      {heat.roundType}
                    </span>
                    <span className="text-sm text-ink-500 font-heading font-medium">
                      Heat {heat.heatNumber}
                    </span>
                  </div>
                  {heat.isComplete && (
                    <span className="inline-flex items-center gap-1 text-[11px] font-heading font-bold text-signal-green bg-signal-green/12 px-2.5 py-0.5 rounded-full">
                      <svg className="h-3 w-3" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M16.704 4.153a.75.75 0 0 1 .143 1.052l-8 10.5a.75.75 0 0 1-1.127.075l-4.5-4.5a.75.75 0 0 1 1.06-1.06l3.894 3.893 7.48-9.817a.75.75 0 0 1 1.05-.143Z" clipRule="evenodd" />
                      </svg>
                      Official
                    </span>
                  )}
                </div>

                {/* Results — stacked on mobile, table on md+ */}
                <div className="hidden md:block overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="text-[11px] font-heading font-bold uppercase tracking-wider text-ink-600 bg-ink-50/50">
                        <th scope="col" className="pl-4 pr-2 py-2 text-center w-14">Pos</th>
                        <th scope="col" className="py-2 text-left">Competitor</th>
                        <th scope="col" className="py-2 text-left">Club</th>
                        <th scope="col" className="pr-4 py-2 text-right w-24"></th>
                      </tr>
                    </thead>
                    <tbody>
                      {heat.results
                        .sort((a, b) => (a.placing ?? 99) - (b.placing ?? 99))
                        .map((result) => (
                          <tr
                            key={result.id}
                            className="result-row border-t border-ink-100 even:bg-ink-50/30"
                          >
                            <td className="pl-4 pr-2 py-3 text-center">
                              <PlacingBadge placing={result.placing} />
                            </td>
                            <td className="py-3">
                              <span className="font-heading font-semibold text-ink-900">
                                {result.members
                                  .map((m) => `${m.firstName} ${m.lastName}`)
                                  .join(", ")}
                              </span>
                            </td>
                            <td className="py-3 text-ink-500 font-heading">
                              {result.clubName}
                            </td>
                            <td className="pr-4 py-3 text-right">
                              {result.status === "Provisional" && (
                                <span className="text-[11px] font-heading font-medium text-signal-amber bg-signal-amber/10 px-2 py-0.5 rounded-full">
                                  Provisional
                                </span>
                              )}
                            </td>
                          </tr>
                        ))}
                    </tbody>
                  </table>
                </div>

                {/* Mobile stacked layout */}
                <div className="md:hidden divide-y divide-ink-100/80">
                  {heat.results
                    .sort((a, b) => (a.placing ?? 99) - (b.placing ?? 99))
                    .map((result) => (
                      <div key={result.id} className="flex items-center gap-3 px-4 py-3">
                        <PlacingBadge placing={result.placing} />
                        <div className="flex-1 min-w-0">
                          <p className="font-heading font-semibold text-ink-900 text-sm truncate">
                            {result.members
                              .map((m) => `${m.firstName} ${m.lastName}`)
                              .join(", ")}
                          </p>
                          <div className="flex items-center gap-2 mt-0.5">
                            <span className="text-xs text-ink-500 font-heading">{result.clubName}</span>
                            {result.status === "Provisional" && (
                              <span className="text-[10px] font-heading font-medium text-signal-amber bg-signal-amber/10 px-1.5 py-0.5 rounded-full">
                                Provisional
                              </span>
                            )}
                          </div>
                        </div>
                      </div>
                    ))}
                </div>
              </div>
            ))}
          </div>
        </section>
      ))}
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
      className={`rounded-[--radius-md] border border-ink-200/60 bg-white px-3 py-2.5 min-h-[44px] text-sm cursor-pointer hover:border-ink-400 focus:outline-none focus:ring-2 focus:ring-tide-500/20 focus:border-tide-500 transition-all ${
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

function patchResult(
  data: CarnivalResultsResponse,
  result: ResultResponse
): CarnivalResultsResponse {
  return {
    ...data,
    events: data.events.map((event) => ({
      ...event,
      heats: event.heats.map((heat) => {
        if (heat.heatId !== result.heatId) return heat;
        const existing = heat.results.findIndex((r) => r.id === result.id);
        const newResults =
          existing >= 0
            ? heat.results.map((r) => (r.id === result.id ? result : r))
            : [...heat.results, result];
        return { ...heat, results: newResults };
      }),
    })),
  };
}
