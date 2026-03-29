using Tides.Api.Dtos;
using Tides.Api.Services;

namespace Tides.Api.Tests.Stubs;

public class StubCarnivalService : ICarnivalService
{
    public Func<Task<List<CarnivalListItemResponse>>>? ListCarnivalsAsyncFunc { get; set; }
    public Func<CreateCarnivalRequest, Task<CarnivalResponse>>? CreateCarnivalAsyncFunc { get; set; }
    public Func<Guid, Task<CarnivalResponse>>? GetCarnivalAsyncFunc { get; set; }
    public Func<Guid, Task<DrawResponse>>? GetDrawAsyncFunc { get; set; }
    public Func<Guid, Guid?, Task<CarnivalResultsResponse>>? GetResultsAsyncFunc { get; set; }
    public Func<Guid, string?, Task<LeaderboardResponse>>? GetLeaderboardAsyncFunc { get; set; }
    public Func<Guid, Guid, Task<HeatDrawResponse>>? GetHeatAsyncFunc { get; set; }
    public Func<Guid, GenerateDrawRequest, Task<DrawResponse>>? GenerateDrawAsyncFunc { get; set; }
    public Func<Guid, CreateEntryRequest, Task<EntryResponse>>? CreateEntryAsyncFunc { get; set; }
    public Func<Guid, Guid, Task>? WithdrawEntryAsyncFunc { get; set; }
    public Func<Guid, RecordResultRequest, Task<ResultResponse>>? RecordResultAsyncFunc { get; set; }
    public Func<Guid, CorrectResultRequest, Task<ResultResponse>>? CorrectResultAsyncFunc { get; set; }
    public Func<Guid, Task>? DeleteResultAsyncFunc { get; set; }
    public Func<Guid, LodgeProtestRequest, Task<ProtestResponse>>? LodgeProtestAsyncFunc { get; set; }
    public Func<Guid, AdjudicateProtestRequest, Task<ProtestResponse>>? AdjudicateProtestAsyncFunc { get; set; }

    public Task<List<CarnivalListItemResponse>> ListCarnivalsAsync()
        => ListCarnivalsAsyncFunc?.Invoke() ?? throw new NotImplementedException();

    public Task<CarnivalResponse> CreateCarnivalAsync(CreateCarnivalRequest request)
        => CreateCarnivalAsyncFunc?.Invoke(request) ?? throw new NotImplementedException();

    public Task<CarnivalResponse> GetCarnivalAsync(Guid id)
        => GetCarnivalAsyncFunc?.Invoke(id) ?? throw new NotImplementedException();

    public Task<DrawResponse> GetDrawAsync(Guid carnivalId)
        => GetDrawAsyncFunc?.Invoke(carnivalId) ?? throw new NotImplementedException();

    public Task<CarnivalResultsResponse> GetResultsAsync(Guid carnivalId, Guid? eventId = null)
        => GetResultsAsyncFunc?.Invoke(carnivalId, eventId) ?? throw new NotImplementedException();

    public Task<LeaderboardResponse> GetLeaderboardAsync(Guid carnivalId, string? ageGroup = null)
        => GetLeaderboardAsyncFunc?.Invoke(carnivalId, ageGroup) ?? throw new NotImplementedException();

    public Task<HeatDrawResponse> GetHeatAsync(Guid carnivalId, Guid heatId)
        => GetHeatAsyncFunc?.Invoke(carnivalId, heatId) ?? throw new NotImplementedException();

    public Task<DrawResponse> GenerateDrawAsync(Guid carnivalId, GenerateDrawRequest request)
        => GenerateDrawAsyncFunc?.Invoke(carnivalId, request) ?? throw new NotImplementedException();

    public Task<EntryResponse> CreateEntryAsync(Guid carnivalId, CreateEntryRequest request)
        => CreateEntryAsyncFunc?.Invoke(carnivalId, request) ?? throw new NotImplementedException();

    public Task WithdrawEntryAsync(Guid carnivalId, Guid entryId)
        => WithdrawEntryAsyncFunc?.Invoke(carnivalId, entryId) ?? Task.CompletedTask;

    public Task<ResultResponse> RecordResultAsync(Guid carnivalId, RecordResultRequest request)
        => RecordResultAsyncFunc?.Invoke(carnivalId, request) ?? throw new NotImplementedException();

    public Task<ResultResponse> CorrectResultAsync(Guid resultId, CorrectResultRequest request)
        => CorrectResultAsyncFunc?.Invoke(resultId, request) ?? throw new NotImplementedException();

    public Task DeleteResultAsync(Guid resultId)
        => DeleteResultAsyncFunc?.Invoke(resultId) ?? Task.CompletedTask;

    public Task<ProtestResponse> LodgeProtestAsync(Guid resultId, LodgeProtestRequest request)
        => LodgeProtestAsyncFunc?.Invoke(resultId, request) ?? throw new NotImplementedException();

    public Task<ProtestResponse> AdjudicateProtestAsync(Guid protestId, AdjudicateProtestRequest request)
        => AdjudicateProtestAsyncFunc?.Invoke(protestId, request) ?? throw new NotImplementedException();
}
