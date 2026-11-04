namespace ApiContracts;

public class ConversationDto
{
    public int Id { get; set; }
    public int OtherUserId { get; set; }
    public required string OtherUserName { get; set; }
    // active: both can talk. request: someone wants to message you. pending: you are waiting for them to accept
    public required string Status { get; set; }
    public string? LastMessage { get; set; }
    public DateTime LastMessageDate { get; set; }
    public int UnreadCount { get; set; }
}
