using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class CommentFileRepository : ICommentRepository
{
    private readonly string commentsFilePath = "comments.json";
    private readonly string commentLikesFilePath = "commentLikes.json";

    public CommentFileRepository()
    {
        JsonFileHelper.EnsureFileExists(commentsFilePath);
        JsonFileHelper.EnsureFileExists(commentLikesFilePath);
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        List<Comment> comments = await JsonFileHelper.LoadAsync<Comment>(commentsFilePath);
        comment.Id = comments.Any() ? comments.Max(c => c.Id) + 1 : 1;
        comments.Add(comment);
        await JsonFileHelper.SaveAsync(commentsFilePath, comments);
        return comment;
    }

    public async Task UpdateAsync(Comment comment)
    {
        List<Comment> comments = await JsonFileHelper.LoadAsync<Comment>(commentsFilePath);
        Comment? existingComment = comments.SingleOrDefault(c => c.Id == comment.Id);
        if (existingComment is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{comment.Id}' not found");
        }

        comments.Remove(existingComment);
        comments.Add(comment);
        await JsonFileHelper.SaveAsync(commentsFilePath, comments);
    }

    public async Task DeleteAsync(int id)
    {
        List<Comment> comments = await JsonFileHelper.LoadAsync<Comment>(commentsFilePath);
        Comment? commentToRemove = comments.SingleOrDefault(c => c.Id == id);
        if (commentToRemove is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{id}' not found");
        }

        comments.Remove(commentToRemove);
        await JsonFileHelper.SaveAsync(commentsFilePath, comments);
    }

    public async Task<Comment> GetSingleAsync(int id)
    {
        List<Comment> comments = await JsonFileHelper.LoadAsync<Comment>(commentsFilePath);
        Comment? comment = comments.SingleOrDefault(c => c.Id == id);
        if (comment is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{id}' not found");
        }

        return comment;
    }

    public IQueryable<Comment> GetMany()
    {
        List<Comment> comments = JsonFileHelper.Load<Comment>(commentsFilePath);
        return comments.AsQueryable();
    }

    public async Task LikeAsync(int commentId, int userId)
    {
        List<CommentLike> commentLikes = await JsonFileHelper.LoadAsync<CommentLike>(commentLikesFilePath);
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
        await JsonFileHelper.SaveAsync(commentLikesFilePath, commentLikes);
    }

    public async Task UnlikeAsync(int commentId, int userId)
    {
        List<CommentLike> commentLikes = await JsonFileHelper.LoadAsync<CommentLike>(commentLikesFilePath);
        CommentLike? like = commentLikes.SingleOrDefault(cl => cl.CommentId == commentId && cl.UserId == userId);
        if (like is null)
        {
            throw new InvalidOperationException(
                $"User '{userId}' has not liked comment '{commentId}'");
        }

        commentLikes.Remove(like);
        await JsonFileHelper.SaveAsync(commentLikesFilePath, commentLikes);
    }

    public async Task<int> GetLikeCountAsync(int commentId)
    {
        List<CommentLike> commentLikes = await JsonFileHelper.LoadAsync<CommentLike>(commentLikesFilePath);
        return commentLikes.Count(cl => cl.CommentId == commentId);
    }

    public IQueryable<Comment> GetReplies(int commentId)
    {
        List<Comment> comments = JsonFileHelper.Load<Comment>(commentsFilePath);
        return comments.Where(c => c.ParentCommentId == commentId).AsQueryable();
    }
}
