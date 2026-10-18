using ApiContracts;
using Entities;

namespace WebAPI;

internal static class DtoMapper
{
    public const string DeletedUserName = "[deleted]";

    public static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            IsModerator = user.IsModerator,
            CreatedDate = user.CreatedDate
        };
    }

    public static PostDto ToDto(Post post, string authorUserName, int likeCount)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId,
            AuthorUserName = authorUserName,
            CreatedDate = post.CreatedDate,
            LikeCount = likeCount
        };
    }

    public static CommentDto ToDto(Comment comment, string authorUserName, int likeCount)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Body = comment.Body,
            UserId = comment.UserId,
            AuthorUserName = authorUserName,
            PostId = comment.PostId,
            ParentCommentId = comment.ParentCommentId,
            CreatedDate = comment.CreatedDate,
            LikeCount = likeCount
        };
    }

    public static TagDto ToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }

    public static void RequireNotBlank(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} is required.");
        }
    }
}
