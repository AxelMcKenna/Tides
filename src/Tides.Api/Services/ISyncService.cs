using Tides.Api.Dtos;

namespace Tides.Api.Services;

public interface ISyncService
{
    Task<SyncBatchResponse> ProcessBatchAsync(SyncBatchRequest request);
}
