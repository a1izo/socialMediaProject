using Entities;

public class TagInMemoryRepository : ITagRepository
{
    private readonly List<Tag> tags = new();

    public Task<Tag> AddAsync(Tag tag)
    {
        tag.Id = tags.Any() ? tags.Max(t => t.Id) + 1 : 1;
        tags.Add(tag);
        return Task.FromResult(tag);
    }

    public Task UpdateAsync(Tag tag)
    {
        Tag? existingTag = tags.SingleOrDefault(t => t.Id == tag.Id);
        if (existingTag is null)
        {
            throw new InvalidOperationException(
                $"Tag with ID '{tag.Id}' not found");
        }

        tags.Remove(existingTag);
        tags.Add(tag);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Tag? tagToRemove = tags.SingleOrDefault(t => t.Id == id);
        if (tagToRemove is null)
        {
            throw new InvalidOperationException(
                $"Tag with ID '{id}' not found");
        }

        tags.Remove(tagToRemove);
        return Task.CompletedTask;
    }

    public Task<Tag> GetSingleAsync(int id)
    {
        Tag? tag = tags.SingleOrDefault(t => t.Id == id);
        if (tag is null)
        {
            throw new InvalidOperationException(
                $"Tag with ID '{id}' not found");
        }

        return Task.FromResult(tag);
    }

    public IQueryable<Tag> GetMany()
    {
        return tags.AsQueryable();
    }
}