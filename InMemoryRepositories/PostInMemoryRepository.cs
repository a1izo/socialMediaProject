using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class PostInMemoryRepository : IPostRepository
{
    private readonly List<Post> posts = new();
    private readonly List<PostLike> postLikes = new();
    private readonly List<PostTag> postTags = new();
    private readonly ITagRepository tagRepository;

    public PostInMemoryRepository(ITagRepository tagRepository)
    {
        this.tagRepository = tagRepository;
        SeedData();
    }

    private void SeedData()
    {
        posts.AddRange(new[]
        {
            new Post { Id = 1, Title = "Welcome to the group chat but online", Body = "Made this forum so we stop losing memes in the group chat. Post responsibly. Or do not.", UserId = 1, CreatedDate = DateTime.Now.AddDays(-15) },
            new Post { Id = 2, Title = "Hot take: cereal is a soup", Body = "Milk, solid ingredients, a bowl and a spoon. Do the math.", UserId = 2, CreatedDate = DateTime.Now.AddDays(-12) },
            new Post { Id = 3, Title = "I have not slept since Tuesday", Body = "Send help or snacks. Preferably snacks.", UserId = 3, CreatedDate = DateTime.Now.AddDays(-8) },
            new Post { Id = 4, Title = "Would a Corolla win a fight against a zeppelin?", Body = "Asking for a friend. The friend is a Corolla.", UserId = 4, CreatedDate = DateTime.Now.AddDays(-5) },
            new Post { Id = 5, Title = "Assignment due soon, nobody panic", Body = "I said nobody panic. I am panicking a little.", UserId = 1, CreatedDate = DateTime.Now.AddDays(-3) }
        });

        postLikes.AddRange(new[]
        {
            new PostLike { Id = 1, UserId = 2, PostId = 1, CreatedDate = DateTime.Now.AddDays(-14) },
            new PostLike { Id = 2, UserId = 3, PostId = 1, CreatedDate = DateTime.Now.AddDays(-14) },
            new PostLike { Id = 3, UserId = 4, PostId = 1, CreatedDate = DateTime.Now.AddDays(-14) },
            new PostLike { Id = 4, UserId = 1, PostId = 2, CreatedDate = DateTime.Now.AddDays(-11) },
            new PostLike { Id = 5, UserId = 3, PostId = 2, CreatedDate = DateTime.Now.AddDays(-11) },
            new PostLike { Id = 6, UserId = 1, PostId = 3, CreatedDate = DateTime.Now.AddDays(-7) },
            new PostLike { Id = 7, UserId = 2, PostId = 4, CreatedDate = DateTime.Now.AddDays(-4) },
            new PostLike { Id = 8, UserId = 3, PostId = 4, CreatedDate = DateTime.Now.AddDays(-4) }
        });

        postTags.AddRange(new[]
        {
            new PostTag { Id = 1, PostId = 1, TagId = 1 },
            new PostTag { Id = 2, PostId = 2, TagId = 2 },
            new PostTag { Id = 3, PostId = 2, TagId = 4 },
            new PostTag { Id = 4, PostId = 3, TagId = 2 },
            new PostTag { Id = 5, PostId = 4, TagId = 3 },
            new PostTag { Id = 6, PostId = 4, TagId = 2 }
        });
    }

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
        bool tagExists = tagRepository.GetMany().Any(t => t.Id == tagId);
        if (!tagExists)
        {
            throw new InvalidOperationException(
                $"Tag with ID '{tagId}' not found");
        }

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

    public IQueryable<Tag> GetTagsForPost(int postId)
    {
        List<int> tagIds = postTags.Where(pt => pt.PostId == postId).Select(pt => pt.TagId).ToList();
        return tagRepository.GetMany().Where(t => tagIds.Contains(t.Id));
    }
}