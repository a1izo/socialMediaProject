using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace EfcRepositories;

public class EfcMessageRepository : IMessageRepository
{
    private readonly AppContext ctx;

    public EfcMessageRepository(AppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Conversation> AddConversationAsync(Conversation conversation)
    {
        var entry = await ctx.Conversations.AddAsync(conversation);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task UpdateConversationAsync(Conversation conversation)
    {
        if (!await ctx.Conversations.AnyAsync(c => c.Id == conversation.Id))
        {
            throw new NotFoundException($"Conversation with ID '{conversation.Id}' not found");
        }

        // an entity loaded through this context is already tracked; only attach detached ones
        if (ctx.Entry(conversation).State == EntityState.Detached)
        {
            ctx.Conversations.Update(conversation);
        }

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteConversationAsync(int id)
    {
        Conversation? existing = await ctx.Conversations.SingleOrDefaultAsync(c => c.Id == id);
        if (existing is null)
        {
            throw new NotFoundException($"Conversation with ID '{id}' not found");
        }

        // its messages go with it (cascade delete)
        ctx.Conversations.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<Conversation> GetConversationAsync(int id)
    {
        Conversation? conversation = await ctx.Conversations.SingleOrDefaultAsync(c => c.Id == id);
        if (conversation is null)
        {
            throw new NotFoundException($"Conversation with ID '{id}' not found");
        }

        return conversation;
    }

    public IQueryable<Conversation> GetConversations()
    {
        return ctx.Conversations.AsQueryable();
    }

    public async Task<Message> AddMessageAsync(Message message)
    {
        var entry = await ctx.Messages.AddAsync(message);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public IQueryable<Message> GetMessages(int conversationId)
    {
        return ctx.Messages.Where(m => m.ConversationId == conversationId).OrderBy(m => m.CreatedDate);
    }
}
