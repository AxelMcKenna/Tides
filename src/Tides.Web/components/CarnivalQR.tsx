"use client";

import { QRCodeSVG } from "qrcode.react";

export function CarnivalQR({ carnivalId }: { carnivalId: string }) {
  const url =
    typeof window !== "undefined"
      ? `${window.location.origin}/carnival/${carnivalId}`
      : "";

  if (!url) return null;

  return (
    <QRCodeSVG
      value={url}
      size={120}
      bgColor="transparent"
      fgColor="#0F1923"
      level="M"
    />
  );
}
