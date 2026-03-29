using Tides.Api.Dtos;

namespace Tides.Api.Services;

public interface ICarnivalService
{
    Task<List<CarnivalListItemResponse>> ListCarnivalsAsync();
    Task<CarnivalResponse> CreateCarnivalAsync(CreateCarnivalRequest request);
    Task<CarnivalResponse> GetCarnivalAsync(Guid id);
    Task<DrawResponse> GetDrawAsync(Guid carnivalId);
    Task<CarnivalResultsResponse> GetResultsAsync(Guid carnivalId, Guid? eventId = null);
    Task<LeaderboardResponse> GetLeaderboardAsync(Guid carnivalId, string? ageGroup = null);
    Task<HeatDrawResponse> GetHeatAsync(Guid carnivalId, Guid heatId);
    Task<DrawResponse> GenerateDrawAsync(Guid carnivalId, GenerateDrawRequest request);
    Task<EntryResponse> CreateEntryAsync(Guid carnivalId, CreateEntryRequest request);
    Task WithdrawEntryAsync(Guid carnivalId, Guid entryId);
    Task<ResultResponse> RecordResultAsync(Guid carnivalId, RecordResultRequest request);
    Task<ResultResponse> CorrectResultAsync(Guid resultId, CorrectResultRequest request);
    Task DeleteResultAsync(Guid resultId);
    Task<ProtestResponse> LodgeProtestAsync(Guid resultId, LodgeProtestRequest request);
    Task<ProtestResponse> AdjudicateProtestAsync(Guid protestId, AdjudicateProtestRequest request);
}
