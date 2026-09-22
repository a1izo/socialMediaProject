using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class TagFileRepository : ITagRepository
{
    private readonly string filePath = "tags.json";

    public TagFileRepository()
    {
        JsonFileHelper.EnsureFileExists(filePath);
    }

    public async Task<Tag> AddAsync(Tag tag)
    {
        List<Tag> tags = await JsonFileHelper.LoadAsync<Tag>(filePath);
        tag.Id = tags.Any() ? tags.Max(t => t.Id) + 1 : 1;
        tags.Add(tag);
        await JsonFileHelper.SaveAsync(filePath, tags);
        return tag;
    }

    public async Task UpdateAsync(Tag tag)
    {
        List<Tag> tags = await JsonFileHelper.LoadAsync<Tag>(filePath);
        Tag? existingTag = tags.SingleOrDefault(t => t.Id == tag.Id);
        if (existingTag is null)
        {
            throw new InvalidOperationException(
                $"Tag with ID '{tag.Id}' not found");
        }

        tags.Remove(existingTag);
        tags.Add(tag);
        await JsonFileHelper.SaveAsync(filePath, tags);
    }

    public async Task DeleteAsync(int id)
    {
        List<Tag> tags = await JsonFileHelper.LoadAsync<Tag>(filePath);
        Tag? tagToRemove = tags.SingleOrDefault(t => t.Id == id);
        if (tagToRemove is null)
        {
            throw new InvalidOperationException(
                $"Tag with ID '{id}' not found");
        }

        tags.Remove(tagToRemove);
        await JsonFileHelper.SaveAsync(filePath, tags);
    }

    public async Task<Tag> GetSingleAsync(int id)
    {
        List<Tag> tags = await JsonFileHelper.LoadAsync<Tag>(filePath);
        Tag? tag = tags.SingleOrDefault(t => t.Id == id);
        if (tag is null)
        {
            throw new InvalidOperationException(
                $"Tag with ID '{id}' not found");
        }

        return tag;
    }

    public IQueryable<Tag> GetMany()
    {
        List<Tag> tags = JsonFileHelper.Load<Tag>(filePath);
        return tags.AsQueryable();
    }
}
