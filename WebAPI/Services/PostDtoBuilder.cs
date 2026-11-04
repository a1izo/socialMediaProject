using ApiContracts;
using Entities;
using RepositoryContracts;

namespace WebAPI.Services;

// Builds the full PostDto (likes, tags, media, comment count). Used by the posts and search controllers.
public class PostDtoBuilder
{
    private readonly IPostRepository postRepo;
    private readonly ICommentRepository commentRepo;
    private readonly IUserRepository userRepo;

    public PostDtoBuilder(IPostRepository postRepo, ICommentRepository commentRepo, IUserRepository userRepo)
    {
        this.postRepo = postRepo;
        this.commentRepo = commentRepo;
        this.userRepo = userRepo;
    }

    public Dictionary<int, string> GetUserNames()
    {
        return userRepo.GetMany().ToDictionary(u => u.Id, u => u.UserName);
    }

    public static string UserNameOf(int userId, Dictionary<int, string> userNames)
    {
        return userNames.GetValueOrDefault(userId, DtoMapper.DeletedUserName);
    }

    public async Task<PostDto> BuildAsync(Post post, Dictionary<int, string> userNames)
    {
        PostDto dto = DtoMapper.ToDto(post, UserNameOf(post.UserId, userNames),
            await postRepo.GetLikeCountAsync(post.Id));
        dto.LikedByUserIds = postRepo.GetLikerIds(post.Id).ToList();
        dto.Tags = postRepo.GetTagsForPost(post.Id).ToList().Select(DtoMapper.ToDto).ToList();
        dto.Media = postRepo.GetMedia(post.Id).ToList().Select(DtoMapper.ToDto).ToList();
        dto.CommentCount = commentRepo.GetMany().Count(c => c.PostId == post.Id);
        return dto;
    }

    public async Task<CommentDto> BuildCommentAsync(Comment comment, Dictionary<int, string> userNames)
    {
        CommentDto dto = DtoMapper.ToDto(comment, UserNameOf(comment.UserId, userNames),
            await commentRepo.GetLikeCountAsync(comment.Id));
        dto.LikedByUserIds = commentRepo.GetLikerIds(comment.Id).ToList();
        return dto;
    }

    public async Task<List<CommentDto>> GetCommentsForPostAsync(int postId, Dictionary<int, string> userNames)
    {
        List<Comment> comments = commentRepo.GetMany()
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedDate)
            .ToList();

        List<CommentDto> dtos = new();
        foreach (Comment comment in comments)
        {
            dtos.Add(await BuildCommentAsync(comment, userNames));
        }

        return dtos;
    }
}
