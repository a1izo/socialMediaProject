using Entities;
using RepositoryContracts;

namespace WebAPI.Services;

// Creates notifications when people interact. Nobody is notified about their own actions,
// and repeated likes/follows from the same person do not stack up.
public class Notifier
{
    public const string Like = "like";
    public const string CommentLike = "commentLike";
    public const string Comment = "comment";
    public const string Reply = "reply";
    public const string Follow = "follow";

    private readonly INotificationRepository notificationRepo;

    public Notifier(INotificationRepository notificationRepo)
    {
        this.notificationRepo = notificationRepo;
    }

    public async Task NotifyAsync(int recipientId, int actorId, string type, int? postId = null, int? commentId = null)
    {
        if (recipientId == actorId)
        {
            return;
        }

        bool duplicate = type is Like or CommentLike or Follow && Find(recipientId, actorId, type, postId, commentId) is not null;
        if (duplicate)
        {
            return;
        }

        await notificationRepo.AddAsync(new Notification
        {
            UserId = recipientId,
            ActorId = actorId,
            Type = type,
            PostId = postId,
            CommentId = commentId,
            CreatedDate = DateTime.Now,
            IsRead = false
        });
    }

    // called on unlike / unfollow
    public async Task RetractAsync(int recipientId, int actorId, string type, int? postId = null, int? commentId = null)
    {
        Notification? existing = Find(recipientId, actorId, type, postId, commentId);
        if (existing is not null)
        {
            await notificationRepo.DeleteAsync(existing.Id);
        }
    }

    private Notification? Find(int recipientId, int actorId, string type, int? postId, int? commentId)
    {
        return notificationRepo.GetMany().FirstOrDefault(n =>
            n.UserId == recipientId && n.ActorId == actorId && n.Type == type &&
            n.PostId == postId && n.CommentId == commentId);
    }
}
