using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageComments;

public class CreateCommentView
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;
    private readonly ICommentRepository commentRepository;

    public CreateCommentView(
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
        Console.WriteLine("Add comment");

        Console.Write("Post ID: ");
        string? postIdInput = Console.ReadLine();
        Console.Write("User ID: ");
        string? userIdInput = Console.ReadLine();
        Console.Write("Reply to comment ID (leave empty for none): ");
        string? parentIdInput = Console.ReadLine();
        Console.Write("Comment: ");
        string? body = Console.ReadLine();

        if (!int.TryParse(postIdInput, out int postId) || !int.TryParse(userIdInput, out int userId))
        {
            Console.WriteLine("Post ID and user ID must be numbers.");
            return;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            Console.WriteLine("Comment is required.");
            return;
        }

        if (!postRepository.GetMany().Any(p => p.Id == postId))
        {
            Console.WriteLine($"Post with ID {postId} does not exist.");
            return;
        }

        if (!userRepository.GetMany().Any(u => u.Id == userId))
        {
            Console.WriteLine($"User with ID {userId} does not exist.");
            return;
        }

        int? parentCommentId = null;
        if (!string.IsNullOrWhiteSpace(parentIdInput))
        {
            if (!int.TryParse(parentIdInput, out int parsedParentId))
            {
                Console.WriteLine("Reply comment ID must be a number.");
                return;
            }

            bool parentValid = commentRepository.GetMany()
                .Any(c => c.Id == parsedParentId && c.PostId == postId);
            if (!parentValid)
            {
                Console.WriteLine($"Comment {parsedParentId} does not exist on post {postId}.");
                return;
            }

            parentCommentId = parsedParentId;
        }

        Comment comment = new()
        {
            Body = body,
            UserId = userId,
            PostId = postId,
            ParentCommentId = parentCommentId,
            CreatedDate = DateTime.Now
        };

        Comment created = await commentRepository.AddAsync(comment);
        Console.WriteLine($"Comment created with ID {created.Id}.");
    }
}