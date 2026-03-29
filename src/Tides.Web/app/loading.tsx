export default function HomeLoading() {
  return (
    <main className="min-h-screen bg-ink-50">
      <div className="bg-tide-900">
        <div className="max-w-7xl mx-auto px-5 py-10">
          <div className="h-9 w-28 bg-tide-800 rounded animate-pulse" />
          <div className="h-4 w-64 bg-tide-800/60 rounded mt-2 animate-pulse" />
        </div>
      </div>
      <div className="max-w-7xl mx-auto px-5 py-8 space-y-8">
        <div className="h-11 w-full bg-white rounded-[--radius-md] border border-ink-200/60 animate-pulse" />
        {[1, 2, 3].map((i) => (
          <div key={i} className="space-y-3">
            <div className="flex items-center gap-2.5">
              <div className="h-4 w-1 rounded-full bg-tide-500" />
              <div className="h-3 w-20 bg-ink-200 rounded animate-pulse" />
            </div>
            {[1, 2].map((j) => (
              <div key={j} className="bg-white rounded-[--radius-lg] border border-ink-200/60 px-5 py-4 shadow-card">
                <div className="h-5 w-48 bg-ink-100 rounded animate-pulse" />
                <div className="h-3 w-32 bg-ink-50 rounded mt-2 animate-pulse" />
              </div>
            ))}
          </div>
        ))}
      </div>
    </main>
  );
}
