using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace EfcRepositories;

public class EfcProfileRepository : IProfileRepository
{
    private readonly AppContext ctx;

    public EfcProfileRepository(AppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Profile> AddAsync(Profile profile)
    {
        if (await ctx.Profiles.AnyAsync(p => p.UserId == profile.UserId))
        {
            throw new InvalidOperationException($"User '{profile.UserId}' already has a profile");
        }

        var entry = await ctx.Profiles.AddAsync(profile);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task UpdateAsync(Profile profile)
    {
        if (!await ctx.Profiles.AnyAsync(p => p.Id == profile.Id))
        {
            throw new NotFoundException($"Profile with ID '{profile.Id}' not found");
        }

        // an entity loaded through this context is already tracked; only attach detached ones
        if (ctx.Entry(profile).State == EntityState.Detached)
        {
            ctx.Profiles.Update(profile);
        }

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Profile? existing = await ctx.Profiles.SingleOrDefaultAsync(p => p.Id == id);
        if (existing is null)
        {
            throw new NotFoundException($"Profile with ID '{id}' not found");
        }

        List<ProfileModule> modules = await ctx.ProfileModules.Where(m => m.UserId == existing.UserId).ToListAsync();
        ctx.ProfileModules.RemoveRange(modules);
        ctx.Profiles.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<Profile> GetSingleAsync(int id)
    {
        Profile? profile = await ctx.Profiles.SingleOrDefaultAsync(p => p.Id == id);
        if (profile is null)
        {
            throw new NotFoundException($"Profile with ID '{id}' not found");
        }

        return profile;
    }

    public IQueryable<Profile> GetMany()
    {
        return ctx.Profiles.AsQueryable();
    }

    public IQueryable<ProfileModule> GetModules(int userId)
    {
        return ctx.ProfileModules.Where(m => m.UserId == userId).OrderBy(m => m.Order);
    }

    public async Task ReplaceModulesAsync(int userId, List<ProfileModule> modules)
    {
        List<ProfileModule> existing = await ctx.ProfileModules.Where(m => m.UserId == userId).ToListAsync();
        ctx.ProfileModules.RemoveRange(existing);

        foreach (ProfileModule module in modules)
        {
            module.Id = 0; // let the database assign a new id
            module.UserId = userId;
        }

        await ctx.ProfileModules.AddRangeAsync(modules);
        await ctx.SaveChangesAsync();
    }
}
