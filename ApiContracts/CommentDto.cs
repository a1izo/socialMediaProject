namespace ApiContracts;

public class CommentDto
{
    public int Id { get; set; }
    public required string Body { get; set; }
    public int UserId { get; set; }
    public required string AuthorUserName { get; set; }
    public int PostId { get; set; }
    public int? ParentCommentId { get; set; }
    public DateTime CreatedDate { get; set; }
    public int LikeCount { get; set; }
}
