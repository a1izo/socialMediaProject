using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly ITagRepository tagRepo;
    private readonly PostDtoBuilder postDtoBuilder;
    private readonly Notifier notifier;

    public PostsController(IPostRepository postRepo, IUserRepository userRepo,
        ITagRepository tagRepo, PostDtoBuilder postDtoBuilder, Notifier notifier)
    {
        this.postRepo = postRepo;
        this.userRepo = userRepo;
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

        await VerifyUserExistsAsync(request.UserId);

        Post post = new()
        {
            Title = request.Title?.Trim() ?? "",
            Body = request.Body?.Trim() ?? "",
            UserId = request.UserId,
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

        PostDto dto = await postDtoBuilder.BuildAsync(created.Id);
        return Created($"/posts/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdatePost([FromRoute] int id, [FromBody] UpdatePostDto request)
    {
        Post post = await postRepo.GetSingleAsync(id);
        bool hasMedia = await postRepo.GetMedia(id).AnyAsync();
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
        PostDto dto = await postDtoBuilder.BuildAsync(id);
        if (includeComments)
        {
            dto.Comments = await postDtoBuilder.GetCommentsForPostAsync(id);
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
            posts = posts.Where(p => p.User.UserName.ToLower() == userName.ToLower());
        }

        if (tagId is not null)
        {
            posts = posts.Where(p => p.PostTags.Any(pt => pt.TagId == tagId));
        }

        if (followedBy is not null)
        {
            posts = posts.Where(p => p.User.Followers.Any(f => f.FollowerId == followedBy));
        }

        posts = orderBy?.ToLower() switch
        {
            null or "" or "newest" => posts.OrderByDescending(p => p.CreatedDate),
            "oldest" => posts.OrderBy(p => p.CreatedDate),
            "likes" => posts.OrderByDescending(p => p.Likes.Count).ThenByDescending(p => p.CreatedDate),
            _ => throw new ArgumentException("orderBy must be one of: newest, oldest, likes.")
        };

        if (take is not null)
        {
            if (take <= 0)
            {
                throw new ArgumentException("take must be a positive number.");
            }

            posts = posts.Take(take.Value);
        }

        return Ok(await postDtoBuilder.BuildManyAsync(posts));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePost([FromRoute] int id)
    {
        // the database removes the post's comments, likes, tags, media and notifications with it
        await postRepo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForPost([FromRoute] int id)
    {
        if (!await postRepo.GetMany().AnyAsync(p => p.Id == id))
        {
            throw new NotFoundException($"Post with ID '{id}' not found");
        }

        return Ok(await postDtoBuilder.GetCommentsForPostAsync(id));
    }

    [HttpPost("{id:int}/likes")]
    public async Task<ActionResult> LikePost([FromRoute] int id, [FromBody] LikeDto request)
    {
        Post post = await postRepo.GetSingleAsync(id);
        await VerifyUserExistsAsync(request.UserId);
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
        List<Tag> tags = await postRepo.GetTagsForPost(id).ToListAsync();
        return Ok(tags.Select(DtoMapper.ToDto).ToList());
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

    private async Task VerifyUserExistsAsync(int userId)
    {
        if (!await userRepo.GetMany().AnyAsync(u => u.Id == userId))
        {
            throw new ArgumentException($"User with ID '{userId}' does not exist.");
        }
    }
}
