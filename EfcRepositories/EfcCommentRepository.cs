using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace EfcRepositories;

public class EfcCommentRepository : ICommentRepository
{
    private readonly AppContext ctx;

    public EfcCommentRepository(AppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        var entry = await ctx.Comments.AddAsync(comment);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task UpdateAsync(Comment comment)
    {
        if (!await ctx.Comments.AnyAsync(c => c.Id == comment.Id))
        {
            throw new NotFoundException($"Comment with ID '{comment.Id}' not found");
        }

        // an entity loaded through this context is already tracked; only attach detached ones
        if (ctx.Entry(comment).State == EntityState.Detached)
        {
            ctx.Comments.Update(comment);
        }

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Comment? existing = await ctx.Comments.SingleOrDefaultAsync(c => c.Id == id);
        if (existing is null)
        {
            throw new NotFoundException($"Comment with ID '{id}' not found");
        }

        // replies, likes and notifications go with it (cascade delete)
        ctx.Comments.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<Comment> GetSingleAsync(int id)
    {
        Comment? comment = await ctx.Comments.SingleOrDefaultAsync(c => c.Id == id);
        if (comment is null)
        {
            throw new NotFoundException($"Comment with ID '{id}' not found");
        }

        return comment;
    }

    public IQueryable<Comment> GetMany()
    {
        return ctx.Comments.AsQueryable();
    }

    public async Task LikeAsync(int commentId, int userId)
    {
        bool alreadyLiked = await ctx.CommentLikes.AnyAsync(l => l.CommentId == commentId && l.UserId == userId);
        if (alreadyLiked)
        {
            throw new InvalidOperationException($"User '{userId}' already liked comment '{commentId}'");
        }

        await ctx.CommentLikes.AddAsync(new CommentLike
        {
            CommentId = commentId,
            UserId = userId,
            CreatedDate = DateTime.Now
        });
        await ctx.SaveChangesAsync();
    }

    public async Task UnlikeAsync(int commentId, int userId)
    {
        CommentLike? like = await ctx.CommentLikes
            .SingleOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);
        if (like is null)
        {
            throw new InvalidOperationException($"User '{userId}' has not liked comment '{commentId}'");
        }

        ctx.CommentLikes.Remove(like);
        await ctx.SaveChangesAsync();
    }

    public IQueryable<Comment> GetReplies(int commentId)
    {
        return ctx.Comments.Where(c => c.ParentCommentId == commentId);
    }
}
