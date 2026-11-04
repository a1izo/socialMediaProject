namespace ApiContracts;

public class MessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public required string Body { get; set; }
    public string? MediaUrl { get; set; }
    public string? MediaKind { get; set; }
    public DateTime CreatedDate { get; set; }
}
