using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SQLitePCL;

namespace API.Data;

public class MessageRepository : IMessageRepository
{
    private readonly DataContext context;
    private readonly IMapper mapper;

    public MessageRepository(DataContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    public void AddMessage(Message message)
    {
        this.context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        this.context.Messages.Remove(message);
    }

    public async Task<Message> GetMessage(int Id)
    {
        return await this.context.Messages.FindAsync(Id);
    }

    public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
    {
        IQueryable<Message> query = this.context.Messages
            .OrderByDescending(t => t.MessageSent).AsQueryable();

        query = messageParams.Container switch
        {
            // Send all messages sent to the current user.
            "Inbox" =>  query.Where(u => u.RecipientUsername == messageParams.Username 
                && !u.RecipientDeleted),
            // Send all the messages the current user has sent.
            "Outbox" => query.Where(u => u.SenderUserName == messageParams.Username
                && !u.SenderDeleted),
            // "Default case: "Unread".
            _ =>        query.Where(u => u.RecipientUsername == messageParams.Username 
                && !u.RecipientDeleted && u.DateRead == null)
        };

        return await PagedList<MessageDTO>.CreateAsync(
            query.ProjectTo<MessageDTO>(this.mapper.ConfigurationProvider),
            messageParams.PageNumber, messageParams.PageSize
        );
    }

    // public async Task<IEnumerable<MessageDTO>> GetMessagesThread(string currentUser, string otherUser)
    // {
    //     IQueryable<Message> query = this.context.Messages.AsQueryable();
        
    //     query = query.Where(m => 
    //         m.SenderUserName == currentUser && m.RecipientUsername == otherUser ||
    //         m.SenderUserName == otherUser && m.RecipientUsername == currentUser
    //     )
    //     .OrderBy(m => m.MessageSent);

    //     return await query.ProjectTo<MessageDTO>(this.mapper.ConfigurationProvider).ToListAsync();
    // }

    public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername)
    {
        // Get messages from current user to other user and from other user to current user.
        var messages = await this.context.Messages
            .Include(u => u.Sender).ThenInclude(p => p.Photos)
            .Include(u => u.Recipient).ThenInclude(p => p.Photos)
            .Where(m =>
                m.RecipientUsername == currentUsername && 
                m.SenderUserName == recipientUsername &&
                !m.RecipientDeleted ||
                m.RecipientUsername == recipientUsername && 
                m.SenderUserName == currentUsername &&
                !m.SenderDeleted
            )
            .OrderBy(m => m.MessageSent)
            .ToListAsync();
        
        // Get all unread messages from other user to current user.
        // No need to query the DB as all the messages are already stored in memory.
        var unreadMessages = messages.Where(m => m.DateRead == null && 
            m.RecipientUsername == currentUsername).ToList();

        // Update the DateRead property for all unread messages, if any, of current user from other user.
        if (unreadMessages.Any())
        {
            foreach (var message in unreadMessages)
            {
                message.DateRead = DateTime.UtcNow;
            }

            // Update the DB.
            await this.context.SaveChangesAsync();
        }

        // Return the list of MessageDTOs.
        return this.mapper.Map<IEnumerable<MessageDTO>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await this.context.SaveChangesAsync() > 0;
    }
}
