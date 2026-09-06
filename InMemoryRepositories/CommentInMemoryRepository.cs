using Entities;

public class CommentInMemoryRepository : ICommentRepository
{
    private readonly List<Comment> comments = new();
    private readonly List<CommentLike> commentLikes = new();

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