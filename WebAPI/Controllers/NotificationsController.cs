using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("users/{userId:int}")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository notificationRepo;
    private readonly IMessageRepository messageRepo;
    private readonly IUserRepository userRepo;
    private readonly IPostRepository postRepo;
    private readonly ICommentRepository commentRepo;

    public NotificationsController(INotificationRepository notificationRepo, IMessageRepository messageRepo,
        IUserRepository userRepo, IPostRepository postRepo, ICommentRepository commentRepo)
    {
        this.notificationRepo = notificationRepo;
        this.messageRepo = messageRepo;
        this.userRepo = userRepo;
        this.postRepo = postRepo;
        this.commentRepo = commentRepo;
    }

    [HttpGet("notifications")]
    public ActionResult<IEnumerable<NotificationDto>> GetNotifications([FromRoute] int userId, [FromQuery] int? take)
    {
        Dictionary<int, string> userNames = userRepo.GetMany().ToDictionary(u => u.Id, u => u.UserName);
        Dictionary<int, Post> posts = postRepo.GetMany().ToDictionary(p => p.Id);
        Dictionary<int, Comment> comments = commentRepo.GetMany().ToDictionary(c => c.Id);

        List<NotificationDto> dtos = notificationRepo.GetMany()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .Take(take ?? 100)
            .ToList()
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                ActorId = n.ActorId,
                ActorUserName = userNames.GetValueOrDefault(n.ActorId, DtoMapper.DeletedUserName),
                PostId = n.PostId,
                PostTitle = n.PostId is not null && posts.TryGetValue(n.PostId.Value, out Post? post) ? Summary(post) : null,
                CommentId = n.CommentId,
                CommentExcerpt = n.CommentId is not null && comments.TryGetValue(n.CommentId.Value, out Comment? comment)
                    ? Excerpt(comment.Body)
                    : null,
                CreatedDate = n.CreatedDate,
                IsRead = n.IsRead
            })
            .ToList();
        return Ok(dtos);
    }

    [HttpPost("notifications/read")]
    public async Task<ActionResult> MarkAllRead([FromRoute] int userId)
    {
        await notificationRepo.MarkAllReadAsync(userId);
        return NoContent();
    }

    [HttpGet("unread")]
    public ActionResult<UnreadCountsDto> GetUnreadCounts([FromRoute] int userId)
    {
        int unreadMessages = 0;
        int requests = 0;
        foreach (Conversation conversation in messageRepo.GetConversations()
                     .Where(c => c.UserOneId == userId || c.UserTwoId == userId).ToList())
        {
            if (!conversation.IsAccepted && conversation.StartedById != userId)
            {
                requests++;
                continue;
            }

            DateTime? lastRead = conversation.UserOneId == userId ? conversation.UserOneLastRead : conversation.UserTwoLastRead;
            unreadMessages += messageRepo.GetMessages(conversation.Id)
                .Count(m => m.SenderId != userId && (lastRead == null || m.CreatedDate > lastRead));
        }

        return Ok(new UnreadCountsDto
        {
            Notifications = notificationRepo.GetMany().Count(n => n.UserId == userId && !n.IsRead),
            Messages = unreadMessages,
            Requests = requests
        });
    }

    private static string Summary(Post post)
    {
        return string.IsNullOrWhiteSpace(post.Title) ? Excerpt(post.Body) : post.Title;
    }

    private static string Excerpt(string text)
    {
        string flat = text.Replace('\n', ' ').Trim();
        return flat.Length <= 80 ? flat : flat[..80] + "…";
    }
}
