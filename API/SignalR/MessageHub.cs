using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub : Hub
{
    private readonly IMessageRepository _messageRepository;

    public MessageHub(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    //Get the name of the user we are connected to > send it up in the query string and retrieve from the http request
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext.Request.Query["user"];
        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var messages = await _messageRepository
            .GetMessageThread(Context.User.GetUsername(), otherUser);

        //users will receive messages from SignalR Instead of making an API call (which requires refreshing browser)
        await Clients.Groups(groupName).SendAsync("ReceiveMessageThread");
    }

    //When a user disconnects from signalR they move to a different part of our application.
    //Unlike presence hub which we are always connected to.
    //Automatically removed by SignalR from any groups they belong to.
    public override Task OnDisconnectedAsync(Exception exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    //group name is a combination of both users but we don't know who will join first
    //order it so the group name is always the same no matter who joins first
    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}{caller}";
    }
}