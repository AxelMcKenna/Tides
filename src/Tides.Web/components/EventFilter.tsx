"use client";

import { useRouter, useSearchParams } from "next/navigation";
import type { EventSummaryResponse } from "@/lib/types";

export function EventFilter({
  events,
  carnivalId,
}: {
  events: EventSummaryResponse[];
  carnivalId: string;
}) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const current = searchParams.get("eventId") ?? "";

  return (
    <div>
      <label htmlFor="event-filter" className="sr-only">
        Filter by event
      </label>
      <select
        id="event-filter"
        value={current}
        onChange={(e) => {
          const val = e.target.value;
          const path = val
            ? `/carnival/${carnivalId}/results?eventId=${val}`
            : `/carnival/${carnivalId}/results`;
          router.push(path);
        }}
        className="rounded-md border border-ink-200 bg-white px-3 py-2 text-sm text-ink-900 cursor-pointer hover:border-ink-400 transition-colors"
      >
        <option value="">All events</option>
        {events.map((e) => (
          <option key={e.id} value={e.id}>
            {e.name} — {e.ageGroup} {e.gender}
          </option>
        ))}
      </select>
    </div>
  );
}
