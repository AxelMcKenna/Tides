export default function HomeLoading() {
  return (
    <main className="min-h-screen">
      <header className="border-b border-ink-200 px-4 py-6">
        <div className="max-w-4xl mx-auto">
          <div className="h-7 w-20 bg-ink-100 rounded animate-pulse" />
          <div className="h-4 w-56 bg-ink-50 rounded mt-2 animate-pulse" />
        </div>
      </header>
      <div className="max-w-4xl mx-auto px-4 py-6 space-y-8">
        {[1, 2, 3].map((i) => (
          <div key={i} className="space-y-2">
            <div className="h-4 w-24 bg-ink-100 rounded animate-pulse" />
            <div className="rounded-lg border border-ink-200 px-4 py-4">
              <div className="h-5 w-48 bg-ink-100 rounded animate-pulse" />
              <div className="h-3 w-32 bg-ink-50 rounded mt-2 animate-pulse" />
            </div>
          </div>
        ))}
      </div>
    </main>
  );
}
