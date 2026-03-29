export interface CarnivalListItemResponse {
  id: string;
  name: string;
  sanction: string;
  startDate: string;
  endDate: string;
  eventCount: number;
  hasResults: boolean;
}

export interface CarnivalResponse {
  id: string;
  name: string;
  hostingClubId: string;
  sanction: string;
  startDate: string;
  endDate: string;
  events: EventSummaryResponse[];
}

export interface EventSummaryResponse {
  id: string;
  name: string;
  category: string;
  ageGroup: string;
  gender: string;
  maxLanes: number;
  roundCount: number;
  hasResults: boolean;
}

export interface CarnivalResultsResponse {
  carnivalId: string;
  events: EventResultsResponse[];
}

export interface EventResultsResponse {
  eventId: string;
  eventName: string;
  ageGroup: string;
  gender: string;
  heats: HeatResultsResponse[];
}

export interface HeatResultsResponse {
  heatId: string;
  heatNumber: number;
  roundType: string;
  isComplete: boolean;
  results: ResultResponse[];
}

export interface ResultResponse {
  id: string;
  heatId: string;
  entryId: string;
  placing: number | null;
  time: string | null;
  judgeScore: number | null;
  status: string;
  clubId: string;
  clubName: string;
  members: MemberBriefResponse[];
}

export interface DrawResponse {
  carnivalId: string;
  events: EventDrawResponse[];
}

export interface EventDrawResponse {
  eventId: string;
  eventName: string;
  rounds: RoundDrawResponse[];
}

export interface RoundDrawResponse {
  roundId: string;
  roundType: string;
  roundNumber: number;
  heats: HeatDrawResponse[];
}

export interface HeatDrawResponse {
  heatId: string;
  heatNumber: number;
  isComplete: boolean;
  entries: LaneEntryResponse[];
}

export interface LaneEntryResponse {
  entryId: string;
  lane: number | null;
  clubId: string;
  clubName: string;
  members: MemberBriefResponse[];
  isWithdrawn: boolean;
}

export interface LeaderboardResponse {
  carnivalId: string;
  standings: ClubStandingResponse[];
}

export interface ClubStandingResponse {
  rank: number;
  clubId: string;
  clubName: string;
  clubAbbreviation: string;
  totalPoints: number;
}

export interface MemberBriefResponse {
  id: string;
  firstName: string;
  lastName: string;
}
