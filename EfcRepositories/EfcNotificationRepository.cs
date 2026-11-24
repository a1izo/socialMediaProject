using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace EfcRepositories;

public class EfcNotificationRepository : INotificationRepository
{
    private readonly AppContext ctx;

    public EfcNotificationRepository(AppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Notification> AddAsync(Notification notification)
    {
        var entry = await ctx.Notifications.AddAsync(notification);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task DeleteAsync(int id)
    {
        Notification? existing = await ctx.Notifications.SingleOrDefaultAsync(n => n.Id == id);
        if (existing is null)
        {
            throw new NotFoundException($"Notification with ID '{id}' not found");
        }

        ctx.Notifications.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public IQueryable<Notification> GetMany()
    {
        return ctx.Notifications.AsQueryable();
    }

    public async Task MarkAllReadAsync(int userId)
    {
        List<Notification> unread = await ctx.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
        foreach (Notification notification in unread)
        {
            notification.IsRead = true;
        }

        await ctx.SaveChangesAsync();
    }
}
