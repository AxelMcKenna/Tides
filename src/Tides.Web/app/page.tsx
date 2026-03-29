import { api } from "@/lib/api";
import { CarnivalSearch } from "@/components/CarnivalSearch";

export const revalidate = 30;

export default async function Home() {
  const carnivals = await api.listCarnivals();

  if (carnivals.length === 0) {
    return (
      <main className="flex min-h-screen flex-col items-center justify-center bg-ink-50">
        <h1 className="text-5xl font-heading font-bold text-ink-900 uppercase tracking-[0.3em]">Shore</h1>
        <p className="mt-3 text-ink-400 font-heading">No carnivals yet</p>
      </main>
    );
  }

  return (
    <main id="main-content" className="min-h-screen bg-ink-50">
      <div className="bg-tide-900 header-accent">
        <div className="max-w-7xl mx-auto px-5 py-12">
          <h1 className="text-5xl font-heading font-bold text-white uppercase tracking-[0.3em]">Shore</h1>
          <p className="text-tide-300 font-heading text-sm mt-2 uppercase tracking-wider">
            Surf Life Saving competition results
          </p>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-5 py-8">
        <CarnivalSearch carnivals={carnivals} />
      </div>
    </main>
  );
}
