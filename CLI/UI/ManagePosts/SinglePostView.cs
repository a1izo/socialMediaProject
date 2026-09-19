using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class SinglePostView
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;
    private readonly ICommentRepository commentRepository;

    public SinglePostView(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ICommentRepository commentRepository)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
        this.commentRepository = commentRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("View post");
        Console.Write("Post ID: ");
        string? postIdInput = Console.ReadLine();

        if (!int.TryParse(postIdInput, out int postId))
        {
            Console.WriteLine("Post ID must be a number.");
            return;
        }

        Post? post = postRepository.GetMany().SingleOrDefault(p => p.Id == postId);
        if (post is null)
        {
            Console.WriteLine($"Post with ID {postId} does not exist.");
            return;
        }

        int likeCount = await postRepository.GetLikeCountAsync(post.Id);

        Console.WriteLine();
        Console.WriteLine(post.Title);
        Console.WriteLine($"By {GetUserName(post.UserId)} on {post.CreatedDate:yyyy-MM-dd HH:mm}");
        Console.WriteLine();
        Console.WriteLine(post.Body);
        Console.WriteLine();
        Console.WriteLine($"Likes: {likeCount}");

        List<Comment> comments = commentRepository.GetMany()
            .Where(c => c.PostId == post.Id)
            .OrderBy(c => c.CreatedDate)
            .ToList();

        Console.WriteLine();
        Console.WriteLine($"Comments ({comments.Count})");
        foreach (Comment comment in comments)
        {
            Console.WriteLine($"[{comment.Id}] {GetUserName(comment.UserId)}: {comment.Body}");
        }
    }

    private string GetUserName(int userId)
    {
        User? user = userRepository.GetMany().SingleOrDefault(u => u.Id == userId);
        return user is null ? $"unknown user {userId}" : user.UserName;
    }
}