"use client";

import {
  HubConnectionBuilder,
  HubConnection,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { useEffect, useRef, useState } from "react";
import type { ResultResponse, LeaderboardResponse } from "./types";

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5266";

export type ConnectionStatus = "connecting" | "connected" | "reconnecting" | "disconnected";

export type SignalRHandlers = {
  ResultRecorded?: (result: ResultResponse) => void;
  ResultCorrected?: (result: ResultResponse) => void;
  LeaderboardUpdated?: (leaderboard: LeaderboardResponse) => void;
};

export function useSignalR(carnivalId: string, handlers: SignalRHandlers) {
  const connectionRef = useRef<HubConnection | null>(null);
  const [status, setStatus] = useState<ConnectionStatus>("connecting");

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE}/hubs/results`)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    for (const [event, handler] of Object.entries(handlers)) {
      if (handler) connection.on(event, handler as (...args: unknown[]) => void);
    }

    connection.onreconnecting(() => setStatus("reconnecting"));
    connection.onreconnected(() => setStatus("connected"));
    connection.onclose(() => setStatus("disconnected"));

    connection
      .start()
      .then(() => {
        setStatus("connected");
        return connection.invoke("JoinCarnival", carnivalId);
      })
      .catch(() => setStatus("disconnected"));

    return () => {
      connection.invoke("LeaveCarnival", carnivalId).catch(() => {});
      connection.stop();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [carnivalId]);

  return status;
}
