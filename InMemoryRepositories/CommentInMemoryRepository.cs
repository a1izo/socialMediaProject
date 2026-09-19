using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class CommentInMemoryRepository : ICommentRepository
{
    private readonly List<Comment> comments = new();
    private readonly List<CommentLike> commentLikes = new();

    public CommentInMemoryRepository()
    {
        SeedData();
    }

    private void SeedData()
    {
        comments.AddRange(new[]
        {
            new Comment { Id = 1, Body = "finally a place for my memes", UserId = 2, PostId = 1, ParentCommentId = null, CreatedDate = DateTime.Now.AddDays(-14) },
            new Comment { Id = 2, Body = "your memes are the reason we needed a new place", UserId = 3, PostId = 1, ParentCommentId = 1, CreatedDate = DateTime.Now.AddDays(-14) },
            new Comment { Id = 3, Body = "soup with commitment issues", UserId = 4, PostId = 2, ParentCommentId = null, CreatedDate = DateTime.Now.AddDays(-11) },
            new Comment { Id = 4, Body = "go to bed mike", UserId = 1, PostId = 3, ParentCommentId = null, CreatedDate = DateTime.Now.AddDays(-7) },
            new Comment { Id = 5, Body = "the Corolla survives everything, that is the lore", UserId = 2, PostId = 4, ParentCommentId = null, CreatedDate = DateTime.Now.AddDays(-4) },
            new Comment { Id = 6, Body = "the zeppelin has a better lawyer though", UserId = 3, PostId = 4, ParentCommentId = 5, CreatedDate = DateTime.Now.AddDays(-4) },
            new Comment { Id = 7, Body = "we are all going to be fine, trust me", UserId = 4, PostId = 5, ParentCommentId = null, CreatedDate = DateTime.Now.AddDays(-2) }
        });

        commentLikes.AddRange(new[]
        {
            new CommentLike { Id = 1, UserId = 1, CommentId = 1, CreatedDate = DateTime.Now.AddDays(-13) },
            new CommentLike { Id = 2, UserId = 2, CommentId = 2, CreatedDate = DateTime.Now.AddDays(-13) },
            new CommentLike { Id = 3, UserId = 3, CommentId = 3, CreatedDate = DateTime.Now.AddDays(-10) },
            new CommentLike { Id = 4, UserId = 4, CommentId = 6, CreatedDate = DateTime.Now.AddDays(-3) }
        });
    }

    public Task<Comment> AddAsync(Comment comment)
    {
        comment.Id = comments.Any() ? comments.Max(c => c.Id) + 1 : 1;
        comments.Add(comment);
        return Task.FromResult(comment);
    }

    public Task UpdateAsync(Comment comment)
    {
        Comment? existingComment = comments.SingleOrDefault(c => c.Id == comment.Id);
        if (existingComment is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{comment.Id}' not found");
        }

        comments.Remove(existingComment);
        comments.Add(comment);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Comment? commentToRemove = comments.SingleOrDefault(c => c.Id == id);
        if (commentToRemove is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{id}' not found");
        }

        comments.Remove(commentToRemove);
        return Task.CompletedTask;
    }

    public Task<Comment> GetSingleAsync(int id)
    {
        Comment? comment = comments.SingleOrDefault(c => c.Id == id);
        if (comment is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{id}' not found");
        }

        return Task.FromResult(comment);
    }

    public IQueryable<Comment> GetMany()
    {
        return comments.AsQueryable();
    }

    public Task LikeAsync(int commentId, int userId)
    {
        bool alreadyLiked = commentLikes.Any(cl => cl.CommentId == commentId && cl.UserId == userId);
        if (alreadyLiked)
        {
            throw new InvalidOperationException(
                $"User '{userId}' already liked comment '{commentId}'");
        }

        CommentLike like = new()
        {
            Id = commentLikes.Any() ? commentLikes.Max(cl => cl.Id) + 1 : 1,
            CommentId = commentId,
            UserId = userId,
            CreatedDate = DateTime.Now
        };
        commentLikes.Add(like);

        return Task.CompletedTask;
    }

    public Task UnlikeAsync(int commentId, int userId)
    {
        CommentLike? like = commentLikes.SingleOrDefault(cl => cl.CommentId == commentId && cl.UserId == userId);
        if (like is null)
        {
            throw new InvalidOperationException(
                $"User '{userId}' has not liked comment '{commentId}'");
        }

        commentLikes.Remove(like);
        return Task.CompletedTask;
    }

    public Task<int> GetLikeCountAsync(int commentId)
    {
        int count = commentLikes.Count(cl => cl.CommentId == commentId);
        return Task.FromResult(count);
    }

    public IQueryable<Comment> GetReplies(int commentId)
    {
        return comments.Where(c => c.ParentCommentId == commentId).AsQueryable();
    }
}