using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace API.Controllers;

public class MessagesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, 
        IMapper mapper)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult> CreateMessage(CreateMessageDto createMessageDto)
    {
        //get username from the token
        var username = User.GetUsername();

        if (username == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("You cannot send messages to yourself");

        var sender = await _userRepository.GetUserByUsernameAsync(username);
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null) return NotFound();

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

        if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

        return BadRequest("Failed to send message");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        var messages = await _messageRepository.GetMessagesForUser(messageParams);
        
        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, 
            messages.TotalCount, messages.TotalPages));

        return messages;
    }
}