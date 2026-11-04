namespace ApiContracts;

public class NotificationDto
{
    public int Id { get; set; }
    // like | commentLike | comment | reply | follow
    public required string Type { get; set; }
    public int ActorId { get; set; }
    public required string ActorUserName { get; set; }
    public int? PostId { get; set; }
    public string? PostTitle { get; set; }
    public int? CommentId { get; set; }
    public string? CommentExcerpt { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsRead { get; set; }
}
