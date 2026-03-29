import type {
  CarnivalListItemResponse,
  CarnivalResponse,
  CarnivalResultsResponse,
  DrawResponse,
  LeaderboardResponse,
} from "./types";

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5266";

async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    next: { revalidate: 30 },
  });
  if (!res.ok) throw new Error(`API ${res.status}: ${path}`);
  return res.json();
}

export const api = {
  listCarnivals: () =>
    apiFetch<CarnivalListItemResponse[]>(`/api/carnivals`),

  getCarnival: (id: string) =>
    apiFetch<CarnivalResponse>(`/api/carnivals/${id}`),

  getDraw: (id: string) =>
    apiFetch<DrawResponse>(`/api/carnivals/${id}/draw`),

  getResults: (id: string, eventId?: string) =>
    apiFetch<CarnivalResultsResponse>(
      `/api/carnivals/${id}/results${eventId ? `?eventId=${eventId}` : ""}`
    ),

  getLeaderboard: (id: string, ageGroup?: string) =>
    apiFetch<LeaderboardResponse>(
      `/api/carnivals/${id}/leaderboard${ageGroup ? `?ageGroup=${ageGroup}` : ""}`
    ),
};
