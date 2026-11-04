namespace ApiContracts;

public class SendMessageDto
{
    public int SenderId { get; set; }
    public string? Body { get; set; }
    public string? MediaUrl { get; set; }
    public string? MediaKind { get; set; }
}
