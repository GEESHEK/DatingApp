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
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public MessageHub(IMessageRepository messageRepository, IUserRepository userRepository, 
        IMapper mapper)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _mapper = mapper;
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

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        //Get username from the token
        var username = Context.User.GetUsername();

        //Not inside an api controller so can't return Http response replace all with hub exceptions
        if (username == createMessageDto.RecipientUsername.ToLower())
            throw new HubException("You cannot send messages to yourself");

        var sender = await _userRepository.GetUserByUsernameAsync(username);
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

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
        
        //use the context .add to track it
        _messageRepository.AddMessage(message);

        if (await _messageRepository.SaveAllAsync())
        {
            var group = GetGroupName(sender.UserName, recipient.UserName);
            await Clients.Group(group).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
        }
    }

    //group name is a combination of both users but we don't know who will join first
    //order it so the group name is always the same no matter who joins first
    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}{caller}";
    }
}