namespace ApiContracts;

public class UploadResultDto
{
    public required string Url { get; set; }
    // image | gif | video
    public required string Kind { get; set; }
}
