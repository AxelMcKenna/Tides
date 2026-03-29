"use client";

import { useEffect, useState, useCallback, useRef } from "react";
import { useParams } from "next/navigation";
import { useSignalR } from "@/lib/signalr";
import { formatDateRange } from "@/lib/dates";
import type {
  LeaderboardResponse,
  CarnivalResultsResponse,
  CarnivalResponse,
} from "@/lib/types";

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5266";
const SLIDE_INTERVAL = 8000;
const DEBOUNCE_MS = 2000;

const displayRankColors: Record<number, string> = {
  1: "text-medal-gold",
  2: "text-medal-silver",
  3: "text-medal-bronze",
};

export default function DisplayPage() {
  const { id } = useParams<{ id: string }>();
  const [carnival, setCarnival] = useState<CarnivalResponse | null>(null);
  const [leaderboard, setLeaderboard] = useState<LeaderboardResponse | null>(null);
  const [results, setResults] = useState<CarnivalResultsResponse | null>(null);
  const [lastUpdatedEventId, setLastUpdatedEventId] = useState<string | null>(null);
  const [currentSlide, setCurrentSlide] = useState<"leaderboard" | "results">("leaderboard");
  const [flash, setFlash] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const fetchAll = useCallback(async () => {
    try {
      const [c, l, r] = await Promise.all([
        fetch(`${API_BASE}/api/carnivals/${id}`).then((r) => r.json()),
        fetch(`${API_BASE}/api/carnivals/${id}/leaderboard`).then((r) => r.json()),
        fetch(`${API_BASE}/api/carnivals/${id}/results`).then((r) => r.json()),
      ]);
      setCarnival(c);
      setLeaderboard(l);
      setResults(r);
    } catch {}
  }, [id]);

  const debouncedFetchAll = useCallback(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(fetchAll, DEBOUNCE_MS);
  }, [fetchAll]);

  useEffect(() => {
    fetchAll();
    return () => { if (debounceRef.current) clearTimeout(debounceRef.current); };
  }, [fetchAll]);

  useEffect(() => {
    const timer = setInterval(() => {
      setCurrentSlide((prev) => (prev === "leaderboard" ? "results" : "leaderboard"));
    }, SLIDE_INTERVAL);
    return () => clearInterval(timer);
  }, []);

  useSignalR(id, {
    ResultRecorded: (result) => {
      setFlash(true);
      setCurrentSlide("results");
      if (result?.heatId) {
        setResults((prev) => {
          if (!prev) return prev;
          const event = prev.events.find((e) => e.heats.some((h) => h.heatId === result.heatId));
          if (event) setLastUpdatedEventId(event.eventId);
          return prev;
        });
      }
      debouncedFetchAll();
      setTimeout(() => setFlash(false), 1500);
    },
    ResultCorrected: () => debouncedFetchAll(),
  });

  if (!carnival || !leaderboard || !results) {
    return (
      <div className="h-screen w-screen bg-display-bg flex flex-col items-center justify-center gap-6">
        <div className="text-center">
          <h1 className="text-5xl font-heading font-bold text-white uppercase tracking-[0.3em]">Shore</h1>
          <p className="text-tide-400/60 text-sm font-heading mt-2 uppercase tracking-[0.2em]">Live Results</p>
        </div>
        <div className="flex items-center gap-3">
          <span className="relative flex h-2.5 w-2.5">
            <span className="absolute inline-flex h-full w-full rounded-full bg-tide-400 opacity-60 live-dot" />
            <span className="relative inline-flex h-2.5 w-2.5 rounded-full bg-tide-400" />
          </span>
          <p className="text-tide-400/80 text-sm font-heading">Connecting...</p>
        </div>
      </div>
    );
  }

  return (
    <div className={`h-screen w-screen bg-display-bg text-white flex flex-col overflow-hidden transition-colors duration-slow ${flash ? "bg-tide-950" : ""}`}>
      {/* Header bar */}
      <div className="shrink-0 px-10 py-5 flex items-center justify-between border-b border-white/5">
        <div>
          <h1 className="text-3xl font-heading font-bold text-white tracking-tight">
            {carnival.name}
          </h1>
          <p className="text-sm font-heading text-white/30 mt-1">
            {formatDateRange(carnival.startDate, carnival.endDate)}
            <span className="mx-2 text-white/15">|</span>
            <span className="uppercase tracking-wider text-xs">{carnival.sanction}</span>
          </p>
        </div>
        <div className="flex items-center gap-3">
          <span className="relative flex h-2.5 w-2.5">
            <span className="absolute inline-flex h-full w-full rounded-full bg-live opacity-60 live-dot" />
            <span className="relative inline-flex h-2.5 w-2.5 rounded-full bg-live" />
          </span>
          <span className="text-xs font-heading font-semibold uppercase tracking-[0.15em] text-live">
            Live
          </span>
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-hidden px-10 py-8" key={currentSlide}>
        <div className="animate-crossfade h-full">
          {currentSlide === "leaderboard" ? (
            <DisplayLeaderboard data={leaderboard} />
          ) : (
            <DisplayResults data={results} lastUpdatedEventId={lastUpdatedEventId} />
          )}
        </div>
      </div>

      {/* Bottom bar */}
      <div className="shrink-0 px-10 py-4 flex items-center justify-between border-t border-white/5">
        <span className="text-[11px] font-heading font-semibold uppercase tracking-[0.3em] text-white/20">
          Shore
        </span>
        <div className="flex gap-2">
          {(["leaderboard", "results"] as const).map((slide) => (
            <button
              key={slide}
              aria-label={`Show ${slide}`}
              onClick={() => setCurrentSlide(slide)}
              className={`h-1.5 rounded-full transition-all duration-standard ${
                currentSlide === slide ? "w-8 bg-tide-400" : "w-3 bg-white/15 hover:bg-white/25"
              }`}
            />
          ))}
        </div>
        <DisplayClock />
      </div>
    </div>
  );
}

function DisplayClock() {
  const [time, setTime] = useState(() =>
    new Date().toLocaleTimeString("en-NZ", { hour: "2-digit", minute: "2-digit" })
  );

  useEffect(() => {
    const timer = setInterval(() => {
      setTime(new Date().toLocaleTimeString("en-NZ", { hour: "2-digit", minute: "2-digit" }));
    }, 60_000);
    return () => clearInterval(timer);
  }, []);

  return (
    <span className="text-[11px] font-data text-white/20 tabular-nums">
      {time}
    </span>
  );
}

function DisplayLeaderboard({ data }: { data: LeaderboardResponse }) {
  if (data.standings.length === 0) {
    return (
      <div className="h-full flex items-center justify-center">
        <p className="text-2xl font-heading font-bold text-white/20">Awaiting results...</p>
      </div>
    );
  }

  return (
    <div className="h-full flex flex-col">
      <h2 className="text-xs font-heading font-semibold uppercase tracking-[0.2em] text-white/30 mb-6">
        Club Standings
      </h2>
      <div className="flex-1 space-y-2 overflow-hidden">
        {data.standings.slice(0, 10).map((s, i) => (
          <div
            key={s.clubId}
            className="flex items-center justify-between rounded-[--radius-md] bg-display-card border border-white/5 px-7 py-4 animate-fade-up"
            style={{ animationDelay: `${i * 50}ms` }}
          >
            <div className="flex items-center gap-5">
              <span className={`text-4xl font-heading font-bold w-12 text-center tabular-nums ${displayRankColors[s.rank] ?? "text-white/20"}`}>
                {s.rank}
              </span>
              <div>
                <p className="text-xl font-heading font-semibold text-white">{s.clubName}</p>
                <p className="text-xs font-heading text-white/30 uppercase tracking-wider">{s.clubAbbreviation}</p>
              </div>
            </div>
            <span className="text-4xl font-data font-bold tabular-nums text-white">
              {s.totalPoints}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}

function DisplayResults({
  data,
  lastUpdatedEventId,
}: {
  data: CarnivalResultsResponse;
  lastUpdatedEventId: string | null;
}) {
  const eventsWithResults = data.events.filter((e) =>
    e.heats.some((h) => h.results.length > 0)
  );

  if (eventsWithResults.length === 0) {
    return (
      <div className="h-full flex items-center justify-center">
        <p className="text-2xl font-heading font-bold text-white/20">Awaiting results...</p>
      </div>
    );
  }

  const latest =
    (lastUpdatedEventId && eventsWithResults.find((e) => e.eventId === lastUpdatedEventId)) ||
    eventsWithResults[eventsWithResults.length - 1];

  return (
    <div className="h-full flex flex-col">
      <div className="mb-6">
        <h2 className="text-3xl font-heading font-bold text-white tracking-tight">{latest.eventName}</h2>
        <p className="text-sm font-heading text-white/30 mt-1">
          {latest.ageGroup}
          <span className="mx-2 text-white/15">/</span>
          {latest.gender}
        </p>
      </div>

      <div className="flex-1 space-y-6 overflow-hidden">
        {latest.heats.map((heat) => (
          <div key={heat.heatId}>
            <h3 className="text-xs font-heading font-medium uppercase tracking-wider text-white/25 mb-3">
              {heat.roundType} — Heat {heat.heatNumber}
            </h3>
            <div className="space-y-2">
              {heat.results
                .sort((a, b) => (a.placing ?? 99) - (b.placing ?? 99))
                .map((result, i) => (
                  <div
                    key={result.id}
                    className="flex items-center justify-between rounded-[--radius-md] bg-display-card border border-white/5 px-7 py-4 animate-fade-up"
                    style={{ animationDelay: `${i * 60}ms` }}
                  >
                    <div className="flex items-center gap-5">
                      <span className={`text-4xl font-heading font-bold w-10 text-center tabular-nums ${displayRankColors[result.placing ?? 0] ?? "text-white/20"}`}>
                        {result.placing ?? "—"}
                      </span>
                      <div>
                        <p className="text-xl font-heading font-semibold text-white">
                          {result.members.map((m) => `${m.firstName} ${m.lastName}`).join(", ")}
                        </p>
                        <p className="text-xs font-heading text-white/30 uppercase tracking-wider">
                          {result.clubName}
                        </p>
                      </div>
                    </div>
                  </div>
                ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
