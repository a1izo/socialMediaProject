using ApiContracts;

namespace BlazorApp.Services;

public interface INotificationService
{
    public Task<List<NotificationDto>> GetNotificationsAsync(int userId);
    public Task MarkAllReadAsync(int userId);
    public Task<UnreadCountsDto> GetUnreadCountsAsync(int userId);
}
