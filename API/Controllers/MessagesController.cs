using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController : BaseApiController
{
    private readonly IUserRepository userRepository;
    private readonly IMessageRepository messageRepository;
    private readonly IMapper mapper;

    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
        this.userRepository = userRepository;
        this.messageRepository = messageRepository;
        this.mapper = mapper;
    }

    // Current user sends a message.
    [HttpPost]  // /api/messages
    public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
    {
        var senderUsername = User.GetUsername();

        if (createMessageDTO.RecipientUsername.ToLower() == senderUsername)
            return BadRequest("API ERROR: You cannot send messages to yourself!");
        
        var sender = await this.userRepository.GetUserByUsernameAsync(senderUsername);
        var recipient = await this.userRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);

        if (recipient == null) return NotFound();

        var message = new Message
        {
            // The EF will take care of the ID fields, but not the username fields. That is why they are being explicitly added here.
            Sender = sender,
            SenderUserName = sender.UserName,
            Recipient = recipient,
            RecipientUsername = recipient.UserName,
            Content = createMessageDTO.Content
        };

        // Save the message to the message repository.
        this.messageRepository.AddMessage(message);

        if (await this.messageRepository.SaveAllAsync())
        {
            // Return the MessageDTO with the status 200 OK message.
            return Ok(this.mapper.Map<MessageDTO>(message));
        }

        return BadRequest("API ERROR: Failed to send message.");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        var messages = await this.messageRepository.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, 
            messages.TotalCount, messages.TotalPages));

        return Ok(messages);
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string username)
    {
        return Ok(await this.messageRepository.GetMessageThread(User.GetUsername(), username));
    }

    // The message will only be deleted if both users have deleted it on their ends.
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var currentUsername = User.GetUsername();
        var message = await this.messageRepository.GetMessage(id);

        // If the user trying to delete the message is neither the sender nor the recipient, return 404 Unauthorized.
        if (message.SenderUserName != currentUsername && message.RecipientUsername != currentUsername)
        {
            return Unauthorized();
        }

        // Set the SenderDeleted or the RecipientDeleted properties of the message based on who the current user is.
        if (message.SenderUserName == currentUsername) message.SenderDeleted = true;
        if (message.RecipientUsername == currentUsername) message.RecipientDeleted = true;

        // Delete the message only if both the sender and recipient have deleted the message.
        if (message.SenderDeleted && message.RecipientDeleted)
        {
            this.messageRepository.DeleteMessage(message);
        }

        // Update the DB.
        if (await this.messageRepository.SaveAllAsync()) return Ok();

        return BadRequest("API ERROR: There was a problem deleting the message");
    }
}
