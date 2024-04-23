using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        //Can access the Clients object, use to invoke methods on clients that are connected to this hub
        //When this user is connected and everyone that is connected to the same hub will receive 
        //the username that has just connected
        await Clients.Others.SendAsync("userIsOnline", Context.User.GetUsername());
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

        await base.OnDisconnectedAsync(exception);
    }
}