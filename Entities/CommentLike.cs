namespace Entities;

public class CommentLike
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CommentId { get; set; }
    public DateTime CreatedDate { get; set; }
}