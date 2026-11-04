using ApiContracts;

namespace BlazorApp.Services;

public class HttpNotificationService : INotificationService
{
    private readonly HttpClient client;

    public HttpNotificationService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(int userId)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"users/{userId}/notifications");
        return await HttpResponseReader.ReadAsync<List<NotificationDto>>(httpResponse);
    }

    public async Task MarkAllReadAsync(int userId)
    {
        HttpResponseMessage httpResponse = await client.PostAsync($"users/{userId}/notifications/read", null);
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task<UnreadCountsDto> GetUnreadCountsAsync(int userId)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"users/{userId}/unread");
        return await HttpResponseReader.ReadAsync<UnreadCountsDto>(httpResponse);
    }
}
