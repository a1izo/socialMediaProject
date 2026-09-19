using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class UpdatePostView
{
    private readonly IPostRepository postRepository;

    public UpdatePostView(IPostRepository postRepository)
    {
        this.postRepository = postRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("Update post");
        Console.Write("Post ID: ");
        string? idInput = Console.ReadLine();

        if (!int.TryParse(idInput, out int id))
        {
            Console.WriteLine("Post ID must be a number.");
            return;
        }

        Post? post = postRepository.GetMany().SingleOrDefault(p => p.Id == id);
        if (post is null)
        {
            Console.WriteLine($"Post with ID {id} does not exist.");
            return;
        }

        Console.WriteLine("Leave a field empty to keep the current value.");

        Console.Write($"Title ({post.Title}): ");
        string? title = Console.ReadLine();
        Console.Write($"Body ({post.Body}): ");
        string? body = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(title)) post.Title = title;
        if (!string.IsNullOrWhiteSpace(body)) post.Body = body;

        await postRepository.UpdateAsync(post);
        Console.WriteLine("Post updated.");
    }
}