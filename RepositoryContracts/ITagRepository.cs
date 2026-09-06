using Entities;

public interface ITagRepository
{
    Task<Tag> AddAsync(Tag tag);
    Task UpdateAsync(Tag tag);
    Task DeleteAsync(int id);
    Task<Tag> GetSingleAsync(int id);
    IQueryable<Tag> GetMany();
}