using Entities;

public interface IPostRepository
{
    Task<Post> AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(int id);
    Task<Post> GetSingleAsync(int id);
    IQueryable<Post> GetMany();

    //like logic
    Task LikeAsync(int postId, int userId);
    Task UnlikeAsync(int postId, int userId);
    Task<int> GetLikeCountAsync(int postId);

    //tag logic
    Task AddTagAsync(int postId, int tagId);
    Task RemoveTagAsync(int postId, int tagId);
    IQueryable<Tag> GetTagsForPostAsync(int postId);
}