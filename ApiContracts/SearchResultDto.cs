namespace ApiContracts;

public class SearchResultDto
{
    public List<ProfileCardDto> Users { get; set; } = new();
    public List<PostDto> Posts { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();
}
