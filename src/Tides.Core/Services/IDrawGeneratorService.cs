using Tides.Core.Domain;
using Tides.Core.Domain.Enums;

namespace Tides.Core.Services;

public interface IDrawGeneratorService
{
    DrawResult GenerateHeats(List<Entry> entries, int maxLanes, RoundType roundType);

    DrawResult GenerateSeededDraw(
        List<Result> previousRoundResults,
        Dictionary<Guid, Entry> entriesById,
        int maxLanes,
        AdvancementRule rule,
        int topN,
        int fastestN);

    DrawResult RedrawOnWithdrawal(DrawResult currentDraw, Guid withdrawnEntryId);

    DrawResult ManuallyAssign(DrawResult currentDraw, Guid entryId, int heatIndex, int lane, string userId);
}
