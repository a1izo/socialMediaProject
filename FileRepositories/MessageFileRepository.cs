using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class MessageFileRepository : IMessageRepository
{
    private readonly string conversationsFilePath = "conversations.json";
    private readonly string messagesFilePath = "messages.json";

    public MessageFileRepository()
    {
        JsonFileHelper.EnsureFileExists(conversationsFilePath);
        JsonFileHelper.EnsureFileExists(messagesFilePath);
    }

    public async Task<Conversation> AddConversationAsync(Conversation conversation)
    {
        List<Conversation> conversations = await JsonFileHelper.LoadAsync<Conversation>(conversationsFilePath);
        conversation.Id = conversations.Any() ? conversations.Max(c => c.Id) + 1 : 1;
        conversations.Add(conversation);
        await JsonFileHelper.SaveAsync(conversationsFilePath, conversations);
        return conversation;
    }

    public async Task UpdateConversationAsync(Conversation conversation)
    {
        List<Conversation> conversations = await JsonFileHelper.LoadAsync<Conversation>(conversationsFilePath);
        Conversation? existing = conversations.SingleOrDefault(c => c.Id == conversation.Id);
        if (existing is null)
        {
            throw new NotFoundException(
                $"Conversation with ID '{conversation.Id}' not found");
        }

        conversations.Remove(existing);
        conversations.Add(conversation);
        await JsonFileHelper.SaveAsync(conversationsFilePath, conversations);
    }

    public async Task DeleteConversationAsync(int id)
    {
        List<Conversation> conversations = await JsonFileHelper.LoadAsync<Conversation>(conversationsFilePath);
        Conversation? existing = conversations.SingleOrDefault(c => c.Id == id);
        if (existing is null)
        {
            throw new NotFoundException(
                $"Conversation with ID '{id}' not found");
        }

        conversations.Remove(existing);
        await JsonFileHelper.SaveAsync(conversationsFilePath, conversations);

        List<Message> messages = await JsonFileHelper.LoadAsync<Message>(messagesFilePath);
        messages.RemoveAll(m => m.ConversationId == id);
        await JsonFileHelper.SaveAsync(messagesFilePath, messages);
    }

    public async Task<Conversation> GetConversationAsync(int id)
    {
        List<Conversation> conversations = await JsonFileHelper.LoadAsync<Conversation>(conversationsFilePath);
        Conversation? conversation = conversations.SingleOrDefault(c => c.Id == id);
        if (conversation is null)
        {
            throw new NotFoundException(
                $"Conversation with ID '{id}' not found");
        }

        return conversation;
    }

    public IQueryable<Conversation> GetConversations()
    {
        List<Conversation> conversations = JsonFileHelper.Load<Conversation>(conversationsFilePath);
        return conversations.AsQueryable();
    }

    public async Task<Message> AddMessageAsync(Message message)
    {
        List<Message> messages = await JsonFileHelper.LoadAsync<Message>(messagesFilePath);
        message.Id = messages.Any() ? messages.Max(m => m.Id) + 1 : 1;
        messages.Add(message);
        await JsonFileHelper.SaveAsync(messagesFilePath, messages);
        return message;
    }

    public IQueryable<Message> GetMessages(int conversationId)
    {
        List<Message> messages = JsonFileHelper.Load<Message>(messagesFilePath);
        return messages.Where(m => m.ConversationId == conversationId).OrderBy(m => m.CreatedDate).AsQueryable();
    }
}
