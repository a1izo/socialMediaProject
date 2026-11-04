namespace ApiContracts;

// A post needs at least one of: a title, a body or an attachment.
public class CreatePostDto
{
    public string? Title { get; set; }
    public string? Body { get; set; }
    public int UserId { get; set; }
    public List<PostMediaDto> Media { get; set; } = new();
}
