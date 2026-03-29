export default function DisplayLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  // No carnival shell — full screen, no tabs, no header
  return <>{children}</>;
}
