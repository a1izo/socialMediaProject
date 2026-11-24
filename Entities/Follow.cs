namespace Entities;

public class Follow
{
    public int Id { get; set; }
    public int FollowerId { get; set; }
    public int FollowingId { get; set; }
    public DateTime CreatedDate { get; set; }

    // navigation properties (EF Core)
    public User Follower { get; set; } = null!;
    public User Following { get; set; } = null!;
}
