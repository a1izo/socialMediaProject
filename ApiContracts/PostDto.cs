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
    public int CommentCount { get; set; }
    public List<int> LikedByUserIds { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();
    public List<PostMediaDto> Media { get; set; } = new();

    public List<CommentDto>? Comments { get; set; }
}
