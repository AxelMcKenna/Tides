"use client";

import { useState } from "react";
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

  useSignalR(carnivalId, {
    ResultRecorded: (result: ResultResponse) => {
      setData((prev) => patchResult(prev, result));
    },
    ResultCorrected: (result: ResultResponse) => {
      setData((prev) => patchResult(prev, result));
    },
  });

  if (data.events.length === 0 || data.events.every((e) => e.heats.length === 0)) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center">
        <div className="h-12 w-12 rounded-full bg-ink-100 flex items-center justify-center mb-4">
          <svg className="h-6 w-6 text-ink-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
          </svg>
        </div>
        <p className="text-ink-500 font-heading font-medium">No results yet</p>
        <p className="text-sm text-ink-400 mt-1">Results will appear here as they are entered.</p>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <div className="flex justify-end">
        <LiveBadge />
      </div>

      {data.events.map((event, eventIndex) => (
        <section
          key={event.eventId}
          className="animate-fade-up"
          style={{ animationDelay: `${eventIndex * 60}ms` }}
        >
          {/* Event header */}
          <div className="flex items-baseline gap-3 mb-3">
            <h2 className="text-lg font-heading font-semibold text-ink-900">
              {event.eventName}
            </h2>
            <div className="flex items-center gap-1.5 text-xs text-ink-400 font-heading">
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
                className="bg-white rounded-[--radius-lg] border border-ink-200/60 shadow-card overflow-hidden"
              >
                {/* Heat header */}
                <div className="px-4 py-2.5 border-b border-ink-100 flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-heading font-semibold text-ink-700">
                      {heat.roundType}
                    </span>
                    <span className="text-sm text-ink-400">
                      Heat {heat.heatNumber}
                    </span>
                  </div>
                  {heat.isComplete && (
                    <span className="inline-flex items-center gap-1 text-[11px] font-heading font-medium text-signal-green bg-signal-green/8 px-2 py-0.5 rounded-full">
                      <svg className="h-3 w-3" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M16.704 4.153a.75.75 0 0 1 .143 1.052l-8 10.5a.75.75 0 0 1-1.127.075l-4.5-4.5a.75.75 0 0 1 1.06-1.06l3.894 3.893 7.48-9.817a.75.75 0 0 1 1.05-.143Z" clipRule="evenodd" />
                      </svg>
                      Official
                    </span>
                  )}
                </div>

                {/* Results table */}
                <div className="overflow-x-auto">
                  <table className="w-full text-sm min-w-[480px]">
                    <thead>
                      <tr className="text-[11px] font-heading font-medium uppercase tracking-wider text-ink-400 bg-ink-50/50">
                        <th scope="col" className="pl-4 pr-2 py-2 text-center w-14">Pos</th>
                        <th scope="col" className="py-2 text-left">Competitor</th>
                        <th scope="col" className="py-2 text-left">Club</th>
                        <th scope="col" className="px-4 py-2 text-right">Time</th>
                        <th scope="col" className="pr-4 py-2 text-right w-24"></th>
                      </tr>
                    </thead>
                    <tbody>
                      {heat.results
                        .sort((a, b) => (a.placing ?? 99) - (b.placing ?? 99))
                        .map((result, i) => (
                          <tr
                            key={result.id}
                            className="result-row border-t border-ink-100/80"
                          >
                            <td className="pl-4 pr-2 py-3 text-center">
                              <PlacingBadge placing={result.placing} />
                            </td>
                            <td className="py-3">
                              <span className="font-heading font-medium text-ink-900">
                                {result.members
                                  .map((m) => `${m.firstName} ${m.lastName}`)
                                  .join(", ")}
                              </span>
                            </td>
                            <td className="py-3 text-ink-500 font-heading">
                              {result.clubName}
                            </td>
                            <td className="px-4 py-3 text-right">
                              {result.time ? (
                                <span className="font-data tabular-nums text-ink-700">
                                  {result.time}
                                </span>
                              ) : (
                                <span className="text-ink-300">—</span>
                              )}
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
              </div>
            ))}
          </div>
        </section>
      ))}
    </div>
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
