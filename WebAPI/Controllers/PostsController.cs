using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private const int MaxMedia = 4;

    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;
    private readonly ICommentRepository commentRepo;
    private readonly ITagRepository tagRepo;
    private readonly PostDtoBuilder postDtoBuilder;
    private readonly Notifier notifier;

    public PostsController(IPostRepository postRepo, IUserRepository userRepo,
        ICommentRepository commentRepo, ITagRepository tagRepo, PostDtoBuilder postDtoBuilder, Notifier notifier)
    {
        this.postRepo = postRepo;
        this.userRepo = userRepo;
        this.commentRepo = commentRepo;
        this.tagRepo = tagRepo;
        this.postDtoBuilder = postDtoBuilder;
        this.notifier = notifier;
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> AddPost([FromBody] CreatePostDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Body) && request.Media.Count == 0)
        {
            throw new ArgumentException("A post needs some text or an attachment.");
        }

        if (request.Media.Count > MaxMedia)
        {
            throw new ArgumentException($"A post can have at most {MaxMedia} attachments.");
        }

        foreach (PostMediaDto media in request.Media)
        {
            MediaUrls.Validate(media.Url, media.Kind);
        }

        User author = GetExistingUser(request.UserId);

        Post post = new()
        {
            Title = request.Title?.Trim() ?? "",
            Body = request.Body?.Trim() ?? "",
            UserId = author.Id,
            CreatedDate = DateTime.Now
        };
        Post created = await postRepo.AddAsync(post);

        for (int i = 0; i < request.Media.Count; i++)
        {
            await postRepo.AddMediaAsync(new PostMedia
            {
                PostId = created.Id,
                Url = request.Media[i].Url,
                Kind = request.Media[i].Kind,
                Order = i
            });
        }

        PostDto dto = await postDtoBuilder.BuildAsync(created, postDtoBuilder.GetUserNames());
        return Created($"/posts/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdatePost([FromRoute] int id, [FromBody] UpdatePostDto request)
    {
        Post post = await postRepo.GetSingleAsync(id);
        bool hasMedia = postRepo.GetMedia(id).Any();
        if (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Body) && !hasMedia)
        {
            throw new ArgumentException("A post needs some text or an attachment.");
        }

        post.Title = request.Title?.Trim() ?? "";
        post.Body = request.Body?.Trim() ?? "";
        await postRepo.UpdateAsync(post);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetSinglePost([FromRoute] int id, [FromQuery] bool includeComments = false)
    {
        Post post = await postRepo.GetSingleAsync(id);
        Dictionary<int, string> userNames = postDtoBuilder.GetUserNames();

        PostDto dto = await postDtoBuilder.BuildAsync(post, userNames);
        if (includeComments)
        {
            dto.Comments = await postDtoBuilder.GetCommentsForPostAsync(post.Id, userNames);
        }

        return Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetManyPosts(
        [FromQuery] string? titleContains,
        [FromQuery] int? userId,
        [FromQuery] string? userName,
        [FromQuery] string? orderBy,
        [FromQuery] int? take,
        [FromQuery] int? tagId,
        [FromQuery] int? followedBy)
    {
        IQueryable<Post> posts = postRepo.GetMany();

        if (!string.IsNullOrWhiteSpace(titleContains))
        {
            posts = posts.Where(p => p.Title.ToLower().Contains(titleContains.ToLower()));
        }

        if (userId is not null)
        {
            posts = posts.Where(p => p.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            List<int> matchingUserIds = userRepo.GetMany()
                .Where(u => u.UserName.ToLower() == userName.ToLower())
                .Select(u => u.Id)
                .ToList();
            posts = posts.Where(p => matchingUserIds.Contains(p.UserId));
        }

        if (tagId is not null)
        {
            posts = posts.Where(p => postRepo.GetTagsForPost(p.Id).Any(t => t.Id == tagId));
        }

        if (followedBy is not null)
        {
            List<int> followingIds = userRepo.GetFollowing(followedBy.Value).Select(u => u.Id).ToList();
            posts = posts.Where(p => followingIds.Contains(p.UserId));
        }

        Dictionary<int, string> userNames = postDtoBuilder.GetUserNames();
        List<PostDto> dtos = new();
        foreach (Post post in posts.ToList())
        {
            dtos.Add(await postDtoBuilder.BuildAsync(post, userNames));
        }

        IEnumerable<PostDto> result = orderBy?.ToLower() switch
        {
            null or "" or "newest" => dtos.OrderByDescending(p => p.CreatedDate),
            "oldest" => dtos.OrderBy(p => p.CreatedDate),
            "likes" => dtos.OrderByDescending(p => p.LikeCount).ThenByDescending(p => p.CreatedDate),
            _ => throw new ArgumentException("orderBy must be one of: newest, oldest, likes.")
        };

        if (take is not null)
        {
            if (take <= 0)
            {
                throw new ArgumentException("take must be a positive number.");
            }

            result = result.Take(take.Value);
        }

        return Ok(result.ToList());
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePost([FromRoute] int id)
    {
        await postRepo.DeleteAsync(id);

        List<int> commentIds = commentRepo.GetMany().Where(c => c.PostId == id).Select(c => c.Id).ToList();
        foreach (int commentId in commentIds)
        {
            await commentRepo.DeleteAsync(commentId);
        }

        return NoContent();
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForPost([FromRoute] int id)
    {
        await postRepo.GetSingleAsync(id);
        return Ok(await postDtoBuilder.GetCommentsForPostAsync(id, postDtoBuilder.GetUserNames()));
    }

    [HttpPost("{id:int}/likes")]
    public async Task<ActionResult> LikePost([FromRoute] int id, [FromBody] LikeDto request)
    {
        Post post = await postRepo.GetSingleAsync(id);
        GetExistingUser(request.UserId);
        await postRepo.LikeAsync(id, request.UserId);
        await notifier.NotifyAsync(post.UserId, request.UserId, Notifier.Like, postId: id);
        return NoContent();
    }

    [HttpDelete("{id:int}/likes/{userId:int}")]
    public async Task<ActionResult> UnlikePost([FromRoute] int id, [FromRoute] int userId)
    {
        Post post = await postRepo.GetSingleAsync(id);
        await postRepo.UnlikeAsync(id, userId);
        await notifier.RetractAsync(post.UserId, userId, Notifier.Like, postId: id);
        return NoContent();
    }

    [HttpGet("{id:int}/tags")]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTagsForPost([FromRoute] int id)
    {
        await postRepo.GetSingleAsync(id);
        List<TagDto> dtos = postRepo.GetTagsForPost(id).ToList().Select(DtoMapper.ToDto).ToList();
        return Ok(dtos);
    }

    [HttpPost("{id:int}/tags/{tagId:int}")]
    public async Task<ActionResult> AddTagToPost([FromRoute] int id, [FromRoute] int tagId)
    {
        await postRepo.GetSingleAsync(id);
        await tagRepo.GetSingleAsync(tagId);
        await postRepo.AddTagAsync(id, tagId);
        return NoContent();
    }

    [HttpDelete("{id:int}/tags/{tagId:int}")]
    public async Task<ActionResult> RemoveTagFromPost([FromRoute] int id, [FromRoute] int tagId)
    {
        await postRepo.RemoveTagAsync(id, tagId);
        return NoContent();
    }

    private User GetExistingUser(int userId)
    {
        User? user = userRepo.GetMany().SingleOrDefault(u => u.Id == userId);
        if (user is null)
        {
            throw new ArgumentException($"User with ID '{userId}' does not exist.");
        }

        return user;
    }
}
