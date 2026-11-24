namespace Entities;

public class CommentLike
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CommentId { get; set; }
    public DateTime CreatedDate { get; set; }

    // navigation properties (EF Core)
    public User User { get; set; } = null!;
    public Comment Comment { get; set; } = null!;
}
