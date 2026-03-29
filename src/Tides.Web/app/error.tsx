"use client";

export default function HomeError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <main className="flex min-h-screen items-center justify-center px-4">
      <div className="text-center max-w-md">
        <h1 className="text-2xl font-heading font-bold uppercase tracking-[0.3em]">Tides</h1>
        <p className="mt-3 text-sm text-ink-500">
          We couldn&apos;t load carnivals right now. This might be a temporary
          issue.
        </p>
        <button
          onClick={reset}
          className="mt-4 rounded-md bg-tide-500 px-4 py-2 text-sm font-medium text-white hover:bg-tide-700 transition-colors cursor-pointer"
        >
          Try again
        </button>
      </div>
    </main>
  );
}
