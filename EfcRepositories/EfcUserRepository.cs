using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace EfcRepositories;

public class EfcUserRepository : IUserRepository
{
    private readonly AppContext ctx;

    public EfcUserRepository(AppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<User> AddAsync(User user)
    {
        var entry = await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task UpdateAsync(User user)
    {
        if (!await ctx.Users.AnyAsync(u => u.Id == user.Id))
        {
            throw new NotFoundException($"User with ID '{user.Id}' not found");
        }

        // an entity loaded through this context is already tracked; only attach detached ones
        if (ctx.Entry(user).State == EntityState.Detached)
        {
            ctx.Users.Update(user);
        }

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        User? existing = await ctx.Users.SingleOrDefaultAsync(u => u.Id == id);
        if (existing is null)
        {
            throw new NotFoundException($"User with ID '{id}' not found");
        }

        ctx.Users.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<User> GetSingleAsync(int id)
    {
        User? user = await ctx.Users.SingleOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            throw new NotFoundException($"User with ID '{id}' not found");
        }

        return user;
    }

    public IQueryable<User> GetMany()
    {
        return ctx.Users.AsQueryable();
    }

    public async Task FollowAsync(int followerId, int followingId)
    {
        if (followerId == followingId)
        {
            throw new InvalidOperationException("A user cannot follow themselves");
        }

        bool alreadyFollowing = await ctx.Follows
            .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        if (alreadyFollowing)
        {
            throw new InvalidOperationException(
                $"User '{followerId}' already follows user '{followingId}'");
        }

        await ctx.Follows.AddAsync(new Follow
        {
            FollowerId = followerId,
            FollowingId = followingId,
            CreatedDate = DateTime.Now
        });
        await ctx.SaveChangesAsync();
    }

    public async Task UnfollowAsync(int followerId, int followingId)
    {
        Follow? follow = await ctx.Follows
            .SingleOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        if (follow is null)
        {
            throw new InvalidOperationException(
                $"User '{followerId}' does not follow user '{followingId}'");
        }

        ctx.Follows.Remove(follow);
        await ctx.SaveChangesAsync();
    }

    public IQueryable<User> GetFollowers(int userId)
    {
        return ctx.Follows.Where(f => f.FollowingId == userId).Select(f => f.Follower);
    }

    public IQueryable<User> GetFollowing(int userId)
    {
        return ctx.Follows.Where(f => f.FollowerId == userId).Select(f => f.Following);
    }
}
