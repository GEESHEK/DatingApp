using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub : Hub
{
    private readonly PresenceTracker _tracker;

    public PresenceHub(PresenceTracker tracker)
    {
        _tracker = tracker;
    }
    
    public override async Task OnConnectedAsync()
    {
        await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
        //Can access the Clients object, use to invoke methods on clients that are connected to this hub
        //When this user is connected and everyone that is connected to the same hub will receive 
        //the username that has just connected
        await Clients.Others.SendAsync("userIsOnline", Context.User.GetUsername());

        //notify the client who is currently online
        var currentUsers = await _tracker.GetOnlineUsers();
        await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
        
        await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

        //get current user minus the one we just removed
        var currentUsers = await _tracker.GetOnlineUsers();
        //keep listening to who is online so use the same method as OnConnectedAsync
        await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

        await base.OnDisconnectedAsync(exception);
    }
}