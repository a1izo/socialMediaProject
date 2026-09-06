using Entities;

namespace InMemoryRepositories;

public class PostInMemoryRepository : IPostRepository
{
    private readonly List<Post> posts = new();
    private readonly List<PostLike> postLikes = new();
    private readonly List<PostTag> postTags = new();
    private readonly List<Tag> tags = new();

    public Task<Post> AddAsync(Post post)
    {
        post.Id = posts.Any() ? posts.Max(p => p.Id) + 1 : 1;
        posts.Add(post);
        return Task.FromResult(post);
    }

    public Task UpdateAsync(Post post)
    {
        Post? existingPost = posts.SingleOrDefault(p => p.Id == post.Id);
        if (existingPost is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{post.Id}' not found");
        }

        posts.Remove(existingPost);
        posts.Add(post);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Post? postToRemove = posts.SingleOrDefault(p => p.Id == id);
        if (postToRemove is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }

        posts.Remove(postToRemove);
        return Task.CompletedTask;
    }

    public Task<Post> GetSingleAsync(int id)
    {
        Post? post = posts.SingleOrDefault(p => p.Id == id);
        if (post is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }

        return Task.FromResult(post);
    }

    public IQueryable<Post> GetMany()
    {
        return posts.AsQueryable();
    }

    public Task LikeAsync(int postId, int userId)
    {
        bool alreadyLiked = postLikes.Any(pl => pl.PostId == postId && pl.UserId == userId);
        if (alreadyLiked)
        {
            throw new InvalidOperationException(
                $"User '{userId}' already liked post '{postId}'");
        }

        PostLike like = new()
        {
            Id = postLikes.Any() ? postLikes.Max(pl => pl.Id) + 1 : 1,
            PostId = postId,
            UserId = userId,
            CreatedDate = DateTime.Now
        };
        postLikes.Add(like);

        return Task.CompletedTask;
    }

    public Task UnlikeAsync(int postId, int userId)
    {
        PostLike? like = postLikes.SingleOrDefault(pl => pl.PostId == postId && pl.UserId == userId);
        if (like is null)
        {
            throw new InvalidOperationException(
                $"User '{userId}' has not liked post '{postId}'");
        }

        postLikes.Remove(like);
        return Task.CompletedTask;
    }

    public Task<int> GetLikeCountAsync(int postId)
    {
        int count = postLikes.Count(pl => pl.PostId == postId);
        return Task.FromResult(count);
    }

    public Task AddTagAsync(int postId, int tagId)
    {
        bool alreadyTagged = postTags.Any(pt => pt.PostId == postId && pt.TagId == tagId);
        if (alreadyTagged)
        {
            throw new InvalidOperationException(
                $"Post '{postId}' already has tag '{tagId}'");
        }

        PostTag postTag = new()
        {
            Id = postTags.Any() ? postTags.Max(pt => pt.Id) + 1 : 1,
            PostId = postId,
            TagId = tagId
        };
        postTags.Add(postTag);

        return Task.CompletedTask;
    }

    public Task RemoveTagAsync(int postId, int tagId)
    {
        PostTag? postTag = postTags.SingleOrDefault(pt => pt.PostId == postId && pt.TagId == tagId);
        if (postTag is null)
        {
            throw new InvalidOperationException(
                $"Post '{postId}' does not have tag '{tagId}'");
        }

        postTags.Remove(postTag);
        return Task.CompletedTask;
    }

    public IQueryable<Tag> GetTagsForPostAsync(int postId)
    {
        List<int> tagIds = postTags.Where(pt => pt.PostId == postId).Select(pt => pt.TagId).ToList();
        return tags.Where(t => tagIds.Contains(t.Id)).AsQueryable();
    }
}