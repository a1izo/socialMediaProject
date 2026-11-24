namespace Entities;

public class Notification
{
    public int Id { get; set; }
    // who receives it
    public int UserId { get; set; }
    // who caused it
    public int ActorId { get; set; }
    // like | commentLike | comment | reply | follow
    public string Type { get; set; }
    public int? PostId { get; set; }
    public int? CommentId { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsRead { get; set; }

    // navigation properties (EF Core)
    public User User { get; set; } = null!;
    public User Actor { get; set; } = null!;
    public Post? Post { get; set; }
    public Comment? Comment { get; set; }
}
