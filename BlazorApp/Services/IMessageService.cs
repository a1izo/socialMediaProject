using ApiContracts;

namespace BlazorApp.Services;

public interface IMessageService
{
    public Task<List<ConversationDto>> GetConversationsAsync(int userId);
    public Task<ConversationDto> StartConversationAsync(int fromUserId, int toUserId);
    public Task<ConversationDto> GetConversationAsync(int id, int userId);
    public Task<List<MessageDto>> GetMessagesAsync(int id, int userId);
    public Task<MessageDto> SendMessageAsync(int id, SendMessageDto request);
    public Task AcceptRequestAsync(int id, int userId);
    public Task DeleteConversationAsync(int id, int userId);
}
