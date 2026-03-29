import { api } from "@/lib/api";
import Link from "next/link";
import { CarnivalQR } from "@/components/CarnivalQR";
import { EventExplorer } from "@/components/EventExplorer";

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

  const today = new Date().toISOString().split("T")[0];
  const isLive = carnival.startDate <= today && carnival.endDate >= today;

  return (
    <div className="grid gap-6 lg:grid-cols-3">
      {/* Left column: Events */}
      <div className="lg:col-span-2 space-y-4">
        <EventExplorer events={carnival.events} carnivalId={id} isLive={isLive} />
      </div>

      {/* Right column: Standings + QR */}
      <div className="space-y-6">
        {leaderboard.standings.length > 0 && (
          <section className="bg-white rounded-[--radius-lg] border border-ink-200 shadow-card overflow-hidden">
            <div className="px-4 py-3 border-b border-ink-100 flex items-center justify-between">
              <h2 className="text-xs font-heading font-bold uppercase tracking-wider text-ink-800">
                Club Standings
              </h2>
              <Link
                href={`/carnival/${id}/leaderboard`}
                className="text-[11px] font-heading font-medium text-tide-600 hover:text-tide-700 transition-colors"
              >
                View all &rarr;
              </Link>
            </div>
            <div>
              {leaderboard.standings.slice(0, 5).map((s) => (
                <div key={s.clubId} className="flex items-center justify-between px-4 py-2.5 border-b border-ink-100/60 last:border-0">
                  <div className="flex items-center gap-3">
                    <span className={`font-heading font-bold tabular-nums w-5 text-center ${rankColors[s.rank] ?? "text-tide-800"}`}>
                      {s.rank}
                    </span>
                    <span className="text-sm font-heading font-semibold text-ink-900">{s.clubName}</span>
                  </div>
                  <span className="font-data font-bold tabular-nums text-ink-700">
                    {s.totalPoints}
                  </span>
                </div>
              ))}
            </div>
          </section>
        )}

        <div className="bg-tide-900 rounded-[--radius-lg] shadow-card p-5 flex flex-col items-center">
          <p className="text-[10px] font-heading font-bold uppercase tracking-[0.2em] text-tide-400 mb-3">
            Share Results
          </p>
          <div className="bg-white rounded-[--radius-md] p-3">
            <CarnivalQR carnivalId={id} />
          </div>
          <p className="text-[11px] text-tide-400 mt-3 font-heading">Scan for live results</p>
        </div>
      </div>
    </div>
  );
}
