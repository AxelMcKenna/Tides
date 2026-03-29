export function formatDateRange(start: string, end: string): string {
  const s = new Date(start + "T00:00:00");
  const e = new Date(end + "T00:00:00");
  const fmt = (d: Date) =>
    d.toLocaleDateString("en-NZ", { day: "numeric", month: "short", year: "numeric" });
  if (start === end) return fmt(s);
  return `${fmt(s)} — ${fmt(e)}`;
}
