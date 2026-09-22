using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class PostFileRepository : IPostRepository
{
    private readonly string postsFilePath = "posts.json";
    private readonly string postLikesFilePath = "postLikes.json";
    private readonly string postTagsFilePath = "postTags.json";
    private readonly ITagRepository tagRepository;

    public PostFileRepository(ITagRepository tagRepository)
    {
        this.tagRepository = tagRepository;
        JsonFileHelper.EnsureFileExists(postsFilePath);
        JsonFileHelper.EnsureFileExists(postLikesFilePath);
        JsonFileHelper.EnsureFileExists(postTagsFilePath);
    }

    public async Task<Post> AddAsync(Post post)
    {
        List<Post> posts = await JsonFileHelper.LoadAsync<Post>(postsFilePath);
        post.Id = posts.Any() ? posts.Max(p => p.Id) + 1 : 1;
        posts.Add(post);
        await JsonFileHelper.SaveAsync(postsFilePath, posts);
        return post;
    }

    public async Task UpdateAsync(Post post)
    {
        List<Post> posts = await JsonFileHelper.LoadAsync<Post>(postsFilePath);
        Post? existingPost = posts.SingleOrDefault(p => p.Id == post.Id);
        if (existingPost is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{post.Id}' not found");
        }

        posts.Remove(existingPost);
        posts.Add(post);
        await JsonFileHelper.SaveAsync(postsFilePath, posts);
    }

    public async Task DeleteAsync(int id)
    {
        List<Post> posts = await JsonFileHelper.LoadAsync<Post>(postsFilePath);
        Post? postToRemove = posts.SingleOrDefault(p => p.Id == id);
        if (postToRemove is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }

        posts.Remove(postToRemove);
        await JsonFileHelper.SaveAsync(postsFilePath, posts);
    }

    public async Task<Post> GetSingleAsync(int id)
    {
        List<Post> posts = await JsonFileHelper.LoadAsync<Post>(postsFilePath);
        Post? post = posts.SingleOrDefault(p => p.Id == id);
        if (post is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }

        return post;
    }

    public IQueryable<Post> GetMany()
    {
        List<Post> posts = JsonFileHelper.Load<Post>(postsFilePath);
        return posts.AsQueryable();
    }

    public async Task LikeAsync(int postId, int userId)
    {
        List<PostLike> postLikes = await JsonFileHelper.LoadAsync<PostLike>(postLikesFilePath);
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
        await JsonFileHelper.SaveAsync(postLikesFilePath, postLikes);
    }

    public async Task UnlikeAsync(int postId, int userId)
    {
        List<PostLike> postLikes = await JsonFileHelper.LoadAsync<PostLike>(postLikesFilePath);
        PostLike? like = postLikes.SingleOrDefault(pl => pl.PostId == postId && pl.UserId == userId);
        if (like is null)
        {
            throw new InvalidOperationException(
                $"User '{userId}' has not liked post '{postId}'");
        }

        postLikes.Remove(like);
        await JsonFileHelper.SaveAsync(postLikesFilePath, postLikes);
    }

    public async Task<int> GetLikeCountAsync(int postId)
    {
        List<PostLike> postLikes = await JsonFileHelper.LoadAsync<PostLike>(postLikesFilePath);
        return postLikes.Count(pl => pl.PostId == postId);
    }

    public async Task AddTagAsync(int postId, int tagId)
    {
        bool tagExists = tagRepository.GetMany().Any(t => t.Id == tagId);
        if (!tagExists)
        {
            throw new InvalidOperationException(
                $"Tag with ID '{tagId}' not found");
        }

        List<PostTag> postTags = await JsonFileHelper.LoadAsync<PostTag>(postTagsFilePath);
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
        await JsonFileHelper.SaveAsync(postTagsFilePath, postTags);
    }

    public async Task RemoveTagAsync(int postId, int tagId)
    {
        List<PostTag> postTags = await JsonFileHelper.LoadAsync<PostTag>(postTagsFilePath);
        PostTag? postTag = postTags.SingleOrDefault(pt => pt.PostId == postId && pt.TagId == tagId);
        if (postTag is null)
        {
            throw new InvalidOperationException(
                $"Post '{postId}' does not have tag '{tagId}'");
        }

        postTags.Remove(postTag);
        await JsonFileHelper.SaveAsync(postTagsFilePath, postTags);
    }

    public IQueryable<Tag> GetTagsForPost(int postId)
    {
        List<PostTag> postTags = JsonFileHelper.Load<PostTag>(postTagsFilePath);
        List<int> tagIds = postTags.Where(pt => pt.PostId == postId).Select(pt => pt.TagId).ToList();
        return tagRepository.GetMany().Where(t => tagIds.Contains(t.Id));
    }
}
