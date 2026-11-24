using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository commentRepo;
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;
    private readonly PostDtoBuilder dtoBuilder;
    private readonly Notifier notifier;

    public CommentsController(ICommentRepository commentRepo, IPostRepository postRepo, IUserRepository userRepo,
        PostDtoBuilder dtoBuilder, Notifier notifier)
    {
        this.commentRepo = commentRepo;
        this.postRepo = postRepo;
        this.userRepo = userRepo;
        this.dtoBuilder = dtoBuilder;
        this.notifier = notifier;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment([FromBody] CreateCommentDto request)
    {
        DtoMapper.RequireNotBlank(request.Body, "Body");

        User? author = await userRepo.GetMany().SingleOrDefaultAsync(u => u.Id == request.UserId);
        if (author is null)
        {
            throw new ArgumentException($"User with ID '{request.UserId}' does not exist.");
        }

        Post? post = await postRepo.GetMany().SingleOrDefaultAsync(p => p.Id == request.PostId);
        if (post is null)
        {
            throw new ArgumentException($"Post with ID '{request.PostId}' does not exist.");
        }

        Comment? parent = null;
        if (request.ParentCommentId is not null)
        {
            parent = await commentRepo.GetMany().SingleOrDefaultAsync(c => c.Id == request.ParentCommentId);
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

        if (parent is not null)
        {
            await notifier.NotifyAsync(parent.UserId, request.UserId, Notifier.Reply, post.Id, created.Id);
        }

        if (parent is null || parent.UserId != post.UserId)
        {
            await notifier.NotifyAsync(post.UserId, request.UserId, Notifier.Comment, post.Id, created.Id);
        }
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
        return Ok(await dtoBuilder.BuildCommentAsync(id));
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
            comments = comments.Where(c => c.User.UserName.ToLower() == userName.ToLower());
        }

        if (postId is not null)
        {
            comments = comments.Where(c => c.PostId == postId);
        }

        return Ok(await dtoBuilder.BuildCommentsAsync(comments.OrderBy(c => c.CreatedDate)));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment([FromRoute] int id)
    {
        // the database removes the replies (and their replies), likes and notifications with it
        await commentRepo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/replies")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetReplies([FromRoute] int id)
    {
        await commentRepo.GetSingleAsync(id);

        return Ok(await dtoBuilder.BuildCommentsAsync(commentRepo.GetReplies(id).OrderBy(c => c.CreatedDate)));
    }

    [HttpPost("{id:int}/likes")]
    public async Task<ActionResult> LikeComment([FromRoute] int id, [FromBody] LikeDto request)
    {
        Comment comment = await commentRepo.GetSingleAsync(id);
        bool userExists = await userRepo.GetMany().AnyAsync(u => u.Id == request.UserId);
        if (!userExists)
        {
            throw new ArgumentException($"User with ID '{request.UserId}' does not exist.");
        }

        await commentRepo.LikeAsync(id, request.UserId);
        await notifier.NotifyAsync(comment.UserId, request.UserId, Notifier.CommentLike, comment.PostId, comment.Id);
        return NoContent();
    }

    [HttpDelete("{id:int}/likes/{userId:int}")]
    public async Task<ActionResult> UnlikeComment([FromRoute] int id, [FromRoute] int userId)
    {
        Comment comment = await commentRepo.GetSingleAsync(id);
        await commentRepo.UnlikeAsync(id, userId);
        await notifier.RetractAsync(comment.UserId, userId, Notifier.CommentLike, comment.PostId, comment.Id);
        return NoContent();
    }
}
