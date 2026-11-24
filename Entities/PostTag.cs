namespace Entities;

public class PostTag
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int TagId { get; set; }

    // navigation properties (EF Core)
    public Post Post { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
