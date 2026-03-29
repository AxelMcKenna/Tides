"use client";

import { QRCodeSVG } from "qrcode.react";

export function CarnivalQR({ carnivalId }: { carnivalId: string }) {
  const url =
    typeof window !== "undefined"
      ? `${window.location.origin}/carnival/${carnivalId}`
      : "";

  if (!url) return null;

  return (
    <div className="flex flex-col items-center gap-2 rounded-lg border border-ink-200 p-4">
      <QRCodeSVG
        value={url}
        size={120}
        bgColor="transparent"
        fgColor="#0F1923"
        level="M"
      />
      <p className="text-xs text-ink-400 text-center">
        Scan for live results
      </p>
    </div>
  );
}
