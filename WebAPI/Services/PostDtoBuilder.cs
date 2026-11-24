using ApiContracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace WebAPI.Services;

// Builds full PostDtos and CommentDtos (author, likes, tags, media, comment count).
// Each call is translated into one database query through the navigation properties,
// instead of loading every related table separately. Used by the posts, comments and search controllers.
public class PostDtoBuilder
{
    private readonly IPostRepository postRepo;
    private readonly ICommentRepository commentRepo;

    public PostDtoBuilder(IPostRepository postRepo, ICommentRepository commentRepo)
    {
        this.postRepo = postRepo;
        this.commentRepo = commentRepo;
    }

    // keeps the order of the given query
    public Task<List<PostDto>> BuildManyAsync(IQueryable<Post> posts)
    {
        return posts
            .Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                UserId = p.UserId,
                AuthorUserName = p.User.UserName,
                CreatedDate = p.CreatedDate,
                LikeCount = p.Likes.Count,
                CommentCount = p.Comments.Count,
                LikedByUserIds = p.Likes.Select(l => l.UserId).ToList(),
                Tags = p.PostTags.Select(pt => new TagDto { Id = pt.Tag.Id, Name = pt.Tag.Name }).ToList(),
                Media = p.Media.OrderBy(m => m.Order).Select(m => new PostMediaDto { Url = m.Url, Kind = m.Kind }).ToList()
            })
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<PostDto> BuildAsync(int postId)
    {
        List<PostDto> dtos = await BuildManyAsync(postRepo.GetMany().Where(p => p.Id == postId));
        return dtos.SingleOrDefault() ?? throw new NotFoundException($"Post with ID '{postId}' not found");
    }

    // keeps the order of the given query
    public Task<List<CommentDto>> BuildCommentsAsync(IQueryable<Comment> comments)
    {
        return comments
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                UserId = c.UserId,
                AuthorUserName = c.User.UserName,
                PostId = c.PostId,
                ParentCommentId = c.ParentCommentId,
                CreatedDate = c.CreatedDate,
                LikeCount = c.Likes.Count,
                LikedByUserIds = c.Likes.Select(l => l.UserId).ToList()
            })
            .ToListAsync();
    }

    public async Task<CommentDto> BuildCommentAsync(int commentId)
    {
        List<CommentDto> dtos = await BuildCommentsAsync(commentRepo.GetMany().Where(c => c.Id == commentId));
        return dtos.SingleOrDefault() ?? throw new NotFoundException($"Comment with ID '{commentId}' not found");
    }

    public Task<List<CommentDto>> GetCommentsForPostAsync(int postId)
    {
        return BuildCommentsAsync(commentRepo.GetMany()
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedDate));
    }
}
