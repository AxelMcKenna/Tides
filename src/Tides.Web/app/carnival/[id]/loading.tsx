export default function CarnivalLoading() {
  return (
    <div className="grid gap-6 lg:grid-cols-3">
      {/* Events skeleton */}
      <div className="lg:col-span-2 space-y-4">
        <div className="h-11 w-full bg-white rounded-[--radius-md] border border-ink-200/60 animate-pulse" />
        <div className="flex flex-wrap gap-2">
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="h-7 w-16 bg-ink-100 rounded-full animate-pulse" />
          ))}
        </div>
        <div className="space-y-2">
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <div
              key={i}
              className="bg-white rounded-[--radius-md] border border-ink-200/60 px-4 py-3.5 shadow-card flex items-center gap-3"
            >
              <div className="h-9 w-9 rounded-[--radius-sm] bg-ink-100 animate-pulse shrink-0" />
              <div className="flex-1">
                <div className="h-4 w-44 bg-ink-100 rounded animate-pulse" />
                <div className="h-3 w-28 bg-ink-50 rounded mt-2 animate-pulse" />
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Sidebar skeleton */}
      <div className="space-y-6">
        <div className="bg-white rounded-[--radius-lg] border border-ink-200/60 shadow-card overflow-hidden">
          <div className="px-4 py-3 border-b border-ink-100">
            <div className="h-3 w-24 bg-ink-100 rounded animate-pulse" />
          </div>
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="flex items-center justify-between px-4 py-2.5 border-b border-ink-100/60 last:border-0">
              <div className="flex items-center gap-3">
                <div className="h-4 w-4 bg-ink-100 rounded animate-pulse" />
                <div className="h-4 w-28 bg-ink-100 rounded animate-pulse" />
              </div>
              <div className="h-4 w-8 bg-ink-100 rounded animate-pulse" />
            </div>
          ))}
        </div>
        <div className="bg-white rounded-[--radius-lg] border border-ink-200/60 shadow-card p-5 flex flex-col items-center">
          <div className="h-3 w-20 bg-ink-100 rounded animate-pulse mb-3" />
          <div className="h-[120px] w-[120px] bg-ink-100 rounded animate-pulse" />
          <div className="h-3 w-32 bg-ink-50 rounded animate-pulse mt-3" />
        </div>
      </div>
    </div>
  );
}
