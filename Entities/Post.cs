namespace Entities;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedDate { get; set; }

    // navigation properties (EF Core)
    public User User { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public ICollection<PostMedia> Media { get; set; } = new List<PostMedia>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
