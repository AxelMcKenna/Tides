import { api } from "@/lib/api";

export default async function DrawPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const draw = await api.getDraw(id);

  if (draw.events.every((e) => e.rounds.length === 0)) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center">
        <div className="h-12 w-12 rounded-full bg-ink-100 flex items-center justify-center mb-4">
          <svg className="h-6 w-6 text-ink-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M3.75 6A2.25 2.25 0 0 1 6 3.75h2.25A2.25 2.25 0 0 1 10.5 6v2.25a2.25 2.25 0 0 1-2.25 2.25H6a2.25 2.25 0 0 1-2.25-2.25V6Z" />
          </svg>
        </div>
        <p className="text-ink-500 font-heading font-medium">No draws yet</p>
        <p className="text-sm text-ink-400 mt-1">Heat draws will appear here once generated.</p>
      </div>
    );
  }

  return (
    <div className="space-y-10">
      {draw.events
        .filter((e) => e.rounds.length > 0)
        .map((event, eventIdx) => (
          <section
            key={event.eventId}
            className="animate-fade-up"
            style={{ animationDelay: `${eventIdx * 60}ms` }}
          >
            <h2 className="text-lg font-heading font-semibold text-ink-900 mb-4">
              {event.eventName}
            </h2>

            {event.rounds.map((round) => (
              <div key={round.roundId} className="mb-6">
                <h3 className="text-xs font-heading font-medium uppercase tracking-wider text-ink-400 mb-3">
                  {round.roundType} {round.roundNumber}
                </h3>

                <div className="grid gap-4 md:grid-cols-2">
                  {round.heats.map((heat, heatIdx) => (
                    <div
                      key={heat.heatId}
                      className="heat-card bg-white rounded-[--radius-lg] border border-ink-200/60 shadow-card overflow-hidden"
                    >
                      {/* Heat header */}
                      <div className="px-4 py-2.5 border-b border-ink-100 flex items-center justify-between bg-ink-50/30">
                        <div className="flex items-center gap-2">
                          <span className="inline-flex items-center justify-center h-6 w-6 rounded-[--radius-xs] bg-tide-900 text-[11px] font-heading font-bold text-white tabular-nums">
                            {heat.heatNumber}
                          </span>
                          <span className="text-sm font-heading font-medium text-ink-700">
                            Heat {heat.heatNumber}
                          </span>
                        </div>
                        {heat.isComplete && (
                          <span className="inline-flex items-center gap-1 text-[11px] font-heading font-medium text-signal-green">
                            <svg className="h-3 w-3" fill="currentColor" viewBox="0 0 20 20">
                              <path fillRule="evenodd" d="M16.704 4.153a.75.75 0 0 1 .143 1.052l-8 10.5a.75.75 0 0 1-1.127.075l-4.5-4.5a.75.75 0 0 1 1.06-1.06l3.894 3.893 7.48-9.817a.75.75 0 0 1 1.05-.143Z" clipRule="evenodd" />
                            </svg>
                            Complete
                          </span>
                        )}
                      </div>

                      {/* Lane assignments */}
                      <div>
                        {heat.entries
                          .sort((a, b) => (a.lane ?? 99) - (b.lane ?? 99))
                          .map((entry, i) => (
                            <div
                              key={entry.entryId}
                              className={`flex items-center gap-3 px-4 py-2.5 ${
                                i > 0 ? "border-t border-ink-100/60" : ""
                              } ${entry.isWithdrawn ? "opacity-40" : ""}`}
                            >
                              <span className="font-data font-medium tabular-nums text-ink-400 w-5 text-center text-sm">
                                {entry.lane ?? "—"}
                              </span>
                              <div className="flex-1 min-w-0">
                                <p className={`text-sm font-heading font-medium text-ink-900 truncate ${entry.isWithdrawn ? "line-through" : ""}`}>
                                  {entry.members
                                    .map((m) => `${m.firstName} ${m.lastName}`)
                                    .join(", ")}
                                </p>
                              </div>
                              <span className="text-xs font-heading text-ink-400 shrink-0">
                                {entry.clubName}
                              </span>
                            </div>
                          ))}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </section>
        ))}
    </div>
  );
}
