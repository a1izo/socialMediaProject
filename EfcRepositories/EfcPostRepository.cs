using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace EfcRepositories;

public class EfcPostRepository : IPostRepository
{
    private readonly AppContext ctx;

    public EfcPostRepository(AppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Post> AddAsync(Post post)
    {
        var entry = await ctx.Posts.AddAsync(post);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task UpdateAsync(Post post)
    {
        if (!await ctx.Posts.AnyAsync(p => p.Id == post.Id))
        {
            throw new NotFoundException($"Post with ID '{post.Id}' not found");
        }

        // an entity loaded through this context is already tracked; only attach detached ones
        if (ctx.Entry(post).State == EntityState.Detached)
        {
            ctx.Posts.Update(post);
        }

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Post? existing = await ctx.Posts.SingleOrDefaultAsync(p => p.Id == id);
        if (existing is null)
        {
            throw new NotFoundException($"Post with ID '{id}' not found");
        }

        // comments, likes, tags, media and notifications go with it (cascade delete)
        ctx.Posts.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<Post> GetSingleAsync(int id)
    {
        Post? post = await ctx.Posts.SingleOrDefaultAsync(p => p.Id == id);
        if (post is null)
        {
            throw new NotFoundException($"Post with ID '{id}' not found");
        }

        return post;
    }

    public IQueryable<Post> GetMany()
    {
        return ctx.Posts.AsQueryable();
    }

    public async Task LikeAsync(int postId, int userId)
    {
        bool alreadyLiked = await ctx.PostLikes.AnyAsync(l => l.PostId == postId && l.UserId == userId);
        if (alreadyLiked)
        {
            throw new InvalidOperationException($"User '{userId}' already liked post '{postId}'");
        }

        await ctx.PostLikes.AddAsync(new PostLike
        {
            PostId = postId,
            UserId = userId,
            CreatedDate = DateTime.Now
        });
        await ctx.SaveChangesAsync();
    }

    public async Task UnlikeAsync(int postId, int userId)
    {
        PostLike? like = await ctx.PostLikes.SingleOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        if (like is null)
        {
            throw new InvalidOperationException($"User '{userId}' has not liked post '{postId}'");
        }

        ctx.PostLikes.Remove(like);
        await ctx.SaveChangesAsync();
    }

    public Task<int> GetLikeCountAsync(int postId)
    {
        return ctx.PostLikes.CountAsync(l => l.PostId == postId);
    }

    public IQueryable<int> GetLikerIds(int postId)
    {
        return ctx.PostLikes.Where(l => l.PostId == postId).Select(l => l.UserId);
    }

    public async Task AddTagAsync(int postId, int tagId)
    {
        if (!await ctx.Tags.AnyAsync(t => t.Id == tagId))
        {
            throw new NotFoundException($"Tag with ID '{tagId}' not found");
        }

        bool alreadyTagged = await ctx.PostTags.AnyAsync(pt => pt.PostId == postId && pt.TagId == tagId);
        if (alreadyTagged)
        {
            throw new InvalidOperationException($"Post '{postId}' already has tag '{tagId}'");
        }

        await ctx.PostTags.AddAsync(new PostTag { PostId = postId, TagId = tagId });
        await ctx.SaveChangesAsync();
    }

    public async Task RemoveTagAsync(int postId, int tagId)
    {
        PostTag? postTag = await ctx.PostTags.SingleOrDefaultAsync(pt => pt.PostId == postId && pt.TagId == tagId);
        if (postTag is null)
        {
            throw new InvalidOperationException($"Post '{postId}' does not have tag '{tagId}'");
        }

        ctx.PostTags.Remove(postTag);
        await ctx.SaveChangesAsync();
    }

    public IQueryable<Tag> GetTagsForPost(int postId)
    {
        return ctx.PostTags.Where(pt => pt.PostId == postId).Select(pt => pt.Tag);
    }

    public async Task AddMediaAsync(PostMedia media)
    {
        await ctx.PostMedia.AddAsync(media);
        await ctx.SaveChangesAsync();
    }

    public IQueryable<PostMedia> GetMedia(int postId)
    {
        return ctx.PostMedia.Where(m => m.PostId == postId).OrderBy(m => m.Order);
    }
}
