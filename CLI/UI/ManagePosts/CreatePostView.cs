using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class CreatePostView
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;

    public CreatePostView(IPostRepository postRepository, IUserRepository userRepository)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("Create post");

        Console.Write("Title: ");
        string? title = Console.ReadLine();
        Console.Write("Body: ");
        string? body = Console.ReadLine();
        Console.Write("User ID: ");
        string? userIdInput = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
        {
            Console.WriteLine("Title and body are required.");
            return;
        }

        if (!int.TryParse(userIdInput, out int userId))
        {
            Console.WriteLine("User ID must be a number.");
            return;
        }

        bool userExists = userRepository.GetMany().Any(u => u.Id == userId);
        if (!userExists)
        {
            Console.WriteLine($"User with ID {userId} does not exist.");
            return;
        }

        Post post = new()
        {
            Title = title,
            Body = body,
            UserId = userId,
            CreatedDate = DateTime.Now
        };

        Post created = await postRepository.AddAsync(post);
        Console.WriteLine($"Post created with ID {created.Id}.");
    }
}