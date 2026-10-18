using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;
    private readonly ICommentRepository commentRepo;
    private readonly ITagRepository tagRepo;

    public PostsController(IPostRepository postRepo, IUserRepository userRepo,
        ICommentRepository commentRepo, ITagRepository tagRepo)
    {
        this.postRepo = postRepo;
        this.userRepo = userRepo;
        this.commentRepo = commentRepo;
        this.tagRepo = tagRepo;
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> AddPost([FromBody] CreatePostDto request)
    {
        DtoMapper.RequireNotBlank(request.Title, "Title");
        DtoMapper.RequireNotBlank(request.Body, "Body");
        User author = GetExistingUser(request.UserId);

        Post post = new()
        {
            Title = request.Title,
            Body = request.Body,
            UserId = author.Id,
            CreatedDate = DateTime.Now
        };
        Post created = await postRepo.AddAsync(post);
        PostDto dto = DtoMapper.ToDto(created, author.UserName, 0);
        return Created($"/posts/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdatePost([FromRoute] int id, [FromBody] UpdatePostDto request)
    {
        DtoMapper.RequireNotBlank(request.Title, "Title");
        DtoMapper.RequireNotBlank(request.Body, "Body");

        Post post = await postRepo.GetSingleAsync(id);
        post.Title = request.Title;
        post.Body = request.Body;
        await postRepo.UpdateAsync(post);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetSinglePost([FromRoute] int id, [FromQuery] bool includeComments = false)
    {
        Post post = await postRepo.GetSingleAsync(id);
        Dictionary<int, string> userNames = GetUserNames();

        PostDto dto = DtoMapper.ToDto(post, UserNameOf(post.UserId, userNames),
            await postRepo.GetLikeCountAsync(post.Id));
        if (includeComments)
        {
            dto.Comments = await GetCommentDtosForPostAsync(post.Id, userNames);
        }

        return Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetManyPosts(
        [FromQuery] string? titleContains,
        [FromQuery] int? userId,
        [FromQuery] string? userName,
        [FromQuery] string? orderBy,
        [FromQuery] int? take)
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

        Dictionary<int, string> userNames = GetUserNames();
        List<PostDto> dtos = new();
        foreach (Post post in posts.ToList())
        {
            dtos.Add(DtoMapper.ToDto(post, UserNameOf(post.UserId, userNames),
                await postRepo.GetLikeCountAsync(post.Id)));
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
        return Ok(await GetCommentDtosForPostAsync(id, GetUserNames()));
    }

    [HttpPost("{id:int}/likes")]
    public async Task<ActionResult> LikePost([FromRoute] int id, [FromBody] LikeDto request)
    {
        await postRepo.GetSingleAsync(id);
        GetExistingUser(request.UserId);
        await postRepo.LikeAsync(id, request.UserId);
        return NoContent();
    }

    [HttpDelete("{id:int}/likes/{userId:int}")]
    public async Task<ActionResult> UnlikePost([FromRoute] int id, [FromRoute] int userId)
    {
        await postRepo.UnlikeAsync(id, userId);
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

    private async Task<List<CommentDto>> GetCommentDtosForPostAsync(int postId, Dictionary<int, string> userNames)
    {
        List<Comment> comments = commentRepo.GetMany()
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedDate)
            .ToList();

        List<CommentDto> dtos = new();
        foreach (Comment comment in comments)
        {
            dtos.Add(DtoMapper.ToDto(comment, UserNameOf(comment.UserId, userNames),
                await commentRepo.GetLikeCountAsync(comment.Id)));
        }

        return dtos;
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

    private Dictionary<int, string> GetUserNames()
    {
        return userRepo.GetMany().ToDictionary(u => u.Id, u => u.UserName);
    }

    private static string UserNameOf(int userId, Dictionary<int, string> userNames)
    {
        return userNames.GetValueOrDefault(userId, DtoMapper.DeletedUserName);
    }
}
