namespace Entities;

public class PostLike
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PostId { get; set; }
    public DateTime CreatedDate { get; set; }

    // navigation properties (EF Core)
    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}
