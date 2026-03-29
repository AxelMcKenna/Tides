using System.Security.Cryptography;
using System.Text;
using Tides.Core.Domain;

namespace Tides.Core.Services;

public class PointsChecksumService : IPointsChecksumService
{
    public string ComputeChecksum(IReadOnlyList<ResultEvent> activeEvents)
    {
        var sorted = activeEvents
            .OrderBy(e => e.HeatId)
            .ThenBy(e => e.EntryId)
            .ThenBy(e => e.EventId);

        var sb = new StringBuilder();
        foreach (var e in sorted)
        {
            sb.Append(e.EventId);
            sb.Append('|');
            sb.Append(e.Placing);
            sb.Append('|');
            sb.Append(e.Time);
            sb.Append('|');
            sb.Append(e.JudgeScore);
            sb.Append('|');
            sb.Append(e.Status);
            sb.Append('\n');
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexStringLower(hash);
    }
}
