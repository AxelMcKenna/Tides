import { api } from "@/lib/api";
import { LiveResults } from "@/components/LiveResults";
import { EventFilter } from "@/components/EventFilter";

export default async function ResultsPage({
  params,
  searchParams,
}: {
  params: Promise<{ id: string }>;
  searchParams: Promise<{ eventId?: string }>;
}) {
  const { id } = await params;
  const { eventId } = await searchParams;
  const [carnival, results] = await Promise.all([
    api.getCarnival(id),
    api.getResults(id, eventId),
  ]);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <EventFilter events={carnival.events} carnivalId={id} />
      </div>
      <LiveResults carnivalId={id} initialData={results} />
    </div>
  );
}
