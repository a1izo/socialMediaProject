using Entities;

public interface ICommentRepository
{
    Task<Comment> AddAsync(Comment comment);
    Task UpdateAsync(Comment comment);
    Task DeleteAsync(int id);
    Task<Comment> GetSingleAsync(int id);
    IQueryable<Comment> GetMany();
    
    //like logic
    Task LikeAsync(int commentId, int userId);
    Task UnlikeAsync(int commentId, int userId);
    Task<int> GetLikeCountAsync(int commentId);

    //replies
    IQueryable<Comment> GetReplies(int commentId);
}