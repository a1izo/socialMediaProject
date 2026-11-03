using ApiContracts;

namespace BlazorApp.Services;

public interface ICommentService
{
    public Task<CommentDto> AddCommentAsync(CreateCommentDto request);
    public Task UpdateCommentAsync(int id, UpdateCommentDto request);
    public Task DeleteCommentAsync(int id);
    public Task<List<CommentDto>> GetCommentsForPostAsync(int postId);
    public Task LikeAsync(int commentId, int userId);
    public Task UnlikeAsync(int commentId, int userId);
}
