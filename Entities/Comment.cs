namespace Entities;

public class Comment
{
    public int Id { get; set; }
    public string Body { get; set; }
    public int UserId { get; set; }
    public int PostId { get; set; }
    public int? ParentCommentId { get; set; }
    public DateTime CreatedDate { get; set; }

    // navigation properties (EF Core)
    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
