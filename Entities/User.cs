namespace Entities;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsModerator { get; set; }
    public DateTime CreatedDate { get; set; }
    // everyone | requests | following | nobody
    public string? MessagePrivacy { get; set; }

    // navigation properties (EF Core)
    public Profile? Profile { get; set; }
    public ICollection<ProfileModule> ProfileModules { get; set; } = new List<ProfileModule>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
    // follows where this user is the one being followed
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    // follows where this user is the follower
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Notification> CausedNotifications { get; set; } = new List<Notification>();
    public ICollection<Conversation> ConversationsAsUserOne { get; set; } = new List<Conversation>();
    public ICollection<Conversation> ConversationsAsUserTwo { get; set; } = new List<Conversation>();
    public ICollection<Conversation> StartedConversations { get; set; } = new List<Conversation>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
}
