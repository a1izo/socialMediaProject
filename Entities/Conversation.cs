namespace Entities;

// A direct message thread between two users.
public class Conversation
{
    public int Id { get; set; }
    public int UserOneId { get; set; }
    public int UserTwoId { get; set; }
    public int StartedById { get; set; }
    // false while it is a message request the other user has not accepted yet
    public bool IsAccepted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastMessageDate { get; set; }
    public DateTime? UserOneLastRead { get; set; }
    public DateTime? UserTwoLastRead { get; set; }

    // navigation properties (EF Core)
    public User UserOne { get; set; } = null!;
    public User UserTwo { get; set; } = null!;
    public User StartedBy { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
