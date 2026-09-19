using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageComments;

public class ListCommentsView
{
    private readonly IUserRepository userRepository;
    private readonly ICommentRepository commentRepository;

    public ListCommentsView(IUserRepository userRepository, ICommentRepository commentRepository)
    {
        this.userRepository = userRepository;
        this.commentRepository = commentRepository;
    }

    public Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("List comments");
        Console.Write("Filter by post ID (leave empty for all): ");
        string? postIdInput = Console.ReadLine();
        Console.Write("Filter by user ID (leave empty for all): ");
        string? userIdInput = Console.ReadLine();

        IQueryable<Comment> query = commentRepository.GetMany();

        if (!string.IsNullOrWhiteSpace(postIdInput))
        {
            if (!int.TryParse(postIdInput, out int postId))
            {
                Console.WriteLine("Post ID must be a number.");
                return Task.CompletedTask;
            }

            query = query.Where(c => c.PostId == postId);
        }

        if (!string.IsNullOrWhiteSpace(userIdInput))
        {
            if (!int.TryParse(userIdInput, out int userId))
            {
                Console.WriteLine("User ID must be a number.");
                return Task.CompletedTask;
            }

            query = query.Where(c => c.UserId == userId);
        }

        List<Comment> comments = query.OrderBy(c => c.Id).ToList();
        if (comments.Count == 0)
        {
            Console.WriteLine("No comments found.");
            return Task.CompletedTask;
        }

        foreach (Comment comment in comments)
        {
            string reply = comment.ParentCommentId is null ? "" : $" (reply to {comment.ParentCommentId})";
            Console.WriteLine(
                $"[{comment.Id}] post {comment.PostId}, {GetUserName(comment.UserId)}{reply}: {comment.Body}");
        }

        return Task.CompletedTask;
    }

    private string GetUserName(int userId)
    {
        User? user = userRepository.GetMany().SingleOrDefault(u => u.Id == userId);
        return user is null ? $"unknown user {userId}" : user.UserName;
    }
}