using Entities;

namespace RepositoryContracts;

public interface INotificationRepository
{
    Task<Notification> AddAsync(Notification notification);
    Task DeleteAsync(int id);
    IQueryable<Notification> GetMany();
    Task MarkAllReadAsync(int userId);
}
