using ApiContracts;

namespace BlazorApp.Services;

public interface IPostService
{
    public Task<PostDto> AddPostAsync(CreatePostDto request);
    public Task UpdatePostAsync(int id, UpdatePostDto request);
    public Task<PostDto> GetPostAsync(int id, bool includeComments = false);
    public Task<List<PostDto>> GetPostsAsync(PostQuery? query = null);
    public Task DeletePostAsync(int id);
    public Task LikeAsync(int postId, int userId);
    public Task UnlikeAsync(int postId, int userId);
    public Task AddTagAsync(int postId, int tagId);
    public Task RemoveTagAsync(int postId, int tagId);
}
