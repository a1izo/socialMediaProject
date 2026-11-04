using Entities;

namespace RepositoryContracts;

public interface IMessageRepository
{
    Task<Conversation> AddConversationAsync(Conversation conversation);
    Task UpdateConversationAsync(Conversation conversation);
    Task DeleteConversationAsync(int id);
    Task<Conversation> GetConversationAsync(int id);
    IQueryable<Conversation> GetConversations();

    Task<Message> AddMessageAsync(Message message);
    IQueryable<Message> GetMessages(int conversationId);
}
