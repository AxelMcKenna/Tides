export default function CarnivalLoading() {
  return (
    <div className="space-y-8 animate-pulse">
      {/* Leaderboard skeleton */}
      <section>
        <div className="flex items-center justify-between mb-3">
          <div className="h-5 w-32 bg-ink-100 rounded" />
          <div className="h-4 w-28 bg-ink-50 rounded" />
        </div>
        <div className="rounded-lg border border-ink-200 overflow-hidden">
          {[1, 2, 3, 4, 5].map((i) => (
            <div
              key={i}
              className="flex items-center justify-between px-4 py-3 border-b last:border-0 border-ink-200"
            >
              <div className="flex items-center gap-3">
                <div className="h-5 w-5 bg-ink-100 rounded" />
                <div className="h-4 w-36 bg-ink-100 rounded" />
              </div>
              <div className="h-5 w-10 bg-ink-100 rounded" />
            </div>
          ))}
        </div>
      </section>

      {/* Events skeleton */}
      <section>
        <div className="h-5 w-20 bg-ink-100 rounded mb-3" />
        <div className="space-y-2">
          {[1, 2, 3, 4].map((i) => (
            <div
              key={i}
              className="rounded-lg border border-ink-200 px-4 py-3"
            >
              <div className="h-4 w-44 bg-ink-100 rounded" />
              <div className="h-3 w-28 bg-ink-50 rounded mt-2" />
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
