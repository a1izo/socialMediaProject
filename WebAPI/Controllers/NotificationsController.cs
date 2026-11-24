using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("users/{userId:int}")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository notificationRepo;
    private readonly IMessageRepository messageRepo;

    public NotificationsController(INotificationRepository notificationRepo, IMessageRepository messageRepo)
    {
        this.notificationRepo = notificationRepo;
        this.messageRepo = messageRepo;
    }

    [HttpGet("notifications")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications([FromRoute] int userId, [FromQuery] int? take)
    {
        // one query: the actor, post and comment come along through the navigation properties
        var rows = await notificationRepo.GetMany()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .Take(take ?? 100)
            .Select(n => new
            {
                Notification = n,
                ActorUserName = n.Actor.UserName,
                PostTitle = n.Post != null ? n.Post.Title : null,
                PostBody = n.Post != null ? n.Post.Body : null,
                CommentBody = n.Comment != null ? n.Comment.Body : null
            })
            .ToListAsync();

        List<NotificationDto> dtos = rows.Select(r => new NotificationDto
        {
            Id = r.Notification.Id,
            Type = r.Notification.Type,
            ActorId = r.Notification.ActorId,
            ActorUserName = r.ActorUserName,
            PostId = r.Notification.PostId,
            PostTitle = r.PostBody is null ? null : Summary(r.PostTitle, r.PostBody),
            CommentId = r.Notification.CommentId,
            CommentExcerpt = r.CommentBody is null ? null : Excerpt(r.CommentBody),
            CreatedDate = r.Notification.CreatedDate,
            IsRead = r.Notification.IsRead
        }).ToList();
        return Ok(dtos);
    }

    [HttpPost("notifications/read")]
    public async Task<ActionResult> MarkAllRead([FromRoute] int userId)
    {
        await notificationRepo.MarkAllReadAsync(userId);
        return NoContent();
    }

    [HttpGet("unread")]
    public async Task<ActionResult<UnreadCountsDto>> GetUnreadCounts([FromRoute] int userId)
    {
        int unreadMessages = 0;
        int requests = 0;
        List<Conversation> conversations = await messageRepo.GetConversations()
            .Where(c => c.UserOneId == userId || c.UserTwoId == userId)
            .ToListAsync();
        foreach (Conversation conversation in conversations)
        {
            if (!conversation.IsAccepted && conversation.StartedById != userId)
            {
                requests++;
                continue;
            }

            DateTime? lastRead = conversation.UserOneId == userId ? conversation.UserOneLastRead : conversation.UserTwoLastRead;
            unreadMessages += await messageRepo.GetMessages(conversation.Id)
                .CountAsync(m => m.SenderId != userId && (lastRead == null || m.CreatedDate > lastRead));
        }

        return Ok(new UnreadCountsDto
        {
            Notifications = await notificationRepo.GetMany().CountAsync(n => n.UserId == userId && !n.IsRead),
            Messages = unreadMessages,
            Requests = requests
        });
    }

    private static string Summary(string? title, string body)
    {
        return string.IsNullOrWhiteSpace(title) ? Excerpt(body) : title;
    }

    private static string Excerpt(string text)
    {
        string flat = text.Replace('\n', ' ').Trim();
        return flat.Length <= 80 ? flat : flat[..80] + "…";
    }
}
