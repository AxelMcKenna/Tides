import Link from "next/link";
import { api } from "@/lib/api";
import { formatDateRange } from "@/lib/dates";
import { CarnivalTabs } from "@/components/CarnivalTabs";

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
        <div className="max-w-7xl mx-auto px-5 py-6">
          <div className="flex items-center gap-4">
            <Link
              href="/"
              aria-label="Back to carnivals"
              className="h-10 w-10 flex items-center justify-center rounded-[--radius-sm] text-tide-300 hover:bg-tide-800 hover:text-white transition-colors duration-fast shrink-0"
            >
              <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
              </svg>
            </Link>
            <div className="min-w-0">
              <h1 className="text-3xl font-heading font-bold text-white truncate">{carnival.name}</h1>
              <div className="flex items-center gap-2.5 mt-1">
                <span className="text-sm text-tide-300 font-heading">
                  {formatDateRange(carnival.startDate, carnival.endDate)}
                </span>
                <span className="text-xs font-heading font-semibold uppercase tracking-wider text-tide-300 bg-tide-800 px-2 py-0.5 rounded">
                  {carnival.sanction}
                </span>
              </div>
            </div>
          </div>
        </div>
      </header>

      <main id="main-content" className="max-w-7xl mx-auto px-5 py-6">
        <CarnivalTabs carnivalId={id} />
        {children}
      </main>
    </div>
  );
}
