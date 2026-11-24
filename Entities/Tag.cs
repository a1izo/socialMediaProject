namespace Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }

    // navigation properties (EF Core)
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}
