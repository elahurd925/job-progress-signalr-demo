using Microsoft.AspNetCore.SignalR;

public class MergeHub : Hub
{
    public async Task JoinGroup(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
    }
}
