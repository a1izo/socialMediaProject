namespace Entities;

public class Message
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string Body { get; set; }
    public string? MediaUrl { get; set; }
    public string? MediaKind { get; set; }
    public DateTime CreatedDate { get; set; }

    // navigation properties (EF Core)
    public Conversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
