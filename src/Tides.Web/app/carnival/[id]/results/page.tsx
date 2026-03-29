import { api } from "@/lib/api";
import { LiveResults } from "@/components/LiveResults";

export default async function ResultsPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const results = await api.getResults(id);

  return <LiveResults carnivalId={id} initialData={results} />;
}
