using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository commentRepo;
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;

    public CommentsController(ICommentRepository commentRepo, IPostRepository postRepo, IUserRepository userRepo)
    {
        this.commentRepo = commentRepo;
        this.postRepo = postRepo;
        this.userRepo = userRepo;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment([FromBody] CreateCommentDto request)
    {
        DtoMapper.RequireNotBlank(request.Body, "Body");

        User? author = userRepo.GetMany().SingleOrDefault(u => u.Id == request.UserId);
        if (author is null)
        {
            throw new ArgumentException($"User with ID '{request.UserId}' does not exist.");
        }

        bool postExists = postRepo.GetMany().Any(p => p.Id == request.PostId);
        if (!postExists)
        {
            throw new ArgumentException($"Post with ID '{request.PostId}' does not exist.");
        }

        if (request.ParentCommentId is not null)
        {
            Comment? parent = commentRepo.GetMany().SingleOrDefault(c => c.Id == request.ParentCommentId);
            if (parent is null || parent.PostId != request.PostId)
            {
                throw new ArgumentException(
                    $"Parent comment with ID '{request.ParentCommentId}' does not exist on post '{request.PostId}'.");
            }
        }

        Comment comment = new()
        {
            Body = request.Body,
            UserId = request.UserId,
            PostId = request.PostId,
            ParentCommentId = request.ParentCommentId,
            CreatedDate = DateTime.Now
        };
        Comment created = await commentRepo.AddAsync(comment);
        CommentDto dto = DtoMapper.ToDto(created, author.UserName, 0);
        return Created($"/comments/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateComment([FromRoute] int id, [FromBody] UpdateCommentDto request)
    {
        DtoMapper.RequireNotBlank(request.Body, "Body");

        Comment comment = await commentRepo.GetSingleAsync(id);
        comment.Body = request.Body;
        await commentRepo.UpdateAsync(comment);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetSingleComment([FromRoute] int id)
    {
        Comment comment = await commentRepo.GetSingleAsync(id);
        return Ok(await ToDtoAsync(comment, GetUserNames()));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetManyComments(
        [FromQuery] int? userId,
        [FromQuery] string? userName,
        [FromQuery] int? postId)
    {
        IQueryable<Comment> comments = commentRepo.GetMany();

        if (userId is not null)
        {
            comments = comments.Where(c => c.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            List<int> matchingUserIds = userRepo.GetMany()
                .Where(u => u.UserName.ToLower() == userName.ToLower())
                .Select(u => u.Id)
                .ToList();
            comments = comments.Where(c => matchingUserIds.Contains(c.UserId));
        }

        if (postId is not null)
        {
            comments = comments.Where(c => c.PostId == postId);
        }

        Dictionary<int, string> userNames = GetUserNames();
        List<CommentDto> dtos = new();
        foreach (Comment comment in comments.OrderBy(c => c.CreatedDate).ToList())
        {
            dtos.Add(await ToDtoAsync(comment, userNames));
        }

        return Ok(dtos);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment([FromRoute] int id)
    {
        await DeleteWithRepliesAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/replies")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetReplies([FromRoute] int id)
    {
        await commentRepo.GetSingleAsync(id);

        Dictionary<int, string> userNames = GetUserNames();
        List<CommentDto> dtos = new();
        foreach (Comment reply in commentRepo.GetReplies(id).OrderBy(c => c.CreatedDate).ToList())
        {
            dtos.Add(await ToDtoAsync(reply, userNames));
        }

        return Ok(dtos);
    }

    [HttpPost("{id:int}/likes")]
    public async Task<ActionResult> LikeComment([FromRoute] int id, [FromBody] LikeDto request)
    {
        await commentRepo.GetSingleAsync(id);
        bool userExists = userRepo.GetMany().Any(u => u.Id == request.UserId);
        if (!userExists)
        {
            throw new ArgumentException($"User with ID '{request.UserId}' does not exist.");
        }

        await commentRepo.LikeAsync(id, request.UserId);
        return NoContent();
    }

    [HttpDelete("{id:int}/likes/{userId:int}")]
    public async Task<ActionResult> UnlikeComment([FromRoute] int id, [FromRoute] int userId)
    {
        await commentRepo.UnlikeAsync(id, userId);
        return NoContent();
    }

    private async Task DeleteWithRepliesAsync(int id)
    {
        List<int> replyIds = commentRepo.GetReplies(id).Select(c => c.Id).ToList();
        await commentRepo.DeleteAsync(id);
        foreach (int replyId in replyIds)
        {
            await DeleteWithRepliesAsync(replyId);
        }
    }

    private async Task<CommentDto> ToDtoAsync(Comment comment, Dictionary<int, string> userNames)
    {
        return DtoMapper.ToDto(comment,
            userNames.GetValueOrDefault(comment.UserId, DtoMapper.DeletedUserName),
            await commentRepo.GetLikeCountAsync(comment.Id));
    }

    private Dictionary<int, string> GetUserNames()
    {
        return userRepo.GetMany().ToDictionary(u => u.Id, u => u.UserName);
    }
}
