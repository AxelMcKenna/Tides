import { api } from "@/lib/api";
import Link from "next/link";

export const revalidate = 30;

function formatDateRange(start: string, end: string): string {
  const s = new Date(start + "T00:00:00");
  const e = new Date(end + "T00:00:00");
  const fmt = (d: Date) =>
    d.toLocaleDateString("en-NZ", { day: "numeric", month: "short", year: "numeric" });
  if (start === end) return fmt(s);
  return `${fmt(s)} — ${fmt(e)}`;
}

export default async function Home() {
  const carnivals = await api.listCarnivals();

  if (carnivals.length === 0) {
    return (
      <main className="flex min-h-screen flex-col items-center justify-center bg-ink-50">
        <h1 className="text-5xl font-heading font-bold text-ink-900 tracking-tight">Shore</h1>
        <p className="mt-3 text-ink-400 font-heading">No carnivals yet</p>
      </main>
    );
  }

  const today = new Date().toISOString().split("T")[0];
  const live = carnivals.filter((c) => c.startDate <= today && c.endDate >= today);
  const upcoming = carnivals.filter((c) => c.startDate > today);
  const past = carnivals.filter((c) => c.endDate < today);

  return (
    <main id="main-content" className="min-h-screen bg-ink-50">
      {/* Hero header */}
      <div className="bg-tide-900">
        <div className="max-w-7xl mx-auto px-5 py-10">
          <h1 className="text-4xl font-heading font-bold text-white tracking-tight">Shore</h1>
          <p className="text-tide-300/60 font-heading text-sm mt-2">
            Surf Life Saving competition results
          </p>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-5 py-8 space-y-10">
        {live.length > 0 && <CarnivalSection title="Live Now" carnivals={live} isLive />}
        {upcoming.length > 0 && <CarnivalSection title="Upcoming" carnivals={upcoming} />}
        {past.length > 0 && <CarnivalSection title="Completed" carnivals={past} />}
      </div>
    </main>
  );
}

function CarnivalSection({
  title,
  carnivals,
  isLive,
}: {
  title: string;
  carnivals: { id: string; name: string; sanction: string; startDate: string; endDate: string; eventCount: number; hasResults: boolean }[];
  isLive?: boolean;
}) {
  return (
    <section>
      <h2 className="text-xs font-heading font-semibold uppercase tracking-[0.15em] text-ink-400 mb-4">
        {title}
      </h2>
      <div className="space-y-3">
        {carnivals.map((c, i) => (
          <Link
            key={c.id}
            href={`/carnival/${c.id}`}
            className="group flex items-center justify-between bg-white rounded-[--radius-lg] border border-ink-200/60 px-5 py-4 shadow-card hover:shadow-float hover:-translate-y-px transition-all duration-standard animate-fade-up"
            style={{ animationDelay: `${i * 50}ms` }}
          >
            <div className="flex items-center gap-4">
              {isLive && (
                <div className="shrink-0 h-10 w-10 rounded-full bg-live/10 flex items-center justify-center">
                  <span className="relative flex h-3 w-3">
                    <span className="absolute inline-flex h-full w-full rounded-full bg-live opacity-60 live-dot" />
                    <span className="relative inline-flex h-3 w-3 rounded-full bg-live" />
                  </span>
                </div>
              )}
              <div>
                <div className="flex items-center gap-2.5">
                  <p className="font-heading font-semibold text-ink-900 group-hover:text-tide-700 transition-colors">
                    {c.name}
                  </p>
                  {isLive && (
                    <span className="text-[10px] font-heading font-bold uppercase tracking-wider text-live bg-live/10 px-2 py-0.5 rounded-full">
                      Live
                    </span>
                  )}
                </div>
                <p className="text-sm text-ink-400 font-heading mt-0.5">
                  {formatDateRange(c.startDate, c.endDate)}
                  <span className="mx-2 text-ink-200">|</span>
                  <span className="text-xs uppercase tracking-wider">{c.sanction}</span>
                </p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <div className="text-right">
                <span className="text-xs font-heading text-ink-400">
                  {c.eventCount} event{c.eventCount !== 1 ? "s" : ""}
                </span>
                {c.hasResults && (
                  <span className="ml-2 text-[11px] font-heading font-medium text-signal-green bg-signal-green/8 px-2 py-0.5 rounded-full">
                    Results
                  </span>
                )}
              </div>
              <svg className="h-4 w-4 text-ink-300 group-hover:text-tide-500 transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="m8.25 4.5 7.5 7.5-7.5 7.5" />
              </svg>
            </div>
          </Link>
        ))}
      </div>
    </section>
  );
}
