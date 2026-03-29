namespace Tides.Core.Services;

public interface ISurfguardSyncService
{
    Task SyncMembersAsync(Guid carnivalId, CancellationToken ct = default);
    Task WritebackResultsAsync(Guid carnivalId, CancellationToken ct = default);
}
