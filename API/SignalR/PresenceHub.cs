using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub : Hub
{
    private readonly PresenceTracker _tracker;

    public override async Task OnConnectedAsync()
    {
        var isOnline = await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
        //Can access the Clients object, use to invoke methods on clients that are connected to this hub
        //When this user is connected and everyone that is connected to the same hub will receive 
        //the username that has just connected
        if (isOnline)
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
        
        //notify the client who is currently online
        var currentUsers = await _tracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }

    public PresenceHub(PresenceTracker tracker)
    {
        _tracker = tracker;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var isOffline = await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

        if (isOffline)
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

        await base.OnDisconnectedAsync(exception);
    }
}