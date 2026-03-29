"use client";

import { useState, useEffect } from "react";
import type { ConnectionStatus } from "@/lib/signalr";

const statusConfig: Record<ConnectionStatus, { label: string; dotClass: string; textClass: string; bgClass: string }> = {
  connecting: { label: "Connecting", dotClass: "bg-signal-amber", textClass: "text-signal-amber", bgClass: "bg-signal-amber/10" },
  connected: { label: "Live", dotClass: "bg-live", textClass: "text-live", bgClass: "bg-live/10" },
  reconnecting: { label: "Reconnecting", dotClass: "bg-signal-amber", textClass: "text-signal-amber", bgClass: "bg-signal-amber/10" },
  disconnected: { label: "Offline", dotClass: "bg-ink-400", textClass: "text-ink-500", bgClass: "bg-ink-100" },
};

function useRelativeTime(timestamp: number | null): string | null {
  const [now, setNow] = useState(Date.now());

  useEffect(() => {
    if (!timestamp) return;
    const timer = setInterval(() => setNow(Date.now()), 10_000);
    return () => clearInterval(timer);
  }, [timestamp]);

  if (!timestamp) return null;

  const seconds = Math.floor((now - timestamp) / 1000);
  if (seconds < 10) return "just now";
  if (seconds < 60) return `${seconds}s ago`;
  const minutes = Math.floor(seconds / 60);
  return `${minutes}m ago`;
}

export function LiveBadge({
  status = "connected",
  lastEvent = null,
  className = "",
}: {
  status?: ConnectionStatus;
  lastEvent?: number | null;
  className?: string;
}) {
  const config = statusConfig[status];
  const freshness = useRelativeTime(lastEvent);

  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full ${config.bgClass} px-2.5 py-1 ${className}`}>
      <span className="relative flex h-2 w-2">
        {status === "connected" && (
          <span className={`absolute inline-flex h-full w-full rounded-full ${config.dotClass} live-dot`} />
        )}
        <span className={`relative inline-flex h-2 w-2 rounded-full ${config.dotClass}`} />
      </span>
      <span className={`text-[11px] font-heading font-semibold uppercase tracking-wider ${config.textClass}`}>
        {config.label}
      </span>
      {freshness && status === "connected" && (
        <span className="text-[10px] font-heading text-ink-400 tabular-nums">
          · {freshness}
        </span>
      )}
    </span>
  );
}
