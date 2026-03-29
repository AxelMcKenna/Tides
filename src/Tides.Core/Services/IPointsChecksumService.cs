using Tides.Core.Domain;

namespace Tides.Core.Services;

public interface IPointsChecksumService
{
    string ComputeChecksum(IReadOnlyList<ResultEvent> activeEvents);
}
