using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub : Hub
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;

    public MessageHub(IUnitOfWork uow, IMapper mapper, IHubContext<PresenceHub> presenceHub)
    {
        _uow = uow;
        _mapper = mapper;
        _presenceHub = presenceHub;
    }

    //Get the name of the user we are connected to > send it up in the query string and retrieve from the http request
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        //'message?user='
        var otherUser = httpContext.Request.Query["user"]; 
        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
        //add the group to signalR
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        //add group to db
        var group = await AddToGroup(groupName);
        
        //So client know who's inside a group in a given time
        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await _uow.MessageRepository
            .GetMessageThread(Context.User.GetUsername(), otherUser);

        if (_uow.HasChanges()) await _uow.Complete();

        //users will receive messages from SignalR Instead of making an API call (which requires refreshing browser)
        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    //When a user disconnects from signalR they move to a different part of our application.
    //Unlike presence hub which we are always connected to.
    //Automatically removed by SignalR from any groups they belong to.
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        //Get username from the token
        var username = Context.User.GetUsername();

        //Not inside an api controller so can't return Http response replace all with hub exceptions
        if (username == createMessageDto.RecipientUsername.ToLower())
            throw new HubException("You cannot send messages to yourself");

        var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
        var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null) throw new HubException("Not found user");

        //ef is not tracking this because we did not retrieve it
        var message = new Message
        {
            //entity framework from take care of the ID fields for us based on the sender and recipient entity
            Sender = sender,
            Recipient = recipient,
            //username will have to be set manually, EF doesn't take care of that
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);

        var group = await _uow.MessageRepository.GetMessageGroup(groupName);

        //check our connection to see if there is any username that matches the recipient username
        //checking to see if user is in that message group(message group has that connection)
        if (group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            //not null then we know user is logged in just not in message tab(connected to message hub)
            if (connections != null)
            {
                //we are using our presence hub to send this to the client
                await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                    new {username = sender.UserName, knownAs = sender.KnownAs});
            }
        }
        
        //use the context .add to track it
        _uow.MessageRepository.AddMessage(message);

        if (await _uow.Complete())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
        }
    }

    //group name is a combination of both users but we don't know who will join first
    //order it so the group name is always the same no matter who joins first
    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    //Add group to db
    private async Task<Group> AddToGroup(string groupName)
    {
        var group = await _uow.MessageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

        if (group == null)
        {
            group = new Group(groupName);
            _uow.MessageRepository.AddGroup(group);
        }
        
        group.Connections.Add(connection);

        if (await _uow.Complete()) return group;
        
        throw new HubException("Failed to add to group");
    }

    //Remove connection from db
    private async Task<Group> RemoveFromMessageGroup()
    {
        var group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        _uow.MessageRepository.RemoveConnection(connection);
        
        if (await _uow.Complete()) return group;

        throw new HubException("Failed to remove from group");
    }
}