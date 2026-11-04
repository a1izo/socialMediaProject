namespace Entities;

public class PostMedia
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string Url { get; set; }
    // image | gif | video
    public string Kind { get; set; }
    public int Order { get; set; }
}
