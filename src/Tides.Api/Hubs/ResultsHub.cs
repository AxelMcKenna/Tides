using Microsoft.AspNetCore.SignalR;

namespace Tides.Api.Hubs;

public class ResultsHub : Hub
{
    public Task JoinCarnival(Guid carnivalId)
        => Groups.AddToGroupAsync(Context.ConnectionId, $"carnival-{carnivalId}");

    public Task LeaveCarnival(Guid carnivalId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"carnival-{carnivalId}");
}
