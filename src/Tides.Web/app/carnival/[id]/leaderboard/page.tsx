import { api } from "@/lib/api";
import { LiveLeaderboard } from "@/components/LiveLeaderboard";

export default async function LeaderboardPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const leaderboard = await api.getLeaderboard(id);

  return <LiveLeaderboard carnivalId={id} initialData={leaderboard} />;
}
