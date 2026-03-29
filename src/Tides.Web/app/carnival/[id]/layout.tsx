import Link from "next/link";
import { api } from "@/lib/api";
import { CarnivalTabs } from "@/components/CarnivalTabs";

function formatDateRange(start: string, end: string): string {
  const s = new Date(start + "T00:00:00");
  const e = new Date(end + "T00:00:00");
  const fmt = (d: Date) =>
    d.toLocaleDateString("en-NZ", { day: "numeric", month: "short", year: "numeric" });
  if (start === end) return fmt(s);
  return `${fmt(s)} — ${fmt(e)}`;
}

export default async function CarnivalLayout({
  children,
  params,
}: {
  children: React.ReactNode;
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const carnival = await api.getCarnival(id);

  return (
    <div className="min-h-screen bg-ink-50">
      <header className="bg-tide-900">
        <div className="max-w-7xl mx-auto px-5">
          {/* Top row: back + brand */}
          <div className="flex items-center justify-between py-3">
            <Link
              href="/"
              className="text-xs font-heading font-medium text-tide-300/60 hover:text-white transition-colors duration-fast"
            >
              &larr; All carnivals
            </Link>
            <span className="text-[11px] font-heading font-semibold uppercase tracking-widest text-tide-400/40">
              Tides
            </span>
          </div>

          {/* Carnival info */}
          <div className="pb-4">
            <h1 className="text-2xl font-heading font-bold text-white">{carnival.name}</h1>
            <div className="flex items-center gap-3 mt-1.5">
              <span className="text-sm text-tide-300/70 font-heading">
                {formatDateRange(carnival.startDate, carnival.endDate)}
              </span>
              <span className="text-xs font-heading font-semibold uppercase tracking-wider text-tide-400/80 bg-tide-800 px-2 py-0.5 rounded">
                {carnival.sanction}
              </span>
            </div>
          </div>

          {/* Tabs — light on dark */}
          <CarnivalTabs carnivalId={id} />
        </div>
      </header>

      <main id="main-content" className="max-w-7xl mx-auto px-5 py-6">
        {children}
      </main>
    </div>
  );
}
