using ApiContracts;

namespace BlazorApp.Services;

public class HttpMessageService : IMessageService
{
    private readonly HttpClient client;

    public HttpMessageService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(int userId)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"conversations?userId={userId}");
        return await HttpResponseReader.ReadAsync<List<ConversationDto>>(httpResponse);
    }

    public async Task<ConversationDto> StartConversationAsync(int fromUserId, int toUserId)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync("conversations",
            new StartConversationDto { FromUserId = fromUserId, ToUserId = toUserId });
        return await HttpResponseReader.ReadAsync<ConversationDto>(httpResponse);
    }

    public async Task<ConversationDto> GetConversationAsync(int id, int userId)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"conversations/{id}?userId={userId}");
        return await HttpResponseReader.ReadAsync<ConversationDto>(httpResponse);
    }

    public async Task<List<MessageDto>> GetMessagesAsync(int id, int userId)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"conversations/{id}/messages?userId={userId}");
        return await HttpResponseReader.ReadAsync<List<MessageDto>>(httpResponse);
    }

    public async Task<MessageDto> SendMessageAsync(int id, SendMessageDto request)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync($"conversations/{id}/messages", request);
        return await HttpResponseReader.ReadAsync<MessageDto>(httpResponse);
    }

    public async Task AcceptRequestAsync(int id, int userId)
    {
        HttpResponseMessage httpResponse = await client.PostAsync($"conversations/{id}/accept?userId={userId}", null);
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task DeleteConversationAsync(int id, int userId)
    {
        HttpResponseMessage httpResponse = await client.DeleteAsync($"conversations/{id}?userId={userId}");
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }
}
