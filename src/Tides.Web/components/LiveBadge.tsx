import type { ConnectionStatus } from "@/lib/signalr";

const statusConfig: Record<ConnectionStatus, { label: string; dotClass: string; textClass: string }> = {
  connecting: { label: "Connecting", dotClass: "bg-signal-amber", textClass: "text-signal-amber" },
  connected: { label: "Live", dotClass: "bg-live", textClass: "text-live" },
  reconnecting: { label: "Reconnecting", dotClass: "bg-signal-amber", textClass: "text-signal-amber" },
  disconnected: { label: "Offline", dotClass: "bg-ink-400", textClass: "text-ink-500" },
};

export function LiveBadge({
  status = "connected",
  className = "",
}: {
  status?: ConnectionStatus;
  className?: string;
}) {
  const config = statusConfig[status];

  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full bg-live/10 px-2.5 py-1 ${className}`}>
      <span className="relative flex h-2 w-2">
        {status === "connected" && (
          <span className={`absolute inline-flex h-full w-full rounded-full ${config.dotClass} opacity-60 live-dot`} />
        )}
        <span className={`relative inline-flex h-2 w-2 rounded-full ${config.dotClass}`} />
      </span>
      <span className={`text-[11px] font-heading font-semibold uppercase tracking-wider ${config.textClass}`}>
        {config.label}
      </span>
    </span>
  );
}
