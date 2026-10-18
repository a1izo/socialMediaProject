namespace ApiContracts;

public class PostDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public int UserId { get; set; }
    public required string AuthorUserName { get; set; }
    public DateTime CreatedDate { get; set; }
    public int LikeCount { get; set; }

    public List<CommentDto>? Comments { get; set; }
}
