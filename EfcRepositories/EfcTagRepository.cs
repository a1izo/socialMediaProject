using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace EfcRepositories;

public class EfcTagRepository : ITagRepository
{
    private readonly AppContext ctx;

    public EfcTagRepository(AppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Tag> AddAsync(Tag tag)
    {
        var entry = await ctx.Tags.AddAsync(tag);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task UpdateAsync(Tag tag)
    {
        if (!await ctx.Tags.AnyAsync(t => t.Id == tag.Id))
        {
            throw new NotFoundException($"Tag with ID '{tag.Id}' not found");
        }

        // an entity loaded through this context is already tracked; only attach detached ones
        if (ctx.Entry(tag).State == EntityState.Detached)
        {
            ctx.Tags.Update(tag);
        }

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Tag? existing = await ctx.Tags.SingleOrDefaultAsync(t => t.Id == id);
        if (existing is null)
        {
            throw new NotFoundException($"Tag with ID '{id}' not found");
        }

        ctx.Tags.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<Tag> GetSingleAsync(int id)
    {
        Tag? tag = await ctx.Tags.SingleOrDefaultAsync(t => t.Id == id);
        if (tag is null)
        {
            throw new NotFoundException($"Tag with ID '{id}' not found");
        }

        return tag;
    }

    public IQueryable<Tag> GetMany()
    {
        return ctx.Tags.AsQueryable();
    }
}
