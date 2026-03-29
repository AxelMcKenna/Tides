import { api } from "@/lib/api";
import Link from "next/link";
import { CarnivalQR } from "@/components/CarnivalQR";

const rankColors: Record<number, string> = {
  1: "text-amber-600",
  2: "text-ink-500",
  3: "text-amber-700",
};

export default async function CarnivalOverview({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const [carnival, leaderboard] = await Promise.all([
    api.getCarnival(id),
    api.getLeaderboard(id),
  ]);

  return (
    <div className="grid gap-6 lg:grid-cols-3">
      {/* Left column: Events */}
      <div className="lg:col-span-2 space-y-6">
        <section>
          <h2 className="text-sm font-heading font-semibold uppercase tracking-wider text-ink-400 mb-3">
            Events ({carnival.events.length})
          </h2>
          <div className="space-y-2">
            {carnival.events.map((event, i) => (
              <Link
                key={event.id}
                href={`/carnival/${id}/results?eventId=${event.id}`}
                className="group flex items-center justify-between bg-white rounded-[--radius-md] border border-ink-200/60 px-4 py-3.5 shadow-card hover:shadow-float hover:-translate-y-px transition-all duration-standard animate-fade-up"
                style={{ animationDelay: `${i * 40}ms` }}
              >
                <div className="flex items-center gap-3">
                  <div className="h-9 w-9 rounded-[--radius-sm] bg-tide-50 flex items-center justify-center">
                    <span className="text-xs font-heading font-bold text-tide-600 uppercase">
                      {event.category.slice(0, 3)}
                    </span>
                  </div>
                  <div>
                    <p className="font-heading font-medium text-ink-900 group-hover:text-tide-700 transition-colors">
                      {event.name}
                    </p>
                    <p className="text-xs text-ink-400 font-heading mt-0.5">
                      {event.ageGroup} · {event.gender}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  {event.hasResults && (
                    <span className="text-[11px] font-heading font-medium text-signal-green bg-signal-green/8 px-2 py-0.5 rounded-full">
                      Results
                    </span>
                  )}
                  {event.roundCount > 0 && !event.hasResults && (
                    <span className="text-[11px] font-heading font-medium text-tide-600 bg-tide-50 px-2 py-0.5 rounded-full">
                      {event.roundCount} rnd{event.roundCount !== 1 ? "s" : ""}
                    </span>
                  )}
                  <svg className="h-4 w-4 text-ink-300 group-hover:text-tide-500 transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="m8.25 4.5 7.5 7.5-7.5 7.5" />
                  </svg>
                </div>
              </Link>
            ))}
          </div>
        </section>
      </div>

      {/* Right column: Standings + QR */}
      <div className="space-y-6">
        {/* Mini leaderboard */}
        {leaderboard.standings.length > 0 && (
          <section className="bg-white rounded-[--radius-lg] border border-ink-200/60 shadow-card overflow-hidden">
            <div className="px-4 py-3 border-b border-ink-100 flex items-center justify-between">
              <h2 className="text-sm font-heading font-semibold text-ink-700">Club Standings</h2>
              <Link
                href={`/carnival/${id}/leaderboard`}
                className="text-[11px] font-heading font-medium text-tide-600 hover:text-tide-700 transition-colors"
              >
                View all &rarr;
              </Link>
            </div>
            <div>
              {leaderboard.standings.slice(0, 5).map((s, i) => (
                <div key={s.clubId} className="flex items-center justify-between px-4 py-2.5 border-b border-ink-100/60 last:border-0">
                  <div className="flex items-center gap-3">
                    <span className={`font-heading font-bold tabular-nums w-5 text-center ${rankColors[s.rank] ?? "text-ink-300"}`}>
                      {s.rank}
                    </span>
                    <div>
                      <span className="text-sm font-heading font-medium text-ink-900">{s.clubName}</span>
                    </div>
                  </div>
                  <span className="font-data font-bold tabular-nums text-ink-700">
                    {s.totalPoints}
                  </span>
                </div>
              ))}
            </div>
          </section>
        )}

        {/* QR Code */}
        <div className="bg-white rounded-[--radius-lg] border border-ink-200/60 shadow-card p-5 text-center">
          <p className="text-xs font-heading font-medium uppercase tracking-wider text-ink-400 mb-3">
            Share Results
          </p>
          <CarnivalQR carnivalId={id} />
          <p className="text-xs text-ink-400 mt-3">Scan to view live results</p>
        </div>
      </div>
    </div>
  );
}
