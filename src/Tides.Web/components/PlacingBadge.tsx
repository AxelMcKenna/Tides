const config: Record<number, { bg: string; text: string; ring: string }> = {
  1: { bg: "bg-medal-gold-bg", text: "text-amber-700", ring: "ring-medal-gold/50" },
  2: { bg: "bg-medal-silver-bg", text: "text-ink-700", ring: "ring-medal-silver/50" },
  3: { bg: "bg-medal-bronze-bg", text: "text-amber-800", ring: "ring-medal-bronze/50" },
};

export function PlacingBadge({ placing, size = "md" }: { placing: number | null; size?: "sm" | "md" | "lg" }) {
  if (placing == null) return <span className="text-ink-300 font-data text-sm">—</span>;

  const style = config[placing];
  const dims = size === "sm" ? "h-6 w-6 text-xs" : size === "lg" ? "h-10 w-10 text-lg" : "h-8 w-8 text-sm";

  if (style) {
    return (
      <span
        className={`inline-flex items-center justify-center rounded-full font-heading font-bold tabular-nums ring-2 ${dims} ${style.bg} ${style.text} ${style.ring}`}
      >
        {placing}
      </span>
    );
  }

  return (
    <span className={`inline-flex items-center justify-center rounded-full font-heading font-semibold tabular-nums text-ink-500 bg-ink-100 ${dims}`}>
      {placing}
    </span>
  );
}
