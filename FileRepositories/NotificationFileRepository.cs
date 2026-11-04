using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class NotificationFileRepository : INotificationRepository
{
    private readonly string notificationsFilePath = "notifications.json";

    public NotificationFileRepository()
    {
        JsonFileHelper.EnsureFileExists(notificationsFilePath);
    }

    public async Task<Notification> AddAsync(Notification notification)
    {
        List<Notification> notifications = await JsonFileHelper.LoadAsync<Notification>(notificationsFilePath);
        notification.Id = notifications.Any() ? notifications.Max(n => n.Id) + 1 : 1;
        notifications.Add(notification);
        await JsonFileHelper.SaveAsync(notificationsFilePath, notifications);
        return notification;
    }

    public async Task DeleteAsync(int id)
    {
        List<Notification> notifications = await JsonFileHelper.LoadAsync<Notification>(notificationsFilePath);
        Notification? notification = notifications.SingleOrDefault(n => n.Id == id);
        if (notification is null)
        {
            throw new NotFoundException(
                $"Notification with ID '{id}' not found");
        }

        notifications.Remove(notification);
        await JsonFileHelper.SaveAsync(notificationsFilePath, notifications);
    }

    public IQueryable<Notification> GetMany()
    {
        List<Notification> notifications = JsonFileHelper.Load<Notification>(notificationsFilePath);
        return notifications.AsQueryable();
    }

    public async Task MarkAllReadAsync(int userId)
    {
        List<Notification> notifications = await JsonFileHelper.LoadAsync<Notification>(notificationsFilePath);
        foreach (Notification notification in notifications.Where(n => n.UserId == userId))
        {
            notification.IsRead = true;
        }

        await JsonFileHelper.SaveAsync(notificationsFilePath, notifications);
    }
}
