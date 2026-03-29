"use client";

export default function CarnivalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <div className="flex min-h-[50vh] items-center justify-center px-4">
      <div className="text-center max-w-md">
        <h2 className="text-lg font-semibold text-ink-900">
          Something went wrong
        </h2>
        <p className="mt-2 text-sm text-ink-500">
          We couldn&apos;t load the carnival data. This might be a temporary
          issue — try again.
        </p>
        <button
          onClick={reset}
          className="mt-4 rounded-md bg-tide-500 px-4 py-2 text-sm font-medium text-white hover:bg-tide-700 transition-colors cursor-pointer"
        >
          Try again
        </button>
      </div>
    </div>
  );
}
