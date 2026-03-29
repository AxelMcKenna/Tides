"use client";

import { useState, useCallback } from "react";
import { useSignalR } from "@/lib/signalr";
import { LiveBadge } from "./LiveBadge";
import type { LeaderboardResponse } from "@/lib/types";

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5266";

const rankColors: Record<number, string> = {
  1: "text-amber-600",
  2: "text-ink-500",
  3: "text-amber-700",
};

export function LiveLeaderboard({
  carnivalId,
  initialData,
}: {
  carnivalId: string;
  initialData: LeaderboardResponse;
}) {
  const [data, setData] = useState(initialData);

  const refreshLeaderboard = useCallback(async () => {
    try {
      const res = await fetch(
        `${API_BASE}/api/carnivals/${carnivalId}/leaderboard`
      );
      if (res.ok) setData(await res.json());
    } catch {
      // Will retry on next event
    }
  }, [carnivalId]);

  useSignalR(carnivalId, {
    ResultRecorded: refreshLeaderboard,
    ResultCorrected: refreshLeaderboard,
  });

  if (data.standings.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center">
        <div className="h-12 w-12 rounded-full bg-ink-100 flex items-center justify-center mb-4">
          <svg className="h-6 w-6 text-ink-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M16.5 18.75h-9m9 0a3 3 0 0 1 3 3h-15a3 3 0 0 1 3-3m9 0v-4.5A3.375 3.375 0 0 0 13.125 11h-2.25A3.375 3.375 0 0 0 7.5 14.25v4.5" />
          </svg>
        </div>
        <p className="text-ink-500 font-heading font-medium">No standings yet</p>
        <p className="text-sm text-ink-400 mt-1">The leaderboard will update as results are entered.</p>
      </div>
    );
  }

  return (
    <div>
      <div className="flex justify-end mb-4">
        <LiveBadge />
      </div>

      <div className="bg-white rounded-[--radius-lg] border border-ink-200/60 shadow-card overflow-hidden">
        <table className="w-full">
          <caption className="sr-only">Club standings leaderboard</caption>
          <thead>
            <tr className="text-[11px] font-heading font-medium uppercase tracking-wider text-ink-400 bg-ink-50/50">
              <th scope="col" className="pl-5 pr-2 py-3 text-center w-16">Rank</th>
              <th scope="col" className="py-3 text-left">Club</th>
              <th scope="col" className="px-5 py-3 text-right">Points</th>
            </tr>
          </thead>
          <tbody>
            {data.standings.map((s, i) => (
              <tr
                key={s.clubId}
                className="result-row border-t border-ink-100/80 animate-fade-up"
                style={{ animationDelay: `${i * 40}ms` }}
              >
                <td className={`pl-5 pr-2 py-4 text-center font-heading font-bold text-lg tabular-nums ${rankColors[s.rank] ?? "text-ink-300"}`}>
                  {s.rank}
                </td>
                <td className="py-4">
                  <div className="flex items-baseline gap-2">
                    <span className={`font-heading text-ink-900 ${i < 3 ? "font-semibold text-base" : "font-medium text-sm"}`}>
                      {s.clubName}
                    </span>
                    <span className="text-xs font-heading text-ink-400 uppercase tracking-wide">
                      {s.clubAbbreviation}
                    </span>
                  </div>
                </td>
                <td className="px-5 py-4 text-right">
                  <span className={`font-data tabular-nums ${i < 3 ? "text-xl font-bold text-ink-900" : "text-lg font-semibold text-ink-600"}`}>
                    {s.totalPoints}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
