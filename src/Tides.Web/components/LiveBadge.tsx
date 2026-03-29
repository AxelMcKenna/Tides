export function LiveBadge({ className = "" }: { className?: string }) {
  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full bg-live/10 px-2.5 py-1 ${className}`}>
      <span className="relative flex h-2 w-2">
        <span className="absolute inline-flex h-full w-full rounded-full bg-live opacity-60 live-dot" />
        <span className="relative inline-flex h-2 w-2 rounded-full bg-live" />
      </span>
      <span className="text-[11px] font-heading font-semibold uppercase tracking-wider text-live">
        Live
      </span>
    </span>
  );
}
